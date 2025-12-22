# Trees and graphs

We will introduce _Meta-Vertex Edge Graph_ (_MVEG_), but before that we will start with simplier structures and their limitations analysis.

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
|_tree_|✅|❌|❌|
|_acyclic graph_|✅|✅|❌|
|_cyclic graph_|✅|✅|✅|

> *Conclusion:* The cyclic graph is the most universal data structure here, supporting all basic types of data relationships.