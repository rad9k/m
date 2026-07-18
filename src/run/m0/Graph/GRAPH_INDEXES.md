# Graph Indexes in m0

## Purpose

This document explains how graph indexes work in `m0`. It is written for a
reader who has not seen the implementation, the optimization history, or any
previous design discussion.

The document covers:

- the physical edge collections stored on a vertex;
- the logical outgoing-edge view produced by source inheritance;
- every outgoing and incoming edge-index family;
- key normalization and zero/one/many result representation;
- lazy rebuilds, shared rebuilds, generation-based staleness checks, and
  incremental updates;
- invalidation after edge, value, inheritance, transaction, persistence, and
  filesystem changes;
- special behavior for ZeroCode stacks, filesystem overlays, file content, and
  `$NoInherit`;
- the store identifier registry and query parse cache, which are lookup
  structures but are not edge indexes;
- concrete examples and common mistakes.

The primary implementation is in:

- `m0/Graph/EasyVertex.cs`
- `m0/Graph/VertexBase.cs`
- `m0/Graph/Internal/EdgeDictionaries.cs`
- `m0/Graph/Internal/OutList.cs`
- `m0/Graph/Internal/InList.cs`
- `m0/Graph/Internal/MetaInList.cs`
- `m0/Graph/Internal/EdgeBucket.cs`
- `m0/Graph/EdgeQueryResult.cs`
- `m0/Graph/GraphUtil.cs`

## 1. The graph model in one page

`m0` uses a Meta-Vertex Edge Graph. An edge has three vertex references:

```text
From --[Meta]--> To
```

For example:

```text
Person-1 --[Name]--> "Alice"
```

Here:

- `Person-1` is the source or `From` vertex;
- `Name` is a normal vertex used as the edge's `Meta` vertex;
- `"Alice"` is the target or `To` vertex.

This is different from a conventional labeled graph. `Name` is not merely a
string label. It is a vertex and can have its own value, edges, inheritance,
and metadata.

When the edge is physically attached, the same `IEdge` object appears in three
places:

```text
Person-1.OutEdgesRaw
"Alice".InEdgesRaw
Name.MetaInEdgesRaw
```

The collections mean:

- `OutEdgesRaw`: physical edges whose `From` is this vertex;
- `InEdgesRaw`: physical edges whose `To` is this vertex;
- `MetaInEdgesRaw`: physical edges whose `Meta` is this vertex;
- `OutEdges`: the logical outgoing view, which can additionally include edges
  inherited from source vertices connected through `$Inherits`.

Incoming edges are always physical. There is no logical or inherited incoming
view.

## 2. Why indexes exist

A vertex can answer queries with two optional filters.

Outgoing:

```csharp
vertex.QueryOutEdges(meta, toValue, out result, out results);
```

Incoming:

```csharp
vertex.QueryInEdges(meta, fromValue, out result, out results);
```

Each argument can be `null`, producing four query shapes:

| Meta filter | Value filter | Outgoing behavior | Incoming behavior |
|---|---|---|---|
| non-null | null | query-meta index | direct incoming-meta index |
| null | non-null | target-value index | source-value index |
| non-null | non-null | query-meta-and-target-value index | direct-meta-and-source-value index |
| null | null | full logical outgoing scan | full physical incoming scan |

Without indexes, each filtered query would scan every edge. The indexes map
normalized keys to either one edge or an ordered group of edges.

## 3. Index inventory

### 3.1 Outgoing structures

| Structure | Source edges | Key | Meta inheritance aliases? | Lazy? |
|---|---|---|---|---|
| logical `OutEdges` snapshot | local physical plus inherited physical | none | not applicable | yes |
| direct-meta index | logical `OutEdges` | `edge.Meta.Value` | no | yes |
| query-meta index | logical `OutEdges` | own and inherited meta values | yes | yes |
| target-value index | logical `OutEdges` | `edge.To.Value` | not applicable | yes |
| query-meta-and-target-value index | logical `OutEdges` | meta alias plus `edge.To.Value` | yes | yes |

### 3.2 Incoming structures

| Structure | Source edges | Key | Meta inheritance aliases? | Lazy? |
|---|---|---|---|---|
| incoming-meta index | physical `InEdgesRaw` | `edge.Meta.Value` | no | yes |
| incoming-source-value index | physical `InEdgesRaw` | `edge.From.Value` | not applicable | yes |
| incoming-meta-and-source-value index | physical `InEdgesRaw` | direct meta plus `edge.From.Value` | no | yes |

### 3.3 Other lookup structures

The following structures are related but are not edge indexes:

- `MetaInEdgesRaw`: a reverse physical list used to find edges that use a
  vertex as metadata;
- `InheritsOutEdges` and `InheritsInEdges`: specialized physical lists used to
  traverse `$Inherits`;
- `metaQueryKeys`: a cache of the strings under which a meta vertex should be
  queryable;
- `StoreBase.VertexIdentifiersDictionary`: identifier-to-vertex lookup within
  a store;
- the bounded query parse caches: query-text-to-parsed-query lookup;
- the ZeroCode parent-frame cache: direct reference to the first
  `$StackFrameInherits` parent.

These are described later so that their relationship to edge indexes is clear.

## 4. Key normalization

Most edge-index keys are strings. The conversion rule is:

```csharp
value as string ?? value?.ToString() ?? ""
```

String comparison and hashing use ordinal semantics.

### Example: string and integer keys

Assume two target vertices:

```text
Target-A.Value = 1          // System.Int32
Target-B.Value = "1"        // System.String
```

Both normalize to:

```text
"1"
```

