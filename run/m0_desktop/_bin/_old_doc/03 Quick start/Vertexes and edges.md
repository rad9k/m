# Vertexes and edges

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

