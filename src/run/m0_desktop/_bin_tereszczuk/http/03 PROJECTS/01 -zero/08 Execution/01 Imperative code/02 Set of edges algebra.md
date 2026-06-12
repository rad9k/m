# Set of edges algebra

## All operators — Compact Table

| Symbol | Operator | Left role | Right role | Effect | Meta and To on new edges | Other changes |
|--------|----------|-----------|------------|--------|--------------------------|---------------|
| `=` | Redirect edges | Edges to replace | New target vertices | Delete left edges; create new edges from the same parent to each right target | **Meta:** left operand edge (`Meta` of removed left edge). **To:** **existing vertex** — each `To` from the right operand (reference, not a copy) | −edges (removed left), +edges |
| `+=` | Add edges | Anchor edges | Target vertices | Keep left edges; add more from the same parent and meta to each right target | **Meta:** left operand edge (`Meta` of anchor left edge). **To:** **existing vertex** — each `To` from the right operand (reference) | +edges |
| `+<` | Add into | Container vertices | Edges to insert | Add each right edge as out-edge of every left container | **Meta:** right operand edge (`right.Meta`). **To:** **existing vertex** — same `To` as the right edge (reference) | +edges |
| `~=` | Delete by To | Edges to remove | Targets for matching | Delete left edges whose target equals a right target | — | −edges |
| `-<` | Delete by (Meta, To) | Container vertices | Edge templates | Delete from each left container out-edges identified by maching on Meta and To with right edges | — | −edges |
| `~<` | Delete inner by To | Container vertices | Targets for matching | Delete inner out-edges whose target equals a right target | — | −edges |
| `<-` | Set value | Vertices to modify | Value source | Assign `.Value` from first right target to every left target | — | copies `.Value` (no new edges) |
| `<<copy<<` | Copy subgraph | Destination (each) | Source root edges | Deep-copy right subgraph under each destination; source unchanged | **Root edge** (destination → copy): **Meta** from right root edge (local **copy** of meta if meta vertex is inside copied scope; else **existing** meta vertex). **To:** **new vertex copy** of right root's `To` (`.Value` copied from source). **Inner edges:** **Meta** from source edge (local copy if in scope, else existing). **To:** **new vertex copy** if target is inside scope; **existing vertex** if target is a link or outside scope | +vertices, +edges |
| `<<move<<` | Move subgraph | Destination (each) | Source root edges | Copy as `<<copy<<`, repin externals to copies, delete source roots | **New subgraph edges:** same as `<<copy<<`. **Repinned externals:** **Meta** remapped (local copy if was inside moved scope, else existing). **To:** **new vertex copy** (matched copy in new scope) | +vertices, +edges, −edges, −vertices (source) |
| `<<copy&replace<<` | Copy and replace | Destination (each) | Source root edges | Copy source, match old subgraph, repin old→new, delete old connectors | **New subgraph:** same as `<<copy<<`. **Repins (old/external referrers):** **Meta** remapped via match map (local copy if in new scope, else existing). **To:** **new vertex copy** from match map (not the old vertex) | +vertices, +edges, −edges, −vertices (old / unmatched) |
| `<<move&replace<<` | Move and replace | Destination (each) | Source root edges | Same as copy&replace, then consume source | Same as `<<copy&replace<<` for new + repin edges; source repins also point to **new copies** | +vertices, +edges, −edges, −vertices (old + source) |
| `<+>` | Set union | Edge set A | Edge set B | New stack = all edges from A and B | **Meta:** **existing** — original `Meta` reference from each operand edge. **To:** **existing** — original `To` reference (no new graph edges; stack holds references) | stack only |
| `<->` | Set subtract | Edge set A | Edge set B | Remove from A edges matching B | — | stack only, −edge references |


## =

**Redirect Left Edges To Right Vertices**

For each (From, Meta) group on the left: remove all those edges, then add new edges with the same From and Meta pointing to every To on the right.

_example:_

## +=

**Add Left Edges To Right Vertices**

For each (From, Meta) on the left: add edges to every To on the right (left edges remain).

_example:_

## +<

**Add Right Edges Into Left Edges**

For every left To and every right edge: `left.To.AddEdge(right.Meta, right.To)`

_example:_

## ~=

**Delete Right Vertices**

