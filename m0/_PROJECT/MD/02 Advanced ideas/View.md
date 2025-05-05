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

- events @Vertex
	- if events == ~00 than transform function is expected to generate whole source/target (so this is not a _transform_ function but a _generate_)
- from @Vertex
- to @Vertex

## Possible view definitions

|comment                       |type           |direction       |source trigger query|source transform function|target trigger query|target transform function|
|:-----------------------------|:--------------|:---------------|:-------------------|:------------------------|:-------------------|:------------------------|
|one time from source to target|fire and forget|source to target|NO                  |YES                      |NO                  |NO                       |
|updatable source to target    |updatable      |source to target|YES                 |YES                      |NO                  |NO                       |
|updatable target to source    |updatable      |target to source|NO                  |NO                       |YES                 |YES                      |
|updatable both                |updatable      |both            |YES                 |YES                      |YES                 |YES                      |

## Graph

```-0
<@$Empty :: "name string">
	<@$Is :: @CreateView>
	<@CreateIn :: expression>
	<@Source :: >
		<@TriggerQuery :: "query">
		<@TransformFunction :: @function>
	<@Target :: >
		<@TriggerQuery :: "query">
		<@TransformFunction :: @function>
```

## Create Syntax

```-0
in <query> create view "<name string>"
	view source
		view query "<string>"
		view query "<string>"
		view function @source_function
	view target
		view query "<string>"
		view query "<string>"
		view function @target_function
```