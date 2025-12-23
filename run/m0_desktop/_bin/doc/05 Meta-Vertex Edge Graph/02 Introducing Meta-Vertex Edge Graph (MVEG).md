# Introducing Meta-Vertex Edge Graph (_MVEG_)

> Meta-Vertex Edge Graph is a _directed cyclic graph_ where each _edge_ insted of a label has additional vertex, called _meta vertex_. 

## Why MVEG

Simple _edges labeling_ is too simple to handle semantics needed to effectively manage complexity we have in typical IT systems. We need something more.

Meta-Vertex Edge Graph (_MVEG_) delivers a lot of properties usefull when dealing with complexity by semantics:

- **simple directed cyclic graph. only add _meta vertex_ in _edge_**

	very simple data model that can easiely express very complex data structures
	
- **semantics is a first-class citizen**
	
	- same rules and system behavioue for data and (any level) semantics:
	
		- same validation
		- same constrains
		- same graphical presentation
		- same relations to other assets (data / semantics)
		
	- same tooling for interaction with data and (any level) semantics:
	
		- same viewing
		- same editing
		- same quering
		- same transformation
	
- **ontological unification**

	- expressing semantics is the same as expressing data
	- relationships between semantics entities are exactly the same as relationships between data objects
	- reccurent relationships on semantics entities possible exacty the same as reccurent relationships on data entities
	
- **any number of _meta_**

	- ability to define semantics of any level
	- any abstraction layers hierarchy and shape
	- relationships between entities of different meta levels
	
- **reduction of ontological complexity**

	- all the ontological expresivene power is aviable without any additional:
	
		- abstraction layers
		- definitions
		- non-necessary complexity
		
	- just two special meta vertexes: `$EdgeTarget` and `$VertexTarget`
		
- **structural homoiconicity**

	- _MVEG_ is used as a base starting point for the new class of programming langages (_graph programming lanugages_), that has homoiconicity property	
	- homoiconicity is provided in a full glory - full structural homoiconicity:
	
		- code and data strutures are the same
		- the execution structure (_Graph Virtual Machine code_) is the same as the _graph programming language_ programmer has direct access to
		- code structure represents code execution flow
		- code can do reflection and auto-modification
		- code and data onthology are the same
		- metaprogramming is core part of the language
	
## MVEG definition

Meta-Vertex Edge Graph is made out _vertexes_ and _edges_.

_Vertex_ has three kinds of properites:
- it has a atomic _value_	
- it h  as a set of _incoming edges_
- it has a set of _outcoming edges_

The _value_ stores an atomic data such as a character string (such as "John") or number value (such as "3.14").

The _edge_ has following properties:
- _from vertex_
- edge's _meta vertex_ - reffering _vertex_ that describes given _edge_
- _to vertex_

In most cases the _incoming edges_ are of less importance, and most of the graph alghoritms in _-zero_ use only _outcoming edges_. 
However in some important scenarios the _incoming edges_ are also necessary to be known for given _vertex_, so this is a reason our model directly supports _incoming edges_.

# How do we write about edges and vertexes?

As precise desciprtion of vertexes / edges shapes (relations) and values might be lenghty and hard to read, in this documentation we are going to use some "shortcuts" - phrases that are shorter but not quite precise.	
	
Those are:

- **"Vertex _X_ has meta edge _Y_"**.

	Precise description: _Outgoing edges_ set for _vertex_ X contains edge, that has _meta vertex_ pointing to _vertex_ with has a value of Y.

- **"Vertex _X_ has meta edge _Y_ with the value of _Z_"**

	Precise description: _Outgoing edges_ set for _vertex_ X contains edge, that has:
	
	- _meta vertex_ pointing to _vertex_ with has a value of Y.
	- _to vertex_ that has value of _Z_.

