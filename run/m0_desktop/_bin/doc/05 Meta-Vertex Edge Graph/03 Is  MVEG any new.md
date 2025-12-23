# Adding semantics to graphs

## Why semantics?

In most graph structures, data is stored as properties of the _vertexes_. Usually we can store some number or string value inside a _vertex_.
If there is a lot of _vertexes_, the orginal creators of the structure, and other people even more - can have feeling of complexity. No formal definition here, just a story... so, what can we do about the complexity in data? One of the attempt is to

> To manage data complexity, it is nice to introduce semantics

Do not want to define complexity or semantics here as those are rabbit holes. Let's go further. There is a world to be saved.

## How graph can handle semantics. Is adding labels enough?

> As _vertexes_ stores data, the _edges_ seems to be a good place to handle semantics. 

But how do we describe an _edge_? Imagine we want to distinguish between two different types of relationships — for example, "is employee" and "is organizational unit". In graphs, this is typically achieved by _edges labeling_. In most cases, this simply means assigning a string value to the _edge_.

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

## One step further

There has been several attempts to do something more than pure _edge labeling_. 

| Approach / Model                  | Year (≈) | Relations as nodes | Ontological uniformity | Recursive meta-relations | Primary domain | Why it is inferior to MVEG |
|----------------------------------|----------|--------------------|------------------------|--------------------------|----------------|---------------------------|
| Davidsonian Event Semantics      | 1967     | ✅ Yes (events)     | ⚠️ Partial              | ⚠️ Implicit              | Logic, linguistics, NLP | Provides semantic interpretation only; lacks a concrete data structure, operational semantics, or support for graph-based computation. |
| Hypergraphs (node-edge form)     | 1970s    | ✅ Yes (hyperedge-nodes) | ⚠️ Partial              | ❌ No                    | Mathematics, modeling | Treats hyperedges as structural devices without identity, behavior, or recursive relational semantics. |
| Conceptual Graphs (J. F. Sowa)   | 1976     | ✅ Yes              | ❌ No (concept / relation / role) | ⚠️ Limited               | Knowledge representation, AI | Maintains multiple ontological primitives and lacks a computational or executable semantics, preventing uniform recursion and use as a programming model. |
| Datalog                           | 1977     | ⚠️ Partial (predicates can be interpreted as meta-nodes) | ❌ No (predicates vs terms) | ❌ No                    | Logic programming, databases | Relations exist as language-level predicates; treating predicates as nodes is ad hoc, lacks native recursion over meta-relations, and cannot integrate facts and types uniformly in a computational graph. |
| Topic Maps (ISO/IEC 13250)       | 1999     | ✅ Yes (associations) | ❌ No (topic / association / role) | ❌ No                    | Knowledge organization | Separates topics, associations, and roles as distinct primitives and disallows associations between associations, blocking recursive meta-modeling and computation. |
| RDF (Triple Model)               | 1999     | ❌ No (predicates)  | ❌ No (subject/predicate/object) | ❌ No                    | Semantic Web | Encodes relations as language-level predicates rather than domain objects, making relations non-identifiable and non-recursive. |
| Property Graphs (Neo4j et al.)   | ~2005    | ❌ No (edges only)  | ❌ No                  | ❌ No                    | Databases | Allows edge attributes but does not elevate relations to first-class nodes, forcing ad hoc patterns for meta-relations and preventing uniform computation. |
| RDF* / SPARQL*                   | 2014     | ⚠️ Indirect (reified triples) | ❌ No                  | ⚠️ Limited               | Annotated RDF data | Reifies statements only as a secondary mechanism, preserving predicate asymmetry and lacking native relational identity or execution semantics. |

Among all the above attempts, some, particullary Topic Maps or Datalog seems to be promising, thus in practice they do not provide coherent and simple model that can easiely cover any kind of complex semantics and multiple meta levels schemas. 

> All of the public attempts so fart ultimately stop short of adopting relations as the sole ontological primitive, instead preserving special-purpose constructs or non-recursive abstractions that fundamentally limit expressiveness and preclude their use as a general computational substrate.

**Those attemps has a lot of potential but current implementations due to the complexity often introduces more problems than they solve.**

