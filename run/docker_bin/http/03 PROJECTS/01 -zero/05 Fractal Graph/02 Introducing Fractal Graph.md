# Introducing Fractal Graph

> **Fractal Graph** is a _directed cyclic graph_ where each _edge_, insted of a label has additional vertex, called _meta vertex_. 

...and that is. Now let's see where it takes us...

## Why Fractal Graph

Simple _edge labeling_ is not sufficient to express semantics required to manage the complexity of typical IT systems. As systems grow, semantics often becomes as complex as data itself — and treating it as mere annotations no longer works.

Fractal Graph addresses this problem by making **semantics a structural element of the graph**, not an external layer.

> **Why do we call those graphs "Fractal" ?**
>
> This is becouse instances of such graphs have tendency to form fractal like structures with possibly infinite number of "deepnes" levels. Those "deepnes" levels are just different conceptual meta levels. As each _edge_ has a _meta vertex_, this _meta vertex_ leads to another "deepnes" level. Becouse the target object of this "going deeper" operation, is another graph, that, by having _edges_ (and thus _meta vertexes_) can lead to another "deeper" level for each of the _edges_, in the end defining possibly infinitely-deep structure.
>
> In current usage of the Fractal Graph, we usu maximum three levels of "deepnes" (meta), but there is nothing (except cognitive and memory limits) stopping to have bigger "deepnes" level in the Fractal Graph instance.

### Core properties of Fractal Graph

Fractal Graph provides several key properties that make it effective for base structure of complex systems:

**Minimal structural extension**
- A simple directed cyclic graph
- The only addition is a meta vertex on each edge

**Very simple data model**
- A minimal set of concepts
- Capable of expressing very complex data and semantic structures

**Semantics as a first-class citizen**
- Data and semantics are treated in exactly the same way 
- Can query semantics in the same way as data
	- Can mix various meta levels in the same query
	- Applies to _Graph Virtual Machine_ as well → example: finding a function to call by a query
	
**Semantics is data. Data is semantics**
- Same rules and behavior for semantics and data:
	- validation
	- constraints
	- graphical representation
	- textual representation
	- persistent storage
	- relationships to other assets (data or semantics)
- Can define triggers on semantic entities 
	- Each reference to semantic entity can fire a programmable event trigger
	- Trigger can check if usage aligns with constraints - e.g., verify the _to vertex_ of the reffering _edge_ has all required _outgoing edges_ and add missing ones with default values
- Can define updatable views on semantic entities
	- Changes at deeper levels of semantic structure can fire programmable view update triggers
	- View update triggers can keep meta models at any level consistent with each other

**Unified tooling**
- The same tools are used to work with data and semantics
- Any meta level (instance, meta model, meta meta model ect.) has unified tooling at any level:
	- viewing
	- editing
	- querying
	- transformation
	
### Ontological unification

There is no ontological split between “data” and “metadata”:
- expressing semantics is the same as expressing data
- relationships between semantic entities are exactly the same as relationships between data entities
- recursive relationships between semantic entities are possible in the same way as recursive relationships between data entities

### Arbitrary number of meta levels and their shape

- any number of meta levels (instance, meta model, meta meta model ect.)
- semantics defined at any level
- abstraction hierarchies of any shape
- relationships between entities across different meta levels

### Reduction of ontological complexity

All expressive ontological power is available without introducing additional layers, such as:
- separate abstraction frameworks
- external definitions
- unnecessary conceptual machinery

To express any complex multi meta hierarchies of data / semantics entities and any relations between them, only three special meta vertices are required:
- $Is
- $EdgeTarget
- $VertexTarget

### Structural homoiconicity

_Fractal Graph_ can serve as a foundation for a new class of programming languages — graph programming languages — that exhibit full structural homoiconicity:
- code and data structures are identical
- the execution structure (Graph Virtual Machine code) is the same structure the programmer directly manipulates
- code structure represents execution flow
- code can do reflection and auto-modification
- code and data share the same ontology
- metaprogramming is a core feature, not an extension
	
## Fractal Graph definition

Fractal Graph consists of _vertexes_ and _edges_.

**Vertex**

_Vertex_ has three kinds of properites:
- an atomic _value_	
- a set of _incoming edges_
- a set of _outgoing edges_

The atomic _value_ may be, for example:
- a string (such as "John")
- number value (such as "3.14")

**Edge**

An _edge_ has the following properties:
- _from vertex_
- edge's _meta vertex_ 
	- reffering _vertex_ that describes given _edge_
	- describing the semantics of the _edge_
	- _meta vertex_ in Fractal Graph always represent not individual edge's desciption but rather relation class - that's why we have _meta_ in its name 
- _to vertex_

In most cases the _incoming edges_ are less important, and many of the graph alghoritms in _-zero_ use only _outgoing edges_. 
However in some important scenarios the _incoming edges_ are also essential (for example: to get the imports from the parent package), which is why the model explicitly supports them.

## How do we write about edges and vertexes?

A fully precise description of graph structure can be verbose and difficult to read.
For clarity, this documentation uses short descriptive phrases that are easier to follow but slightly informal.	
	
Shortcut phrases:

- **"Vertex _X_ has meta edge _Y_"**.

	Precise meaning: The set of _outgoing edges_ of _vertex_ X contains an _edge_ whose _meta vertex_ points to a _vertex_ with atomic value Y.

- **"Vertex _X_ has meta edge _Y_ with the value of _Z_"**

	Precise description: The set of _outgoing edges_ of _vertex_ X contains an _edge_:
		- whose _meta vertex_ points to a _vertex_ with atomic value Y
		- whose _target vertex_ has atomic value Z