Therefore, a value query for integer `1` and a value query for string `"1"`
address the same bucket.

This is intentional compatibility behavior. The indexes do not preserve the
original runtime type in their keys.

### Example: custom values

If a custom object returns `"Open"` from `ToString()`, it shares a key with the
literal string `"Open"`.

### Null and empty string

The key converter maps `null` to `""`. However, `null` in the public query API
means "do not apply this filter." A caller cannot use a `null` query argument
to request only edges whose normalized value is empty.

### Combined keys

Meta-plus-value indexes use `GraphUtil.MetaAndValueKey`. It contains two
normalized strings:

```text
(normalized meta value, normalized endpoint value)
```

Its equality and hash code are ordinal for both fields.

## 5. Bucket representation and result shape

An index key can match zero, one, or many edges.

### Zero matches

The dictionary contains no key.

### One match

The dictionary stores the `IEdge` directly. No list is allocated for the
single-edge case.

### Many matches

When a second edge is added to the same key, the bucket is promoted to an
ordered `List_VertexBase` containing:

```text
[first edge, second edge]
```

Further edges are appended in encounter order.

On removal:

- many to many: remove the exact `IEdge` reference and retain the list;
- two to one: collapse the list back to a single `IEdge`;
- one to zero: remove the dictionary key.

Removal uses reference identity, not structural equality. Two edges with the
same `From`, `Meta`, and `To` are still separate objects and separate bucket
entries.

### Dedicated `EdgeBucket`

The outgoing query-meta index uses the internal `EdgeBucket` struct rather
than an `object` union. It still represents the same empty/single/many states
and preserves the same order.

### Low-level `result` and `results`

`QueryOutEdges` and `QueryInEdges` return:

| Cardinality | `result` | `results` |
|---|---|---|
| zero | `null` | `null` |
| one | the edge | `null` |
| many | `null` | ordered list |

Callers must check both outputs.

### `EdgeQueryResult`

`GraphUtil.GetQueryOutResult` and `GetQueryInResult` return a readonly
`EdgeQueryResult` struct. It provides:

- `Count`;
- `IsEmpty`;
- `FirstOrDefault`;
- indexed access;
- allocation-free struct enumeration in common cases.

For an unfiltered full scan, these helpers wrap `OutEdges` or `InEdgesRaw`
directly rather than copying them.

The older `GraphUtil.GetQueryOut` and `GetQueryIn` APIs return `IList<IEdge>`.
They allocate a new list for zero or one result. For many results they may
return the internal bucket list. Code that only needs count, existence, first,
or enumeration should prefer `EdgeQueryResult` or the dedicated
`GetQuery*Count` and `ExistQuery*` helpers.

Returned index buckets are views of current internal state. They must be
treated as read-only and short-lived across mutations.

The public backing-dictionary properties on `VertexBase`, such as
`OutEdgesByValue` and `InEdgesByMeta`, do not perform freshness checks. They
can be `null`, cold, or stale. Production callers should enter through
`QueryOutEdges`, `QueryInEdges`, `GraphUtil.GetQuery*Result`, or
`GetOutOdgesByMeta()` so graph core can validate and rebuild the required
family.

## 6. Physical collections

### 6.1 `OutEdgesRaw`

`OutEdgesRaw` is the physical source-of-truth list for outgoing edges.

For:

```text
Order-7 --[CustomerMeta]--> Customer-42
```

the exact edge object is physically present in:

```text
Order-7.OutEdgesRaw
Customer-42.InEdgesRaw
CustomerMeta.MetaInEdgesRaw
```

`OutList.OnAdd` performs reverse wiring and sends the local outgoing mutation
to `EasyVertex.HandleLocalOutEdgeMutation`.

`OutList.OnRemove` removes reverse references, updates or invalidates outgoing
indexes, and performs detach callbacks.

Graph mutations should normally use:

```csharp
source.AddEdge(meta, target);
source.DeleteEdge(edge);
```

Direct list mutation is reserved for graph-core reconstruction and specialized
stack paths.

The phrase "raw edges without inheritance" means that raw lists do not contain
logical copies inherited from ancestors. The physical inheritance relationship
itself is still an ordinary edge:

```text
Child.OutEdgesRaw contains Child --[$Inherits]--> Parent
Parent.InEdgesRaw contains that same physical edge
```

`EasyVertex` additionally keeps `InheritsOutEdges` and `InheritsInEdges` lists
to traverse those relationships efficiently.

### 6.2 `InEdgesRaw`

`InEdgesRaw` contains physical edges whose target is the vertex.

It is maintained by `InList`. Adding or removing an incoming edge marks all
incoming dictionaries dirty. Incoming indexes are not incrementally patched.

### 6.3 `MetaInEdgesRaw`

`MetaInEdgesRaw` contains physical edges that use the vertex as `Meta`.

It is primarily a reverse dependency list. For example, when the value or
inheritance of a meta vertex changes, graph core can find source vertices whose
query-meta indexes may now be stale.

There is no separate query dictionary over `MetaInEdgesRaw`.

## 7. Logical outgoing edges

`OutEdges` is not a dictionary. It is a cached logical edge sequence used as
the source for outgoing index rebuilds.

### 7.1 Vertex without source inheritance

If a vertex has no active source inheritance, logical and physical outgoing
edges are the same list:

```text
OutEdges === OutEdgesRaw
```

No copy is required.

Example:

```text
Person-1 --[Name]--> "Alice"
Person-1 --[Age]--> 30
```

`Person-1.OutEdges` is the two physical edges in the same order.

### 7.2 Vertex with source inheritance

Source inheritance is represented by:

```text
Child --[$Inherits]--> Parent
```

