# How to shape data - trees and graphs

> In this chapter, we will introduce _Fractal Graph_, but before that we will start with simplier structures and their limitations analysis.


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

## How to manage complexity. Semantics?

In most graph structures, data is stored as properties of the _vertexes_. Usually we can store some number or string value inside a _vertex_.
If there is a lot of _vertexes_, the orginal creators of the structure, and other people even more - can have feeling of complexity. No formal definition here, just a story... so, what can we do about the complexity in data? One of the attempt is to

> To manage data complexity, it is nice to introduce semantics

Do not want to define complexity or semantics here as those are rabbit holes. Let's go further. There is a world to be saved.

## How graph can handle semantics. Is adding labels enough?

> As _vertexes_ stores data, the _edges_ seems to be a good place to handle semantics. 
>
> But how do we describe an _edge_? Imagine we want to distinguish between two different types of relationships — for example, "is employee" and "is organizational unit". In graphs, this is typically achieved by _edges labeling_. In most cases, this simply means assigning a string value to the _edge_.

This works for most of the cases, but introduces two fundamental problems:

- **Edges lack shared identity**

	Labels are independent string values attached to individual _edges_. To discover which _edges_ share the same relationship type, we must scan all _edges_ and compare their label strings — either through direct string comparison or by building auxiliary lookup structures. The system has no built-in concept of a relationship type that multiple _edges_ can reference; each label is merely a standalone string. 
	
	Example consequences are:
	- Not being able to rename all _edges_ of given type easiely
	- Not being albe to count all instances of given _edge_ type easiely
	- Not being able to delete all instances of given _edge_ type easiely
	
- **Edge labels lack structure**

	Labels are plain strings with no additional capabilities. We cannot use other complex entities for describing _edges_. In particular, we can not define hierarchies between descriptions (like "manages" is a type of "works with"), or describe what given _edges_ mean beyond the label text itself.
	
	Example consequences are:
	- Not being able to define _edge_ constrains such as minimal or maximal count
	- Not being able to define target type for the _edge_ types
	- Not being able to define relationships between _edge_ types
	
> _Edge labeling_ is not doing the job → Need something more. **Need to have meta model for the edges**
