# From graph to meta edge enhanced graph

We will introduce _meta edge enhanced graph_ (_MEEG_), but first let analyze some simplier structures and their limitations.
## Trees, acyclic graphs, cyclic graphs

Starging from the data modelling perspective, lest's analyze what types of structures can be formed out of _vertexes_ (sometimes called _nodes_) and _edges_ in between _vertexes_. 
For the simplicity and implementation transparency, we assume that all _edges_ are _directed edges_ → each _edge_ has _from vertex_ and _to vertex_ defined.
- _**tree**_
	Each _to vertex_ can have only one _from vertex_ → as we are discussing _trees_ here this rule orginally is expressed as: each _node_ can have only one _parent node_.
	No _edges_ cycles allowed → not possible to return to the same _vertex_ when traversing the structure.

- _**acyclic graph**_
	Multiple _from vertexes_ possible for given _to vertex_.
	No _edges_ cycles allowed → not possible to return to the same _vertex_ when traversing the structure.
	
- _**cyclic graph**_
	Multiple _from vertexes_ possible for given _to vertex_.
	_Edges_ cycles allowed → possible to return to the same _vertex_ when traversing the structure.
	
Now let's analyze what data relationships are possible to be expressed by _tree_, _acyclic graph_, _cyclic graph_. As we store the data in the _vertexes_, the analyzed relationships are in fact relationships between _vertexes_.
|structure type|one to many|many to many|recurrency|
|-|-|-|-|
|_tree_|YES|no|no|
|_acyclic graph_|YES|YES|no|
|_cyclic graph_|YES|YES|YES|
The conclusion is that _cyclic graph_ is most universal data structure here, and handles all basic types of relations between data.
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