When `HasInheritance` and `AllowInheritance` are true, logical `OutEdges` is
built as:

1. all of `Child.OutEdgesRaw`;
2. the physical outgoing edges of each transitive parent;
3. inherited edges whose meta is marked `$NoInherit` are skipped.

The traversal uses a visited set, so a parent reached through both sides of a
diamond contributes its physical edges once.

Example:

```text
Employee --[$Inherits]--> Person
Person   --[Name]-------> "default name"
Employee --[Department]-> "Engineering"
```

Logical `Employee.OutEdges` contains:

```text
Employee --[$Inherits]--> Person
Employee --[Department]-> "Engineering"
Person   --[Name]-------> "default name"
```

The inherited `Name` edge remains the original edge object. Its `From` is
still `Person`; it is not cloned or rewritten.

### 7.3 `AllowInheritance`

If `AllowInheritance` is false, logical inheritance is disabled even if
`$Inherits` edges exist. `OutEdges` then aliases `OutEdgesRaw`.

ZeroCode stack vertices use this mode because `$StackFrameInherits` has its own
lookup semantics and is not ordinary source inheritance.

### 7.4 Diamond inheritance

Example:

```text
Base  --[Flag]---------> true
Left  --[$Inherits]---> Base
Right --[$Inherits]---> Base
Leaf  --[$Inherits]---> Left
Leaf  --[$Inherits]---> Right
```

`Leaf.OutEdges` contains the original `Base --[Flag]--> true` edge once, not
twice.

Local raw edges are always added before inherited parent groups, and edges
within one raw list retain their order. The relative order of contributions
from different inheritance parents is an implementation traversal order over a
visited set, not a stable public sorting contract. Code that requires a
domain-specific order should sort explicitly.

### 7.5 Cycles

New `$Inherits` edges that would create a cycle are rejected before physical
mutation. Traversals also retain visited or active-path protection because an
old or damaged store may already contain a cycle.

## 8. Direct-meta outgoing index

The direct-meta index is exposed by the historically named:

```csharp
GetOutOdgesByMeta()
```

The spelling is part of the existing API.

It maps:

```text
normalized edge.Meta.Value -> one or many logical outgoing edges
```

It indexes logical `OutEdges`, so it includes inherited source edges. "Direct"
means that it does not create aliases for the meta vertex's inheritance.
It does not mean "local physical edges only."

### Example

```text
source --[Email]--> "a@example.com"
source --[Email]--> "b@example.com"
source --[Phone]--> "123"
```

The direct-meta index contains:

```text
"Email" -> [email edge 1, email edge 2]
"Phone" -> phone edge
```

### Meta inheritance is intentionally ignored

Assume:

```text
WorkEmail --[$Inherits]--> Email
source    --[WorkEmail]--> "a@example.com"
```

The direct-meta index contains:

```text
"WorkEmail" -> edge
```

It does not contain:

```text
"Email" -> edge
```

This distinction is required by JSON and REST representations that need the
edge's actual meta category rather than query aliases.

## 9. Query-meta outgoing index

The query-meta index powers:

```csharp
source.QueryOutEdges(metaValue, null, ...);
```

It maps every queryable meta value to an `EdgeBucket`.

For each edge it adds keys for:

1. the edge's own `Meta.Value`;
2. every transitive parent value of the meta vertex.

Duplicate strings are removed.

### Example: meta inheritance

```text
WorkEmail --[$Inherits]--> Email
source    --[WorkEmail]--> "a@example.com"
```

The same edge is indexed under:

```text
"WorkEmail"
"Email"
```

Therefore:

```csharp
source.QueryOutEdges("WorkEmail", null, ...); // match
source.QueryOutEdges("Email", null, ...);     // same match
```

### Same meta value, different meta vertices

Two unrelated meta vertices can both have value `"Tag"`. A query by `"Tag"`
matches edges using either vertex because query keys are value-based.

When exact meta vertex identity is required, use
`GraphUtil.FindEdgeByMetaVertex`, which obtains candidates from the index and
then filters with `ReferenceEquals(edge.Meta, requestedMeta)`.

## 10. Outgoing target-value index

The outgoing value index powers:

```csharp
source.QueryOutEdges(null, targetValue, ...);
```

It maps:

```text
normalized edge.To.Value -> one or many logical outgoing edges
```

### Example

```text
source --[Name]--> "Alice"
source --[Alias]-> "Alice"
source --[Name]--> "Bob"
```

The index contains:

```text
"Alice" -> [Name/Alice edge, Alias/Alice edge]
"Bob"   -> Name/Bob edge
```

The meta value does not participate in this index.

## 11. Outgoing query-meta-and-target-value index

This index powers:

```csharp
source.QueryOutEdges(metaValue, targetValue, ...);
```

Its key is:

```text
(queryable meta alias, normalized edge.To.Value)
```

It uses meta inheritance aliases in the same way as the query-meta index.

### Example

```text
WorkEmail --[$Inherits]--> Email
source    --[WorkEmail]--> "a@example.com"
```

The edge is indexed under:

```text
("WorkEmail", "a@example.com")
("Email",     "a@example.com")
```

It is not indexed under:

```text
("Email", "other@example.com")
```

## 12. Incoming indexes

Incoming indexes always use physical `InEdgesRaw`. Source inheritance never
adds incoming edges to a target.

Assume:

```text
Alice --[MemberOf]--> Team-1
Bob   --[MemberOf]--> Team-1
Carol --[OwnerOf]--> Team-1
```

### 12.1 Incoming meta

Query:

```csharp
team.QueryInEdges("MemberOf", null, ...);
```

