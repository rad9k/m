# Set of edges algebra

## <<copy<<

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

> Please mind that we are using `$\:A` here instead of `@@A`. This is because of the fact that `<<move<<` in order to delete the source sub graph anchor (here: the edge between `dest` and `A`) edge, needs to have edge with proper `From` on the right of `<<move<<`. 
>
> Depending on certain conditions (edge set source operator, other operators) `From` part in the edge set can contain `null` or proper value. The edge set algebra is not very consistant in this area, and this is by the design - currently we are still figuring out what is the optimal balance.

## <<copy&replace<<

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

## <<move&replace<<

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



## Legend

- **Left / Right operand** — a ZeroCode expression evaluated on the current stack; the result is an **edge set** (`OutEdges` of the execution result).
- **Yes / No** — direct effect of the operator itself (not side effects from sub-expressions on the left or right).
- **Deletes vertices** — none of these operators explicitly delete vertex objects; `~=` deletes **edges**, not `To` vertices.

## Operators Table

| Symbol | Operator name | Left operand | Right operand | Description | Copies value? | From where value comes from? | Creates vertices? | From where the value of the new vertices come from? | Deletes vertices? | Creates edges? | From where the meta vertex for the new edges come from? | Deletes edges? | How deleted edges are identified? |
|--------|---------------|--------------|---------------|-------------|---------------|-------------------------------|-------------------|------------------------------------------------------|-------------------|----------------|----------------------------------------------------------|----------------|-------------------------------------|
| `=` | Redirect Left Edges To Right Vertices | Edge set; grouped by (From, Meta) | Edge set; uses each edge's To | For each (From, Meta) group on the left: **remove** all those edges, then **add** new edges with the same From and Meta pointing to every To on the right | No | — | No | — | No | Yes | Left edge's Meta (representative edge in each (From, Meta) group) | Yes | All left edges in the group |
| `+=` | Add Left Edges To Right Vertices | Edge set; grouped by (From, Meta) | Edge set; uses each edge's To | For each (From, Meta) on the left: **add** edges to every To on the right (left edges remain) | No | — | No | — | No | Yes | Left edge's Meta (representative edge in each (From, Meta) group) | No | — |
| `+<` | Add Right Edges Into Left Edges | Edge set; uses each edge's To as parent | Edge set; uses each edge's (Meta, To) | For every left To and every right edge: left.To.AddEdge(right.Meta, right.To) | No | — | No | — | No | Yes | Right edge's Meta | No | — |
| ~`=` | Delete Right Vertices | Edge set (edges to remove) | Edge set; match by To vertex identity | Delete left edges whose To equals any right edge's To | No | — | No | — | No | No | — | Yes | Left edge is deleted when left.To == right.To (vertex identity); each right template edge used at most once per left edge |
| `-<` | Delete Right Edges From Left Edges | Edge set; uses each edge's To as container | Edge set; (Meta, To) templates | From each left To, delete out-edges matching any right (Meta, To) | No | — | No | — | No | No | — | Yes | From each left To: out-edges matched by (Meta, To) against deduplicated right templates (EdgeKey_MetaTo; lookup via OutList.Get) |
| ~`<` | Delete Right Vertices From Left Edges | Edge set; uses each edge's To as container | Edge set; match by To vertex identity | From each left To, delete its out-edges whose To equals any right To | No | — | No | — | No | No | — | Yes | From each left To: out-edges whose To == right.To (vertex identity); each right template edge used at most once per candidate out-edge |
| `<-` | Set Left Vertexes To First Right Vertex Value | Edge set; modifies each edge's To | Edge set; first edge's To supplies the value | Set left.To.Value = right[0].To.Value for all left edges | **Yes** (.Value only) | Right operand — first edge's To.Value | No | — | No | No | — | No | — |
| `<+<` | Add Right Edges Into First Left Edge And Set Store For SubGraph As Is In Left Vertex | Edge set; **first** edge's To is target root | Edge set / subgraph root (right expression result) | Deep-copy right subgraph into first left To via MoveEdgesIntoVertex_NoLinksNoBootstrap (no links, no bootstrap vertices) | **Yes** (Values in copied subgraph) | Right subgraph — each source vertex's .Value (root overwrites target; others copied into new vertices) | **Yes** (new copies inside target) | Right subgraph — sourceVertex.Value for each copied vertex | No | Yes | In-subgraph meta → copied local meta vertex; outside meta → original sourceEdge.Meta / sourceInEdge.Meta reference | Yes (rewire + scaffolding) | Temporary scaffolding edges from AddVertexAndReturnEdge(null, …); plus external edges replaced during rewire (DeleteEdgesList on snapshotted out-edges) |
| `<<<` | Add Right Edges Into First Left Edge And Set Store For SubGraph Including Links As Is In Left Vertex | Edge set; **first** edge's To is target root | Edge set / subgraph root (right expression result) | Same as <+<, but MoveEdgesIntoVertex_NoBootstrap — **includes links** | **Yes** (Values in copied subgraph) | Right subgraph — each source vertex's .Value (root overwrites target; others copied into new vertices) | **Yes** (new copies inside target) | Right subgraph — sourceVertex.Value for each copied vertex (includes link vertices) | No | Yes | Same as <+< (in-subgraph meta → local copy; outside meta → original reference) | Yes (rewire + scaffolding) | Same as <+< |


## Implementation notes

- **`-<`**: deletion matching uses the `(Meta, To)` pair — see `OutList.Get`, which in stack mode matches on `Meta` and `To` without requiring the same `From` in the template.
- **~`=` / `~<`**: `CreateEdgeKey_ToSet` deduplicates edges by object reference, but deletion itself compares **`To` by vertex identity** (`==`).
- **`<+<` / `<<<`**: new vertices receive the value from the corresponding source vertex; meta for new edges is either a copied local meta vertex (when meta was inside the subgraph) or the original external reference.

### `=` vs `+=`

- **`=`** — replaces targets: removes old edges in each `(From, Meta)` group, then points to right `To` vertices.
- **`+=`** — appends new edges; existing left edges are kept.

### `~=` vs `-<` vs `~<`

- **~`=`** — deletes the **left edges themselves** that point to vertices appearing on the right.
- **-`<`** — deletes **out-edges from** each `left.To` that match right `(Meta, To)` pairs.
- **~`<`** — deletes **out-edges from** each `left.To` whose `To` equals any right `To` (by vertex identity).

### `<+<` vs `<<<`

Both operators deep-copy the right subgraph into the **first** left target vertex:

- **`<+<`** — `MoveEdgesIntoVertex_NoLinksNoBootstrap` (subgraph without links, bootstrap vertices excluded).
- **`<<<`** — `MoveEdgesIntoVertex_NoBootstrap` (subgraph **with links**, bootstrap vertices excluded).

Both create **new vertex copies** inside the target, copy `.Value` fields, and create/delete edges during rewire and scaffolding cleanup.

### `PropagateToStackExpression` (`=` only)

When the left expression inherits `PropagateToStackExpression`, new edges are added to `exe.Stack` instead of `toAdd.From`.