# Graph Change Trigger

## Graph

- `GraphChangeTrigger :: <name string>`
	- `$Is :: GraphChangeTrigger`
	- `CreateIn :: <query>`
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

## Create Syntax

	in <query> create trigger "<name string>"
		trigger filter @<GraphChangeFilterEnum value>
		trigger query "<string>"
		trigger listener @listener_function