Index:

```text
"MemberOf" -> [Alice edge, Bob edge]
"OwnerOf"  -> Carol edge
```

Incoming meta matching is direct value matching. It does not add aliases for
meta inheritance.

### 12.2 Incoming source value

Query:

```csharp
team.QueryInEdges(null, "Alice", ...);
```

Index:

```text
"Alice" -> Alice edge
"Bob"   -> Bob edge
"Carol" -> Carol edge
```

The key is `edge.From.Value`, not the source identifier.

### 12.3 Incoming meta and source value

Query:

```csharp
team.QueryInEdges("MemberOf", "Alice", ...);
```

Index key:

```text
("MemberOf", "Alice")
```

Again, incoming meta matching does not use meta inheritance aliases.

### 12.4 Incoming/outgoing meta asymmetry

Assume:

```text
WorkEmail --[$Inherits]--> Email
source    --[WorkEmail]--> target
```

Then:

```csharp
source.QueryOutEdges("Email", null, ...); // matches
target.QueryInEdges("Email", null, ...);  // does not match
target.QueryInEdges("WorkEmail", null, ...); // matches
```

Outgoing query-meta matching expands meta inheritance. Incoming matching uses
only the edge's actual normalized `Meta.Value`.

## 13. Full-scan queries

When both filters are `null`:

```csharp
source.QueryOutEdges(null, null, ...);
target.QueryInEdges(null, null, ...);
```

the low-level methods return a copied list:

- outgoing: `OutEdges.ToList()`;
- incoming: `InEdgesRaw.ToList()`.

For allocation-sensitive code, use:

```csharp
GraphUtil.GetQueryOutResult(source, null, null);
GraphUtil.GetQueryInResult(target, null, null);
```

These readonly results wrap the current list directly.

### 13.1 Text queries

The ZeroCode colon form:

```text
Meta:Value
```

eventually calls outgoing `QueryOutEdges(meta, value, ...)`. It therefore uses
the same query-meta inheritance and normalized target-value indexes described
above.

The incoming slash operation is a separate physical `InEdgesRaw` traversal. It
does not turn `QueryInEdges` into an inheritance-aware incoming view.

## 14. Lazy construction and dirty flags

Indexes are built on demand.

At vertex initialization:

```text
all incoming index flags = dirty
all outgoing index flags = dirty
```

Reading one index rebuilds only what that read needs, except for the shared
logical-plus-first-index optimization described below.

The outgoing dirty families are:

```text
logical edges
direct meta
query meta
value
query meta + value
```

The incoming dirty families are:

```text
meta
source value
meta + source value
```

### Example

After creating a vertex and adding 1,000 edges:

```csharp
source.QueryOutEdges("Name", null, ...);
```

builds the query-meta index. It does not eagerly build the target-value or
meta-and-value indexes.

A later:

```csharp
source.QueryOutEdges(null, "Alice", ...);
```

builds the target-value index at that time.

The aggregate `OutEdgesDictionariesNeedsRebuild` and
`InEdgesDictionariesNeedsRebuild` properties are bulk invalidation controls:
setting one to `true` marks every family dirty. Their getters should not be
treated as an authoritative statement that every family is still dirty,
because individual lazy rebuilds clear per-family flags.

## 15. Shared logical and first-index rebuild

For an inheriting vertex, the logical edge snapshot and the first requested
outgoing index need the same edge traversal.

When both are dirty, graph core can build:

```text
logical OutEdges + exactly one requested index
```

in one pass.

This applies to the first requested:

- direct-meta index;
- query-meta index;
- value index;
- query-meta-and-value index.

Other indexes remain dirty until requested.

### Example

```text
Child --[$Inherits]--> Parent
Parent has 1,000 physical data edges
```

The first query-meta lookup on `Child` can scan those edges once to produce
both `Child.OutEdges` and the query-meta dictionary. It does not scan once for
the logical list and again for the dictionary.

Filesystem overlay vertices disable this shared path because their logical
view has an additional overlay source.

## 16. Inherited-index freshness: generations and dependency stamps

Eagerly walking every descendant after every parent mutation is expensive.
Instead, inheriting vertices validate cached parent versions when queried.

Each `EasyVertex` maintains generations for:

- outgoing structure;
- direct-meta dependencies;
- query-meta dependencies;
- value dependencies.

Each built inherited index stores a stamp containing the relevant generations
of its transitive parents.

A global inheritance dependency epoch is a fast filter:

1. if the index's checked epoch equals the global epoch, it is current;
2. otherwise, compare the stored parent dependency versions;
3. if no relevant dependency changed, update the checked epoch without a
   rebuild;
4. if a relevant dependency changed, mark only the affected index dirty;
5. if parent structure changed, mark the whole outgoing state dirty.

The global epoch does not itself make every inherited index stale. It only says
that a precise dependency check may be required.

### Example: independent hierarchies

```text
Child-A --[$Inherits]--> Parent-A
Child-B --[$Inherits]--> Parent-B
```

Changing `Parent-A` increases the global epoch. A later query on `Child-B`
checks its recorded `Parent-B` versions, sees no change, and avoids a rebuild.

### Dependency by index family

| Cached child index | Parent generations checked |
|---|---|
| logical `OutEdges` | structure |
| direct-meta | structure and direct-meta |
| query-meta | structure and query-meta |
| value | structure and value |
| query-meta-and-value | structure, query-meta, and value |

## 17. Incremental updates for local outgoing mutations

For a simple local vertex with warm indexes, a single `AddEdge` or `DeleteEdge`
can patch already-built outgoing indexes instead of invalidating and rebuilding
all of them.

