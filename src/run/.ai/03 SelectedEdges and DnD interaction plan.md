# SelectedEdges, click and DnD interaction plan

Out of scope: `UXItem.cs`, `UXVisualiser.cs`.

## Gesture model

| Event | Action |
|---|---|
| MouseDown | Record pending edge, Ctrl, `WasInSelectionAtMouseDown`. Do **not** mutate `SelectedEdges`. |
| MouseMove + drag threshold | `ApplyForDrag` → sync UI → `DoDragDrop` → `suppressNextMouseUp`. |
| MouseUp (no drag) | `ApplyForClick` → sync UI. |

Implementation: `SelectedEdgesInteractionHelper` + per-visualiser pending fields.

## Click rules (`ApplyForClick`)

| In selection at MouseDown? | Ctrl | Effect |
|---|---|---|
| no | no | clear + add clicked |
| no | yes | add clicked |
| yes | yes | remove clicked |
| yes | no | clear others, keep clicked only |

## Drag rules (`ApplyForDrag`)

| Ctrl | In selection at MouseDown? | Effect before DnD |
|---|---|---|
| yes | any (including empty) | add clicked if missing |
| no | yes | no graph change; payload = all SelectedEdges |
| no | no | clear + only clicked |

Note: without Ctrl, **click** on selected item narrows to one edge; **drag** on selected item keeps multi-selection.

## Context menu rules (`ApplyForContextMenu` on RMB down)

Not related to DnD. Applied immediately on right mouse button down, before context menu opens.

| Edge under RMB in SelectedEdges? | Effect |
|---|---|
| yes | no graph change; menu operates on current SelectedEdges |
| no (including empty SelectedEdges) | clear + only clicked edge; sync UI |

## Edge identity when matching SelectedEdges

Use `SelectedEdgeMatchMode` in `SelectedEdgesInteractionHelper`.

### `FullIEdge` (From + Meta + To)

Use `EdgeHelper.FindIEdgeVertexByIEdge` / `EdgeHelper.DeleteVertexByEdge`.

Visualisers:

- **TreeVisualiser** — done (UI sync + delete)
- **ListVisualiser** — done
- **GraphVisualiser** — done (`ToVertexOnly`, `ApplyFor*ByToVertex`)
- **IconVisualiser** — already uses `FindIEdgeVertexByIEdge`
- **FormVisualiser** — already uses `FindIEdgeVertexByIEdge`
- **InEdgesListVisualiser** — already uses `FindIEdgeVertexByIEdge`

When migrating a visualiser, audit for:

- `FindEdgeVertexByToVertex`
- `DeleteVertexByEdgeOnlyToVertex`
- `DeleteVertexByEdgeTo`
- UI sync that highlights by `To` only

Replace with full IEdge match unless listed below.

### `ToVertexOnly` (To only)

Use `EdgeHelper.FindEdgeVertexByToVertex` / `EdgeHelper.DeleteVertexByEdgeTo`.

Visualisers (intentional exception):

- **GraphVisualiser**
- **GraphVisualiser3D**

Do not switch these to `FullIEdge` without explicit product decision.

## Implementation phases

| Phase | Status | Files |
|---|---|---|
| 1 Helper | done | `SelectedEdgesInteractionHelper.cs` |
| 2 Tree + ListVisualiserHelper | done | `TreeVisualiser.cs`, `ListVisualiserHelper.cs` |
| 2b Tree FullIEdge UI sync | done | `SelectedVerticesUpdated_Reccurent`, `UpdateSelectedVertices` |
| 3 ListVisualiser | done | `ListVisualiser.cs` |
| 4 GraphVisualiser | done | `GraphVisualiser.cs`, `SelectedEdgesInteractionHelper.cs` — `ToVertexOnly` |
| 5 IconVisualiser | done | `IconVisualiser.cs` |
| 6 GraphVisualiser3D | done | `GraphVisualiser3D.cs` — keep `ToVertexOnly` |
| 7 FormVisualiser drag | done | `FormVisualiser.cs` |

Remove from drag paths: `CopySelectedVerticesToTemp`, `RestoreSelectedVertices`.

## Per-phase checklist (each visualiser)

### Interaction

- [ ] MouseDown: pending only
- [ ] MouseUp: `ApplyForClick` when not suppressed
- [ ] Drag: `ApplyForDrag` before `DoDragDrop`
- [ ] No `RestoreSelectedVertices` on drag

### Edge matching

- [ ] Confirm `SelectedEdgeMatchMode` for this visualiser
- [ ] Graph sync UI uses same mode as graph mutations
- [ ] Test: two edges same Meta+To, different From — only clicked row/node highlights (except Graph/Graph3D)

### Regression

- [ ] Hover does not change `SelectedEdges`
- [ ] Keyboard highlight independent from click/drag pending
- [ ] Tree: expand/collapse, double-click, tree→UXContainer DnD

## Manual tests (Tree)

1. Two tree rows: same Meta+To, different From — click one → only that row selected.
2. A selected, click+drag B without Ctrl → DnD carries B only.
3. A+B selected, click+drag B without Ctrl → DnD carries A+B.
4. Ctrl+drag with empty selection → DnD carries clicked edge only.