Delete left edges whose To equals any right edge's To.

_example:_

## -<

**Delete Right Edges From Left Edges**

From each left To, delete out-edges matching any right (Meta, To).

_example:_

## ~<

**Delete Right Vertices From Left Edges**

From each left To, delete its out-edges whose To equals any right To.

_example:_

## <-

**Set Left Vertexes To First Right Vertex Value**

Set `left.To.Value = right[0].To.Value` for all left edges.

_example:_

## <<copy<<

**Copy Subgraph**

For each left To: deep-copy right subgraph (non-link scope) into that vertex; source subgraph stays intact.

_example:_

```ZeroCode
"Temp"
	"dest"
	"A"
		"B1"
			"C1"
			"C2"
		"B2"
			"C1"
			"C2"
	@@dest <<copy<< @@A
```

after executing above:

```ZeroCode
"Temp"
	"dest"
		"A"
			"B1"
				"C1"
				"C2"
			"B2"
				"C1"
				"C2"
	"A"
		"B1"
			"C1"
			"C2"
		"B2"
			"C1"
			"C2"
	@@dest <<copy<< @@A
```

## <<move<<

**Move Subgraph**

Like `<<copy<<`, then repin external referrers of source vertices onto copies; delete original input root edges from their `From`; source scope disposed when unreferenced.

_example:_

```ZeroCode
"Temp"
	"dest"
	"A"
		"B1"
			"C1"
			"C2"
		"B2"
			"C1"
			"C2"
	@@dest <<move<< $\:A
```

after executing above:

```ZeroCode
"Temp"
	"dest"
		"A"
			"B1"
				"C1"
				"C2"
			"B2"
				"C1"
				"C2"
	@@dest <<move<< $\:A
```

> Please mind that we are using `$\:A` here instead of `@@A`. This is because of the fact that `<<move<<` in order to delete the source sub graph anchor (here: the edge between `Temp` and `A`) edge, needs to have edge with proper `From` on the right of `<<move<<`. 
>
> Depending on certain conditions (internal mechanic of given edge set operator and possibly other operators in the processing chain) `From` part in the edge set can contain `null` or proper value. The edge set algebra is not very consistant in this area, and this is by the design - currently we are still figuring out what is the optimal balance.

## <<copy&replace<<

**Copy And Replace Subgraph**

Copy the source subgraph under the target vertex, match it to the similar old subgraph already there, redirect all outside references from the old nodes to the new copies, remove the old attachment edges, and leave the original source unchanged.

_example:_

```ZeroCode
"Temp"
	"dest"
		"A"
			"B2"
	<@dest\A\B2>
	"A"
		"B1"
			"C1"
			"C2"
		"B2"
			"C1"
			"C2"
	@@dest <<copy&replace<< @@A
```

after executing above:

```ZeroCode
"Temp"
	"dest"
		"A"
			"B1"
				"C1"
				"C2"
			"B2"
				"C1"
				"C2"
	<@dest\A\B2>
	"A"
		"B1"
			"C1"
			"C2"
		"B2"
			"C1"
			"C2"
	@@dest <<copy&replace<< @@A
```

> Mind that incoming `<@dest\A\B2>` edge has been replaced by new edge with same `From` and `Meta` but pointing to newly created vertex with a value of `B2`.

## <<move&replace<<

**Move And Replace Subgraph**

Same as `<<copy&replace<<`, plus relocate source: repin source external referrers onto new copies and delete input root edges.

_example:_

```ZeroCode
"Temp"
	"dest"
		"A"
			"B2"
	<@dest\A\B2>
	"A"
		"B1"
			"C1"
			"C2"
		"B2"
			"C1"
			"C2"
	@@dest <<move&replace<< $\:A
```

after executing above:

```ZeroCode
"Temp"
	"dest"
		"A"
			"B1"
				"C1"
				"C2"
			"B2"
				"C1"
				"C2"
	<@dest\A\B2>
	@@dest <<move&replace<< $\:A
```

> Mind that incoming `<@dest\A\B2>` edge has been replaced by new edge with same `From` and `Meta` but pointing to newly created vertex with a value of `B2`.

also:

