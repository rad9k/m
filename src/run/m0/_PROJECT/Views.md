# Views

## About

View is defined as meta edge. When this meta edge is added to given vertex (the _given_ vertex will be called from now the **source** vertex), the view is created in the **target** vertex.

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
	- generate function
	- trigger query
	- transform function
- **target**
	- generate function
	- trigger query
	- transform function

### generate function parameters

- from @Vertex
- to @Vertex

### transform function parameters

- events @Vertex
- from @Vertex
- to @Vertex

## Possible view definitions

|name|type|direction|

## View defintion
## Syntax