# Graph Change Trigger

## Graph

- `GraphChangeTrigger :: <name string>`
	- `$Is :: GraphChangeTrigger`
	- `ChangeTypeFilter :: <GraphChangeFilterEnum>`
	- `ScopeQuery :: <string>`
	- `Listener :: <listener function>`

### GraphChangeFilterEnum

- OnlyNonTransactedRootVertexEvents
- FilterOutRootVertexEvents
- ValueChange
- OutputEdgeAdded
- OutputEdgeRemoved
- InputEdgeAdded
- InputEdgeRemoved
- MetaEdgeAdded
- MetaEdgeRemoved
- OutputEdgeDisposed

## Syntax

	vertex +< create_trigger "<name string>"
		trigger_filter @<GraphChangeFilterEnum value>
		trigger_query "<string>"
		trigger_listener @listener_function