The mutation handler updates only indexes that are currently built.

### Add example

Before:

```text
"Tag" -> edge A
```

Add edge B with the same key:

```text
"Tag" -> [edge A, edge B]
```

### Remove example

Before:

```text
"Tag" -> [edge A, edge B]
```

Remove exact reference edge A:

```text
"Tag" -> edge B
```

Remove edge B:

```text
key "Tag" is removed
```

### When incremental update is allowed

The current implementation requires:

- non-null meta and target;
- attached store;
- no active source inheritance on the vertex;
- no inheritance children that depend on this source;
- the edge is not `$Inherits`;
- logical edges are either dirty or alias `OutEdgesRaw`;
- the mutation path explicitly permits incremental behavior.

### When it falls back to lazy rebuild

Fallback occurs for:

- source inheritance changes;
- a vertex that inherits outgoing edges;
- a source with inheritance descendants;
- `$Inherits` edges;
- detached loader reconstruction;
- filesystem overlay vertices;
- specialized stack paths that intentionally bypass normal reverse wiring;
- any missing key or edge that makes an incremental remove inconsistent;
- a mutation burst beyond the current budget.

Fallback clears the current-index mask and marks all outgoing families dirty.
The next query rebuilds only the required state.

## 18. Adaptive mutation-burst policy

Incremental patching is best when a query follows a small mutation burst.
Repeatedly patching every warm index is wasteful for a large write-only burst.

Each vertex begins with an incremental mutation budget of one.

### First unknown burst

Example:

```text
add edge 1
add edge 2
add edge 3
query
```

The first add can be patched. Once the budget is exceeded, the vertex abandons
incremental state and marks indexes dirty. The query performs a lazy rebuild.
With the initial budget of one, the second add triggers that fallback; later
adds in the same burst only increase the observed burst size used for learning.

### Learning

After a fallback followed by a query, the vertex learns a larger budget:

```text
max(observed mutations * 2, 8), capped at 256
```

For an observed burst of 5, the learned budget is 10.

For an observed burst of 100, the learned budget is 200.

For an observed burst of 200, the learned budget is capped at 256.

### Repeated query-heavy workload

```text
add 5 edges
query
add 5 edges
query
add 5 edges
query
```

After learning, the warm indexes can be patched through the repeated bursts.

### Write-only workload

```text
add 1,000 edges
no query
```

The policy falls back early rather than spending time updating indexes that are
never read.

Tests and benchmarks can set a fixed budget internally to compare incremental
and rebuild-only strategies. Production normally uses the adaptive policy.

## 19. Invalidation matrix

### 19.1 Local edge add or remove

Normal path:

- physical lists are updated immediately;
- warm local outgoing indexes may be patched incrementally;
- the source's structure generation changes if it has inheritance children;
- incoming indexes on the target are marked dirty;
- meta reverse dependencies remain current through `MetaInEdgesRaw`.

### 19.2 Target `Value` change

If:

```text
source --[Meta]--> target
```

and `target.Value` changes, the source's:

- target-value index becomes stale;
- query-meta-and-target-value index becomes stale.

The direct-meta and query-meta indexes remain valid because the edge's meta did
not change.

If descendants inherit this source edge, the source's value generation changes.
Descendants detect it through dependency stamps on their next relevant query.

### 19.3 Source `Value` change

If a vertex's own `Value` changes, every physical target reached through its
`OutEdgesRaw` has all incoming dictionary families marked dirty. In particular:

- incoming source-value keys change;
- incoming meta-and-source-value keys change;
- the incoming meta-only family is conservatively invalidated by the shared
  master flag even though its key did not change.

The next incoming query on each affected target rebuilds only the requested
family.

### 19.4 Meta vertex `Value` change

If `Meta.Value` changes, graph core uses `MetaInEdgesRaw` to find affected
source vertices.

Affected families include:

- direct-meta, because the actual meta key changed;
- query-meta, because query aliases changed;
- meta-and-value, because its first key component changed.

The target-value-only index remains valid.

Targets of edges that directly use the renamed meta also have all incoming
families conservatively marked dirty. Their direct incoming-meta and combined
keys must reflect the new actual `Meta.Value`.

### 19.5 Meta inheritance change

If:

```text
DerivedMeta --[$Inherits]--> BaseMeta
```

is added or removed, edges using `DerivedMeta` gain or lose the `"BaseMeta"`
query alias.

Affected families:

- query-meta;
- query-meta-and-value.

The direct-meta index remains based on the edge's actual meta value.

### 19.6 Source inheritance change

Adding or removing:

```text
Child --[$Inherits]--> Parent
```

changes the logical edge set. All outgoing families on the child become dirty.

### 19.7 Incoming list change

Any physical `InEdgesRaw` add or remove marks all three incoming dictionary
families dirty. Incoming indexes are rebuilt lazily.

### 19.8 Transaction rollback

Index invalidation is immediate during each mutation. Queries inside a
transaction therefore see earlier changes in that same transaction.

Rollback applies inverse graph mutations in reverse order. Those inverse
mutations use the same list callbacks and invalidation rules, so indexes reflect
the restored graph on the next query.

Index maintenance is not delayed until commit.

## 20. `$NoInherit`

`$NoInherit` is a marker on a meta vertex. It prevents edges using that meta
from entering a child's logical outgoing view through source inheritance.

Example:

```text
Secret has marker [$NoInherit]
Parent --[Secret]------> "token"
Child  --[$Inherits]---> Parent
```

Results:

```text
Parent.OutEdges contains Secret/token
Child.OutEdges does not contain Secret/token
```

