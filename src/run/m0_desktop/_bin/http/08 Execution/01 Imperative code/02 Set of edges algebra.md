# Set of edges algebra

## Legend

- **Left / Right operand** — a ZeroCode expression evaluated on the current stack; the result is an **edge set** (`OutEdges` of the execution result).
- **Yes / No** — direct effect of the operator itself (not side effects from sub-expressions on the left or right).
- **Deletes vertices** — none of these operators explicitly delete vertex objects; `~=` deletes **edges**, not `To` vertices.

## Operators Table

| Symbol | Operator name | Left operand | Right operand | Description | Copies value? | Creates vertices? | Deletes vertices? | Creates edges? | Deletes edges? |
|--------|---------------|--------------|---------------|-------------|---------------|-------------------|-------------------|----------------|----------------|
| `=` | `RedirectLeftEdgesToRightVertices` | Edge set; grouped by `(From, Meta)` | Edge set; uses each edge's `To` | For each `(From, Meta)` group on the left: **remove** all those edges, then **add** new edges with the same `From` and `Meta` pointing to every `To` on the right | No | No | No | Yes | Yes |
| `+=` | `AddLeftEdgesToRightVertices` | Edge set; grouped by `(From, Meta)` | Edge set; uses each edge's `To` | For each `(From, Meta)` on the left: **add** edges to every `To` on the right (left edges remain) | No | No | No | Yes | No |
| `+<` | `AddRightEdgesIntoLeftEdges` | Edge set; uses each edge's `To` as parent | Edge set; uses each edge's `(Meta, To)` | For every left `To` and every right edge: `left.To.AddEdge(right.Meta, right.To)` | No | No | No | Yes | No |
| `~=` | `DeleteRightVertices` | Edge set (edges to remove) | Edge set; match by `To` vertex identity | Delete left edges whose `To` equals any right edge's `To` | No | No | No | No | Yes |
| `-<` | `DeleteRightEdgesFromLeftEdges` | Edge set; uses each edge's `To` as container | Edge set; `(Meta, To)` templates | From each left `To`, delete out-edges matching any right `(Meta, To)` | No | No | No | No | Yes |
| `~<` | `DeleteRightVerticesFromLeftEdges` | Edge set; uses each edge's `To` as container | Edge set; match by `To` vertex identity | From each left `To`, delete its out-edges whose `To` equals any right `To` | No | No | No | No | Yes |
| `<-` | `SetLeftVertexesToFirstRightVertexValue` | Edge set; modifies each edge's `To` | Edge set; first edge's `To` supplies the value | Set `left.To.Value = right[0].To.Value` for all left edges | **Yes** (`.Value` only) | No | No | No | No |
| `<+<` | `AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex` | Edge set; **first** edge's `To` is target root | Edge set / subgraph root (right expression result) | Deep-copy right subgraph into first left `To` via `MoveEdgesIntoVertex_NoLinksNoBootstrap` (no links, no bootstrap vertices) | **Yes** (Values in copied subgraph) | **Yes** (new copies inside target) | No | Yes | Yes (rewire + scaffolding) |
| `<<<` | `AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex` | Edge set; **first** edge's `To` is target root | Edge set / subgraph root (right expression result) | Same as `<+<`, but `MoveEdgesIntoVertex_NoBootstrap` — **includes links** | **Yes** (Values in copied subgraph) | **Yes** (new copies inside target) | No | Yes | Yes (rewire + scaffolding) |

## Notes

### `=` vs `+=`

- **`=`** — replaces targets: removes old edges in each `(From, Meta)` group, then points to right `To` vertices.
- **`+=`** — appends new edges; existing left edges are kept.

### `~=` vs `-<` vs `~<`

- **`~=`** — deletes the **left edges themselves** that point to vertices appearing on the right.
- **`-<`** — deletes **out-edges from** each `left.To` that match right `(Meta, To)` pairs.
- **`~<`** — deletes **out-edges from** each `left.To` whose `To` equals any right `To` (by vertex identity).

### `<+<` vs `<<<`

Both operators deep-copy the right subgraph into the **first** left target vertex:

- **`<+<`** — `MoveEdgesIntoVertex_NoLinksNoBootstrap` (subgraph without links, bootstrap vertices excluded).
- **`<<<`** — `MoveEdgesIntoVertex_NoBootstrap` (subgraph **with links**, bootstrap vertices excluded).

Both create **new vertex copies** inside the target, copy `.Value` fields, and create/delete edges during rewire and scaffolding cleanup.

### `PropagateToStackExpression` (`=` only)

When the left expression inherits `PropagateToStackExpression`, new edges are added to `exe.Stack` instead of `toAdd.From`.