> Please mind that we are using `$\:A` here instead of `@@A`. This is because of the fact that `<<move&replace<<` in order to delete the source sub graph anchor (here: the edge between `Temp` and `A`) edge, needs to have edge with proper `From` on the right of `<<move&replace<<`. 
>
> Depending on certain conditions (internal mechanic of given edge set operator and possibly other operators in the processing chain) `From` part in the edge set can contain `null` or proper value. The edge set algebra is not very consistant in this area, and this is by the design - currently we are still figuring out what is the optimal balance.

## <+>

**Edge Set Add**

Stack algebra: build a new edge set = union of left and right (all edges from both sides).

## <->

**Edge Set Subtract**

Stack algebra: remove from left set every edge matching a right template.

## All operators - Full operators table

| Symbol | Operator name | Left operand | Right operand | Description | Copies value? | From where value comes from? | Creates vertices? | From where the value of the new vertices come from? | Deletes vertices? | Creates edges? | From where the meta vertex for the new edges come from? | Deletes edges? | How deleted edges are identified? |
|--------|---------------|--------------|---------------|-------------|---------------|-------------------------------|-------------------|------------------------------------------------------|-------------------|----------------|----------------------------------------------------------|----------------|-------------------------------------|
| `=` | Redirect Left Edges To Right Vertices | Edge set; grouped by (From, Meta) | Edge set; uses each edge's To | For each (From, Meta) group on the left: **remove** all those edges, then **add** new edges with the same From and Meta pointing to every To on the right | No | — | No | — | No | Yes | Left edge's Meta (representative edge in each (From, Meta) group) | Yes | All left edges in the group |
| `+=` | Add Left Edges To Right Vertices | Edge set; grouped by (From, Meta) | Edge set; uses each edge's To | For each (From, Meta) on the left: **add** edges to every To on the right (left edges remain) | No | — | No | — | No | Yes | Left edge's Meta (representative edge in each (From, Meta) group) | No | — |
| `+<` | Add Right Edges Into Left Edges | Edge set; uses each edge's To as parent | Edge set; uses each edge's (Meta, To) | For every left To and every right edge: `left.To.AddEdge(right.Meta, right.To)` | No | — | No | — | No | Yes | Right edge's Meta | No | — |
| `~=` | Delete Right Vertices | Edge set (edges to remove) | Edge set; match by To vertex identity | Delete left edges whose To equals any right edge's To | No | — | No | — | No | No | — | Yes | Left edge is deleted when `left.To == right.To` (vertex identity); each right template edge used at most once per left edge |
| `-<` | Delete Right Edges From Left Edges | Edge set; uses each edge's To as container | Edge set; (Meta, To) templates | From each left To, delete out-edges matching any right (Meta, To) | No | — | No | — | No | No | — | Yes | From each left To: out-edges matched by (Meta, To) against deduplicated right templates (`EdgeKey_MetaTo`; lookup via `OutList.Get`) |
| `~<` | Delete Right Vertices From Left Edges | Edge set; uses each edge's To as container | Edge set; match by To vertex identity | From each left To, delete its out-edges whose To equals any right To | No | — | No | — | No | No | — | Yes | From each left To: out-edges whose `To == right.To` (vertex identity); each right template edge used at most once per candidate out-edge |
| `<-` | Set Left Vertexes To First Right Vertex Value | Edge set; modifies each edge's To | Edge set; first edge's To supplies the value | Set `left.To.Value = right[0].To.Value` for all left edges | **Yes** (`.Value` only) | Right operand — first edge's `To.Value` | No | — | No | No | — | No | — |
| `<<copy<<` | Copy Subgraph | Edge set; **each** edge's `To` is `copyTo` destination | Edge set; **root edges** (subgraph anchors via each edge's `To`) | For each left `To`: deep-copy right subgraph (non-link scope) into that vertex; **source subgraph stays intact** | **Yes** | Right subgraph — `source.Value` for each copied vertex | **Yes** (new copies under each left `To`) | Right subgraph — `source.Value` for each scope vertex | No | Yes | In-scope meta → copied local meta vertex; outside scope → original `edge.Meta` reference; root attachment uses root edge's Meta (remapped if in scope) | Yes | |
| `<<move<<` | Move Subgraph | Edge set; **each** edge's `To` is `moveTo` destination | Edge set; **root edges** (must carry proper `From` for anchor deletion — often `$\:A` instead of `@@A`) | Like `<<copy<<`, then repin external referrers of source vertices onto copies; delete original **input root edges** from their `From`; source scope disposed when unreferenced | **Yes** | Right subgraph — `source.Value` for each copied vertex | **Yes** (new copies under each left `To`) | Right subgraph — `source.Value` for each scope vertex | **Yes** (indirect) | Yes | Same as `<<copy<<` | Yes | Original input root edges deleted from `inputEdge.From` (`From/Meta/To` match); external in-edges / meta-in-edges deleted during repin (replaced by new edges) |
| `<<copy&replace<<` | Copy And Replace Subgraph | Edge set; **each** edge's `To` is `replaceTo` | Edge set; **root edges** (source subgraph) | Copy the source subgraph under the target vertex, match it to the similar old subgraph already there, redirect all outside references from the old nodes to the new copies, remove the old attachment edges, and leave the original source unchanged. | **Yes** | Right subgraph — `source.Value`; matched old vertices keep values until disposed | **Yes** (new copies under each left `To`) | Right subgraph — `source.Value` for each scope vertex | **Yes** (indirect — unmatched old scope) | Yes | In-scope meta → local copy; external repins use remapped meta via `oldToNew` / `sourceToNew` | Yes | Old root connectors on `replaceTo` deleted; external edges deleted during repin |
| `<<move&replace<<` | Move And Replace Subgraph | Edge set; **each** edge's `To` is `replaceTo` | Edge set; **root edges** (source subgraph) | Same as `<<copy&replace<<`, plus relocate source: repin source external referrers onto new copies and delete input root edges | **Yes** | Right subgraph — `source.Value` | **Yes** (new copies under each left `To`) | Right subgraph — `source.Value` for each scope vertex | **Yes** (indirect — old + source scope) | Yes | Same as `<<copy&replace<<` | Yes | Same as `<<copy&replace<<`, plus input root edges deleted from `inputEdge.From`; source external edges deleted during repin |
| `<+>` | Edge Set Add) | Edge set | Edge set | **Stack algebra:** build a **new** edge set = union of left and right (all edges from both sides) | No | — | No | — | No | Yes | Each new stack edge reuses original edge's `Meta` (from left or right operand) | No | — |
| `<->` | Edge Set Subtract | Edge set (base set) | Edge set (templates to remove) | **Stack algebra:** remove from left set every edge matching a right template | No | — | No | — | No | No | — | Yes | From left stack: `DeleteEdgesList(right)` — each right edge removed via `DeleteEdge` matching `From/Meta/To` on the left stack's held edges |