The parent's local edge is unaffected. The marker only filters inherited
copies of the edge in descendants' logical views.

Adding or removing the marker after indexes are warm finds sources through the
marked meta's `MetaInEdgesRaw`, changes their structure generation, and causes
dependent descendants to rebuild lazily.

Changing a meta vertex's value to or from `$NoInherit` also invalidates
consumers.

## 21. Filesystem overlay vertices

`AbstractFileSystemVertex` extends the logical outgoing view with generated
filesystem metadata.

Its logical `OutEdges` is:

```text
local OutEdgesRaw
+ inherited parent OutEdgesRaw filtered by $NoInherit
+ FileSystemVertex.OutEdges
```

Typical overlay edges represent values such as filename or size.

Filesystem vertices:

- disable incremental local outgoing-index updates;
- disable the shared logical-plus-first-index rebuild;
- rebuild through their overlay-aware logical `OutEdges`;
- invalidate host indexes when `Refresh` changes generated metadata.

This prevents a generic optimization from accidentally dropping overlay edges
from a query index.

## 22. File-backed content values

A file-backed `FileContentVertex.Value` reads an entire file. Reading it while
building an unrelated index would cause hidden I/O and potentially allocate a
large string.

File-backed content vertices implement `IExplicitQueryValueVertex`.

During value or meta-and-value index rebuild:

1. the file-backed endpoint is recognized;
2. its value is not read;
3. that edge is omitted from the dictionary key build;
4. the index records that explicit-value endpoints exist.

If a later query explicitly supplies a value filter, graph core scans the
relevant edges and evaluates those values at query time.

The explicit-value flag is conservative and belongs to the whole vertex index,
not to one key. If any endpoint requires explicit evaluation, a value-filtered
query uses the scan fallback rather than combining a dictionary hit with a
partial scan. After removal, the conservative flag can remain set until the
family is rebuilt; correctness is unchanged, but the fallback may do extra
work.

### Example

```text
Document --[Content]--> FileContentVertex("large.txt")
```

Query:

```csharp
document.QueryOutEdges("UnrelatedMeta", "x", ...);
```

The index build does not read `large.txt`.

Explicit query:

```csharp
document.QueryOutEdges("Content", expectedText, ...);
```

may read the file because the caller explicitly requested content equality.

This design preserves equality-query behavior while preventing implicit file
reads during unrelated index construction.

For outgoing combined queries, the fallback still applies query-meta
inheritance aliases through the same meta-key expansion used by the dictionary
path.

The same protection exists in the incoming direction. If a file-backed
`FileContentVertex` is the `From` endpoint of an edge, the target's incoming
source-value and meta-plus-source-value indexes skip that source value. An
explicit incoming value query then scans `InEdgesRaw`. Incoming meta matching
remains direct even in this fallback.

## 23. ZeroCode stack vertices

`NoInEdgeInOutVertexVertex` is an `EasyVertex` specialization used for
execution stacks.

Important differences:

- `AllowInheritance` is false;
- graph-change events are disabled;
- incoming and meta-incoming storage is lazy and normally unused;
- original `IEdge` references and duplicates can be stored without reverse
  wiring;
- a batch can append original edges and invalidate outgoing indexes once;
- stack query misses fall back through `$StackFrameInherits`;
- the first parent-frame reference is cached separately.

Normal stack-local queries still use the outgoing query indexes.

Normal stack edge-copy operations do not wire `InEdgesRaw` or
`MetaInEdgesRaw`, so incoming queries are normally empty. The lazy collections
still exist for explicit compatibility operations and tests.

### Parent shadowing example

```text
Parent frame: Local -> "parent"
Child frame:  Local -> "child"
```

A query on the child returns `"child"` and does not merge the parent bucket.

If the child has no local `Local` binding, the query walks to the cached parent
frame.

The parent-frame cache is not an edge dictionary. The
`$StackFrameInherits` edge remains the source of truth and cache invalidation
occurs on stack add/remove/batch paths.

## 24. Persistence, detach, and attach

### JSON and Binary reconstruction

Loaders reconstruct physical outgoing edges while stores or edges are detached.
Incremental index patching is disabled in this state.

After reconstruction, indexes are dirty and rebuild lazily from the restored
physical lists.

Edge dictionaries, logical snapshots, generation dependency stamps, and
incremental mutation state are not serialized. A reloaded graph reconstructs
them from physical edges on demand. After an index is warmed and the store is
attached, normal local mutations can use incremental maintenance again.

### Cross-store detach

Detach removes affected reverse incoming and meta-incoming references from
snapshots of the physical lists. Local incoming indexes are marked dirty even
when cascading removal is suppressed by detach state.

### Attach

Attach resolves both endpoints before mutation, restores reverse lists, and
causes the normal list callbacks to invalidate affected indexes. A failed
attach rolls back partial reverse wiring.

## 25. The store identifier registry

Every registered vertex belongs to a store-level dictionary:

```text
vertex.Identifier -> IVertex instance
```

This is implemented by `StoreBase.VertexIdentifiersDictionary`.

It is not an edge index. It supports persistence and lookup by stable store
identifier.

Rules:

- registering an unused identifier adds the mapping;
- registering the same instance again is idempotent;
- registering a different vertex with the same identifier throws;
- removing a vertex verifies reference ownership before deleting the key;
- ephemeral ZeroCode stack vertices can intentionally avoid registration.

Filesystem rename updates the physical path, vertex identifier, and store
registry as one controlled operation with rollback on failure.

## 26. Query parse caches

`EasyVertex.Get` and `GetAll` accept textual graph queries. Parsing those
strings produces temporary query graph vertices.

