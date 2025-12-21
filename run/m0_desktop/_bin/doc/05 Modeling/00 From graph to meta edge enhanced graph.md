# From graph to meta edge enhanced graph

We will introduce _meta edge enhanced graph_ (_MEEG_), but before that we will start with simplier structures and their limitations analysis.
## Trees, acyclic graphs, cyclic graphs

Starting from a data modelling perspective, lest's analyze what types of structures can be formed from _vertexes_ (sometimes called _nodes_) connected by _edges_. 

> For the simplicity and implementation transparency, we assume that all _edges_ are _directed edges_ → each _edge_ has defined _from vertex_ and _to vertex_.

- _**tree**_

	- Each _to vertex_ can have only one _from vertex_ → in the context of _trees_, this is expressed as: each _node_ can have only one _parent node_.
	- Cycles are forbidden → it's impossible to return to the same _vertex_ when traversing the structure.

- _**acyclic graph**_

	- A _to vertex_ can have multiple _from vertexes_.
	- Cycles are forbidden → it's impossible to return to the same _vertex_ when traversing the structure.
	
- _**cyclic graph**_

	- A _to vertex_ can have multiple _from vertexes_.
	- Cycles are allowed → it's possible to return to the same _vertex_ when traversing the structure.	
## Data relationships in different structures

Now let's examine what data relationships each structure type can express. Since data are stored inside _vertexes_, we're analyzing relationships between _vertexes_.

|structure type|one to many|many to many|recurrency|
|-|-|-|-|
|_tree_|✓|✗|✗|
|_acyclic graph_|✓|✓|✗|
|_cyclic graph_|✓|✓|✓|

*Conclusion:* The cyclic graph is the most universal data structure here, supporting all basic types of data relationships.
## How graph can handle semantics → adding labels

In most of the graph structures, data are property of the _vertexes_. Usually we can store some number or string value inside a _vertex_. But how to describe the _edge_? Imagine we would like to discriminate two possible relationships - for example "is employee" and "is organisational unit". Typicaly in graphs this is achieved by _labeling_ edges. In most of the cases this is just assigning a string value to the _edge_.
This works for most of the cases, but introduces two fundamental problems:
- If one wants to rename relationship name, we need to iterate over all the _edges_ in given system and change the _label string_ in each of the _edges_ with given _label string_.
- Not possible to express additional details about _edges_, in particular not possible to define any relationships between abstractions that describes _edges_. Those abstractions are limited to string values attached to _edges_ and the graph structure do not provide any additional mechanism to describe the more nuanced relationships between _vertexes_. 
## Defining meta edge enhanced graph (MEEG)

_Vertex_ has three kinds of properites:
- it has a atomic _value_	
- it h  as a set of _incoming edges_
- it has a set of _outcoming edges_

The _value_ stores an atomic data such as a character string (such as "John") or number value (such as "3.14").

The _edge_ has following properties:
- _from vertex_
- edge's _meta vertex_ that points to _vertex_
- _to vertex_

In most cases the _incoming edges_ are of less importance, and most of the graph alghoritms in _MinusZero_ use only _outcoming edges_. 
However in some important scenarios the _incoming edges_ are also necessary to be known for given _vertex_, so this is a reason our model directly supports _incoming edges_.
# How do we write about edges and vertexes?

As precise desciprtion of vertexes / edges shapes and values might be lenghty and hard to read, in this documentation we are going to use some "shortcuts" - phrases that are shorter but not quite precise.	
	
Those are:
- **"Vertex _X_ has meta edge _Y_"**.

	Precise description: _Outgoing edges_ set for _vertex_ X contains edge, that has _meta vertex_ pointing to _vertex_ with has a value of Y.

- **"Vertex _X_ has meta edge _Y_ with the value of _Z_"**

	Precise description: _Outgoing edges_ set for _vertex_ X contains edge, that has:
	
	- _meta vertex_ pointing to _vertex_ with has a value of Y.
	- _to vertex_ that has value of _Z_.

