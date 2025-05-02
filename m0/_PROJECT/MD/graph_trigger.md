# Graph Change Trigger

## Graph 

```-0
<$GraphChangeTrigger :: "name string">
	<@$Is :: @GraphChangeTrigger>		
	<@ChangeTypeFilter :: @GraphChangeFilterEnum/value>
	<@ScopeQuery :: "query">
	<@Listener :: @ListenerFunction>
```

## GraphChangeFilterEnum

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

## create trigger Graph

```-0
<@CreateTrigger :: "name string">
	<@$Is :: @CreateTrigger>
	<@CreateIn :: "query">
	<@ChangeTypeFilter :: @GraphChangeFilterEnum/value>
	<@ScopeQuery :: "query">
	<@Listener :: @ListenerFunction>
```

## create trigger Syntax

```-0
in <query> create trigger "<name string>"
	trigger filter @<GraphChangeFilterEnum value>
	trigger query "<string>"
	trigger listener @listener_function
```