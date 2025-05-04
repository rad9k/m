# Vertexes and edges

_Vertex_ has two kinds of properites:
- it has a atomic _value_
- it has a set of _incoming edges_
- it has a set of _outcoming edges_

The _value_ stores an atomic data such as a character string (such as "John") or number value (such as "3.14").

The _edge_ has following properties:
- source _vertex_
- edge's _meta vertex_ that points to _vertex_
- target _vertex_

In most cases the _incoming edges_ are of less importance, and most of the graph alghoritms in _MinusZero_ use only _outcoming edges_. However in some important edge scenarios the _incoming edges_

