# Views

## About

View is defined as _meta edge_. When this _meta edge_ is added to given vertex (the _given_ vertex will be called from now the **source** vertex), the view is created in the **target** vertex.

## View abstract definition

- **view**
	- type:
		- fire and forget (_> no triggers_)		
			- The view is generated when edge is added.
		- updatable (_> has to have triggers_)
	- direction:
		- source to target
		- target to source
		- both
- **source**
	- trigger query
	- transform function
- **target**	
	- trigger query
	- transform function

__After the "Possible view definitions" evaluation we come to conclusion, that just trigger queries and transform function are enough to express above.__

### transform function parameters

- event @Vertex
	- if event == ~00 than transform function is expected to generate whole source/target (so this is not a _transform_ function but a _generate_)
- from @Vertex
- to @Vertex

## Possible view definitions

|comment                       |type           |direction       |source trigger query|source transform function|target trigger query|target transform function|
|:-----------------------------|:--------------|:---------------|:-------------------|:------------------------|:-------------------|:------------------------|
|one time from source to target|fire and forget|source to target|NO                  |YES                      |NO                  |NO                       |
|updatable source to target    |updatable      |source to target|YES                 |YES                      |NO                  |NO                       |
|updatable target to source    |updatable      |target to source|NO                  |NO                       |YES                 |YES                      |
|updatable both                |updatable      |both            |YES                 |YES                      |YES                 |YES                      |

## create trigger Graph

```-0
<(?<ANY>) :: "">
	<@$Is :: CreateView>
	<@Name :: "view name">
	<@ViewInner :: "">
		<@Is :: @ViewInner>
		<@Expression :: "">
			<@Is :: @FromTriggerQuery>
			<@Query :: "query body">
		<@Expression :: "">
			<@Is :: @FromToTransformFunction>
			<@Target :: @ListenerFunction>
		<@Expression :: "">
			<@Is :: @ToTriggerQuery>
			<@Query :: "query body">
		<@Expression :: "">
			<@Is :: @ToFromTransformFunction>
			<@Target :: @ListenerFunction>

```

## create view syntax

```-0
view "trigger name" {
	from query "query body"
	from to transform @TransformFunction
	to query "query body"
	to from transform @TransformFunction
}
```

above expression will create view at local stack, so that means it can be used in following way

```-0
AddHere +< view "view name"{
	...
}
```