# Graph Change Trigger

## Graph 
```ZeroCode
<@$GraphChangeTrigger :: "name string">
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
```ZeroCode
<(?<ANY>) :: "">
	<@$Is :: CreateTrigger>
	<@Name :: "trigger name">
	<@CreateTriggerInner :: "">
		<@Is :: @CreateTriggerInner>
		<@Expression :: "">
			<@Is :: @ScopeQuery>
			<@Query :: "query body">
		<@Expression :: "">
			<@Is :: @ChangeTypeFilter>
			<@Value :: @ChangeTypeFilterEnum\ValueChange>
		<@Expression :: "">
			<@Is :: @Listener>
			<@Target :: @ListenerFunction>

```
## create trigger syntax
```ZeroCode
create trigger "trigger name" {
	filter @@GraphChangeFilterEnum\ValueChange
	query "query body"
	listener @@ListenerFunction
}
```
above expression will create trigger at local stack, so that means it can be used in following way
```ZeroCode
AddHere +< create trigger "trigger name"{
	...
}
```