Two bounded LRU caches store parsed queries:

- regular mode: default capacity 512;
- meta mode: default capacity 128.

These are query-plan caches, not edge indexes.

They:

- use the full query string as key;
- keep regular and meta-mode entries separate;
- promote hits in LRU order;
- evict and release external references;
- do not retain parse errors;
- can be reset during bootstrap;
- use a lease so an active parsed query cannot be disposed during concurrent
  eviction or reset.

Only the string overloads `Get(bool, string)` and `GetAll(bool, string)` use
these caches. Overloads that already receive a parsed `IVertex` expression go
directly to the executer and do not perform a text-cache lookup.

## 27. Diagnostics

There is no in-process graph-index counter telemetry in production code.
Correctness is verified by contract and integration tests. Performance is
measured with BenchmarkDotNet in `m0_graph_benchmarks`.

## 28. End-to-end worked examples

### Example A: local meta query

Graph:

```text
person --[Name]--> "Alice"
person --[Age]----> 30
```

First call:

```csharp
person.QueryOutEdges("Name", null, out var one, out var many);
```

Steps:

1. complete any pending incremental burst;
2. see that query-meta is dirty;
3. build logical `OutEdges` if required;
4. build keys `"Name"` and `"Age"`;
5. return the Name edge through `one`;
6. keep `many == null`.

Second identical call uses the warm dictionary.

### Example B: one key with many edges

Graph:

```text
person --[Tag]--> "admin"
person --[Tag]--> "active"
```

Query:

```csharp
person.QueryOutEdges("Tag", null, out var one, out var many);
```

Result:

```text
one == null
many == [Tag/admin edge, Tag/active edge]
```

Order follows edge encounter order.

### Example C: source inheritance plus meta inheritance

Graph:

```text
WorkEmail --[$Inherits]--> Email
Person    --[WorkEmail]--> "a@example.com"
Employee  --[$Inherits]--> Person
```

Query:

```csharp
employee.QueryOutEdges("Email", "a@example.com", ...);
```

Steps:

1. `Employee.OutEdges` receives the original `Person` edge;
2. query-meta key generation for `WorkEmail` adds both `"WorkEmail"` and
   `"Email"`;
3. target value normalizes to `"a@example.com"`;
4. combined key `("Email", "a@example.com")` returns the inherited edge.

### Example D: target value change

Initial graph:

```text
person --[Name]--> target
target.Value = "Alice"
```

After warming all indexes:

```csharp
target.Value = "Alicia";
```

Effects:

- `"Alice"` is stale in the source value and combined indexes;
- those two outgoing families are invalidated;
- meta-only indexes remain usable;
- the next value query rebuilds or validates the relevant family;
- a query for `"Alice"` no longer matches;
- a query for `"Alicia"` matches.

### Example E: incremental local mutation

Warm simple vertex:

```text
source --[Tag]--> "A"
```

Add:

```text
source --[Tag]--> "B"
```

If the mutation is within budget:

- `OutEdgesRaw` already contains the edge;
- logical `OutEdges` aliases `OutEdgesRaw`;
- direct-meta `"Tag"` is promoted to many;
- query-meta `"Tag"` is promoted to many;
- value key `"B"` is added;
- combined key `("Tag", "B")` is added;
- no full edge scan is required.

### Example F: inherited parent mutation

```text
Child --[$Inherits]--> Parent
```

After the child indexes are warm, add a data edge to `Parent`.

Effects:

1. parent structure generation changes;
2. global epoch changes;
3. no eager traversal of every child is required;
4. the next child query sees the epoch mismatch;
5. its parent stamp shows changed structure;
6. the child rebuilds the required logical/index state and sees the edge.

### Example G: incoming lookup

```text
Alice --[MemberOf]--> Team
Bob   --[MemberOf]--> Team
```

Query:

```csharp
team.QueryInEdges("MemberOf", "Alice", out var one, out var many);
```

The incoming combined key is:

```text
("MemberOf", "Alice")
```

It returns Alice's edge only. Source inheritance on Alice does not alter
`Team.InEdgesRaw`.

### Example H: transaction read-your-writes

Inside one transaction:

```text
add Name/Alice
query -> sees Alice
change target value to Alicia
query -> sees Alicia, not Alice
delete edge
query -> sees no edge
rollback
query -> sees the pre-transaction graph
```

This works because dirty marking or incremental patching happens at mutation
time. Commit is not responsible for index visibility.

## 29. Common mistakes

### Mistake 1: treating `OutEdges` as physical

`OutEdges` may contain edges whose `From` is an ancestor. Use `OutEdgesRaw`
when persistence or physical ownership matters.

### Mistake 2: expecting inherited incoming edges

Incoming indexes use `InEdgesRaw` only. Source inheritance does not create
incoming edges on the target.

### Mistake 3: confusing direct-meta and query-meta

`GetOutOdgesByMeta()` groups by the actual `Meta.Value`.
`QueryOutEdges(meta, null)` also matches inherited meta aliases.

### Mistake 4: assuming key type is preserved

Integer `1` and string `"1"` share the same normalized key.

### Mistake 5: assuming a many-result list is a safe snapshot

Filtered many results may expose an internal bucket list. Treat it as read-only
and do not retain it across graph mutations.

### Mistake 6: manually mutating edge collections

Use `AddEdge` and `DeleteEdge`. Direct mutation can bypass transaction events,
ownership validation, or other graph lifecycle rules even when a list callback
updates indexes.

### Mistake 7: assuming every mutation triggers a rebuild immediately

Rebuilds are lazy. A mutation makes a cache dirty, changes a generation, or
patches a warm index. Work is performed only when required by the next query.

