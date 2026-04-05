# Graph Change Trigger

## Graph 

```-0
<$GraphChangeTrigger :: "name string">
	<@$Is :: @GraphChangeTrigger>		
	<@ChangeTypeFilter :: @GraphChangeFilterEnum\value>
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
<(?<ANY>) :: "">
	<@$Is :: CreateTrigger>
	<@Name :: "trigger name">
	<@TriggerInner :: "">
		<@Is :: @TriggerInner>
		<@Expression :: "">
			<@Is :: @ScopeQuery>
			<@Query :: "query body">
		<@Expression :: "">
			<@Is :: @ChangeTypeFilter>
			<@Value :: @ChangeTypeFilterEnum\Value>
		<@Expression :: "">
			<@Is :: @Listener>
			<@Target :: @ListenerFunction>

```

## create trigger syntax

```-0
trigger "trigger name" {
	filter @GraphChangeFilterEnum\value
	query "query body"
	listener @ListenerFunction
}
```

above expression will create trigger at local stack, so that means it can be used in following way

```-0
AddHere +< trigger "trigger name"{
	...
}
```