### Notes

#### `=` vs `+=`
- **`=`** — replaces targets: removes old edges in each `(From, Meta)` group, then points to right `To` vertices.
- **`+=`** — appends new edges; existing left edges are kept.

#### `~=` vs `-<` vs `~<`
- **`~=`** — deletes the **left edges themselves** that point to vertices appearing on the right.
- **`-<`** — deletes **out-edges from** each `left.To` that match right `(Meta, To)` pairs.
- **`~<`** — deletes **out-edges from** each `left.To` whose `To` equals any right `To` (by vertex identity).

#### `<<copy<<` vs `<<move<<` vs `<<copy&replace<<` vs `<<move&replace<<`
- **`<<copy<<`** — copy only; source subgraph unchanged.
- **`<<move<<`** — copy + consume source (delete input roots + repin external referrers).
- **`<<copy&replace<<`** — copy + reconcile with similar existing subgraph under destination (fuzzy match, repin, delete old connectors).
- **`<<move&replace<<`** — same as copy&replace, but source is also consumed.
Copy scope traverses **non-link** out-edges (`IsLink` stops traversal); link targets remain references to original vertices.
For **`<<move<<`**, the right operand should be an edge set with a proper `From` on root edges (e.g. `$\:A`) so the anchor edge can be deleted from its real `From` vertex.

#### `<+>` vs `<->` vs graph mutation operators
`<+>` and `<->` operate on **edge sets on the execution stack** (set union / subtraction). They do not rewrite the data graph the way `=`, `+=`, or `<<move<<` do — unless the stack edges themselves are live graph edges and a later operator acts on them.

#### `PropagateToStackExpression` (`=` only)
When the left expression inherits `PropagateToStackExpression`, new edges are added to `exe.Stack` instead of `toAdd.From`.