### Mistake 8: reading file content during an unrelated query

File-backed content is intentionally excluded from ordinary value-index builds.
Only an explicit equality query should evaluate it.

### Mistake 9: reading a backing dictionary directly

The `OutEdgesBy*` and `InEdgesBy*` properties do not ensure freshness. Use a
query entry point or `GetOutOdgesByMeta()`.

### Mistake 10: relying on parent-group order

Edge order inside a physical list and inside a bucket is preserved. Relative
ordering between different inheritance parents is not a sorted API contract.

### Mistake 11: using query equality for identity constraints

Query keys use normalized values and can intentionally group different objects.
For example, target cardinality validation scans physical `InEdgesRaw` and
compares the meta vertex by reference. Use identity-aware helpers or explicit
reference comparison when object identity matters.

### Mistake 12: assigning `null` to clear a value

The current `EasyVertex.Value` setter ignores `null`. No value change or index
invalidation occurs. This is existing behavior, not a special null index key.

## 30. Verification map

The main contract suites are:

- `m0_graph_tests/QueryContractTests.cs`
- `m0_graph_tests/InheritanceContractTests.cs`
- `m0_graph_tests/GenerationCounterContractTests.cs`
- `m0_graph_tests/IncrementalOutgoingIndexContractTests.cs`
- `m0_graph_tests/StackContractTests.cs`
- `m0_graph_tests/DiagnosticsContractTests.cs`
- `m0_graph_tests/TransactionContractTests.cs`
- `m0_graph_tests/QueryParseCacheContractTests.cs`
- `m0_graph_tests/StoreInvariantContractTests.cs`
- `m0_graph_tests/CrossStoreContractTests.cs`
- `m0_graph_tests/EdgeLifecycleContractTests.cs`
- `m0_graph_tests/IterativeTraversalContractTests.cs`
- `m0_graph_integration_tests/GenerationCounterIntegrationTests.cs`
- `m0_graph_integration_tests/PersistenceRoundtripTests.cs`
- `m0_graph_integration_tests/FileSystemInvariantTests.cs`
- `m0_graph_integration_tests/ZeroCodeStackIntegrationTests.cs`
- `m0_graph_integration_tests/QueryParseCacheIntegrationTests.cs`

The focused benchmark classes are:

- `QueryLookupBenchmarks`
- `MutationQueryBenchmarks`
- `InheritedTargetValueMutationBenchmarks`
- `InheritedIndexRebuildBenchmarks`
- `IncrementalOutgoingIndexBenchmarks`
- `IncrementalOutgoingBurstBenchmarks`
- `QueryResultBenchmarks`
- `FileContentIndexBenchmarks`
- `NoInheritOverlayBenchmarks`
- `StackParentFrameCacheBenchmarks`

Representative contracts include:

| Behavior | Contract |
|---|---|
| physical forward/reverse wiring | `AddEdgeWiresEveryPhysicalCollection` |
| zero/one/many outgoing results | `ReadOnlyQueryOutResultPreservesZeroOneManyAndFullScan` |
| zero/one/many incoming results | `ReadOnlyQueryInResultPreservesZeroOneManyAndFullScan` |
| parallel-edge order and identity | `QueryPreservesParallelEdgeOrderAndIdentity` |
| normalized `ToString()` equivalence | `ValueKeysPreserveCurrentToStringEquivalence` |
| direct meta versus query meta | `QueryMetaMatchesDerivedMetaButDirectMetaViewDoesNotAliasIt` |
| inherited logical edges | `LogicalOutEdgesContainParentRawEdges` |
| diamond deduplication | `DiamondInheritanceRemainsValidWithoutDuplicateBaseEdges` |
| physical-only incoming edges | `IncomingEdgesRemainPhysicalWhenTargetHasInheritance` |
| local incremental add/remove | `WarmIndexesUpdateAcrossLocalAddAndRemoveWithoutRebuild` |
| adaptive burst learning | `AdaptiveMutationBudgetLearnsRepeatedQueryBurst` |
| deferred descendant validation | `WideMutationDefersDescendantInvalidationUntilQuery` |
| independent hierarchy remains warm | `UnrelatedHierarchyRemainsWarmAfterMutation` |
| dynamic `$NoInherit` | `DynamicNoInheritMarkerInvalidatesWarmChildren` |
| file-content I/O guard | `FileBackedContentIndexBuildSkipsReadForUnrelatedQuery` |
| transaction read-your-writes | `QueryReadsEachMutationBeforeTransactionCompletion` |
| rollback of warm indexes | `RollbackRestoresChangedValueAndWarmIndexes` |
| JSON/Binary reconstruction | `StoreRoundtripPreservesGraphStructure` |
| cross-store detach/attach | `CrossStoreEdgeRestoresPhysicalLinksAfterDetachAndAttach` |
| filesystem overlay refresh | `FileRenameInvalidatesCachedOverlayMetadata` |

## 31. Summary

The indexing model can be reduced to five rules:

1. Physical lists are the graph source of truth.
2. Outgoing queries use a logical source-inheritance view; incoming queries use
   physical incoming edges only.
3. Filtered indexes are lazy and are separated by dependency: actual meta,
   query meta, endpoint value, or both.
4. Small local mutations patch warm outgoing indexes; complex or bursty
   mutations fall back to safe lazy rebuild.
5. Inherited caches use parent generation stamps for precise query-time
   freshness without eager descendant invalidation.

These rules preserve immediate query visibility, edge identity, ordering,
duplicates, inheritance semantics, persistence behavior, and safe fallbacks for
special graph implementations.
