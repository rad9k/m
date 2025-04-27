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
	- trigger query
	- transform function
- **target**	
	- trigger query
	- transform function

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
|updatable target to source    |updatable      |target to source|NO                  |NO                       |YES                 |YES                     
|updatable both                |updatable      |both            |YES                 |YES                      |YES                 |YES                      |

## View defintion
## Syntax