# Is MVEG any new?

There has been several attempts to do something more than pure _edge labeling_. Is _MVEG_ any new than?

> Most if not all other knowledge represenation schemas shares common limitation: edges and their metadata require different access mechanisms → you cannot navigate from an edge to its type definition through the same pointer-based traversal used for data navigation. 
>
> Existing paradigms have struggled with the "Ontological Wall" - the separation between the data being described and the mechanisms used to describe it. MVEG creates a breach in this wall not by adding features, but by a radical structural simplification.

## State of the art context

Let's analyze leading historic and modern aproaches to the data and knowledge modeling and compare them to the _MVEG_.

### Hypergraphs (standard)

Standard hypergraphs define hyperedges as arbitrary sets of vertices without internal structure - a hyperedge connecting {A, B, C} is simply the set itself, with no distinguished meta-layer describing what that connection means. While modern hypergraph libraries allow attaching metadata as properties to edges and nodes, this metadata exists externally to the graph structure and requires separate mechanisms for querying. The "meaning" of a hyperedge (what kind of relationship it represents) must be encoded through separate type systems or annotations rather than being intrinsic to the structure. To add properties or create meta-relationships about hyperedges, systems must lift them into special node representations, breaking the structural uniformity and requiring explicit conversion mechanisms between different abstraction levels.

> Standard hypergraphs treat edges as mathematical sets without semantic identity, forcing metadata and relationship types into external annotations that require separate query mechanisms rather than being navigable elements of the graph structure itself.

### Hypergraphs (extension)

Modern recursive hypergraph implementations like Graphbrain's Semantic Hypergraph support nesting where "the first element in the hyperedge is a connector, followed by one or more arguments, possibly in a recursive fashion", allowing hyperedges to participate as elements in other hyperedges. However, this recursion is syntactic and positional - the connector (first element) defines the relationship type, but accessing metadata about that connector requires treating it as a separate entity through pattern matching rather than direct structural navigation. Research implementations like Ubergraphs formalize recursive hypergraph structures, but these remain academic extensions without standardized semantics: each implementation defines its own rules for how recursion works, what properties hyperedges can have, and how to query meta-information. The fundamental limitation is you cannot point to "this specific hyperedge" and traverse to its type definition as a first-class operation - instead, you must reconstruct it through pattern matching or manual encoding.

> Recursive hypergraphs achieve nesting through syntactic composition (hyperedges containing hyperedges as elements), but lack uniform semantics for treating edges as addressable entities with navigable type definitions - requiring implementation-specific pattern matching rather than direct pointer-based traversal to meta-information.

### Conceptual Graphs

Conceptual Graphs use a bipartite structure that strictly separates concept nodes from relation nodes. Because relations belong to a different ontological category than concepts, a relation cannot directly describe another relation. Although concepts and relations can both be classified in type hierarchies and have subtypes, the operational semantics treat them differently - relation nodes have arity constraints and numbered arcs, while concept nodes have referents and can be coreferenced, creating "ontological bloat" with separate rules for each category.

_MVEG_ differs from Conceptual Graphs by using a single ontological primitive instead of a bipartite concept–relation split. Relations in MVEG are edges instantiated from relation classes represented by meta-vertices, not relation nodes acting as events. Meta-vertices describe entire classes of relations and can be related recursively using the same graph structure. This preserves ontological uniformity and avoids role-based relation modeling. As a result, MVEG supports structural meta-recursion with lower conceptual overhead. 

> Conceptual Graphs enforce a strict bipartite ontology with disjoint concept and relation type hierarchies, whereas MVEG uses a single uniform structure that enables recursive meta-relations without ontological separation.

### Datalog / HiLog

HiLog provides higher-order syntax allowing arbitrary terms to appear in predicate positions, but its model theory is first-order - the system embeds higher-order syntax into first-order logic through transformations like 'p(X)(Y,Z(V)(W)) → apply(p(X),Y,apply(apply(Z,V),W))'. This translation into predicate calculus using wrapper predicates means HiLog's higher-order capability exists at the query/reasoning language level, not in the physical data structure - predicates remain logical symbols to be unified and resolved, not addressable graph nodes with navigable edges. Implementations like XSB parse HiLog syntax but integration is partial; Flora-2 provides full implementation, yet even there, the storage model doesn't maintain predicates as first-class graph entities with explicit incoming/outgoing edge sets. While HiLog allows meta-level reasoning through syntactic manipulation (treating predicates as terms in logical expressions), accessing metadata about a predicate requires pattern matching and unification rather than direct structural navigation - you query for facts about predicates through the same resolution mechanism used for data, not through pointer-based traversal to a meta-node.

> HiLog achieves higher-order reasoning through syntactic embedding into first-order logic, where predicates-as-terms are manipulated by the query language and resolution engine, but implementations don't store them as navigable graph nodes with explicit edge structures - meta-level access remains query-based pattern matching rather than direct pointer traversal to physical meta-vertices.

### Topic Maps

Topic Maps model knowledge using three core primitives: topics (proxies for subjects), associations (hypergraph relationships between topics), and occurrences (links to external resources), with associations defined as relationships where each topic plays a role as a member. The fundamental constraint is that "only the topic is allowed to have names and occurrences and to play roles in associations" - you can only make assertions about subjects represented by topics, and "those assertions themselves are not topics", creating the "TAMD fracture" (Topic-Association-Member-Data separation). To describe properties of an association itself (such as documenting its source or attaching metadata), you must use reification - creating a separate topic that represents the association, effectively converting it from an association construct into a topic construct. While Topic Maps provide higher semantic abstraction than RDF by supporting n-ary relationships, this rigid tripartite structure prevents uniform recursive meta-relations: an association cannot directly play a role in another association without explicit conversion to a different ontological category, breaking structural uniformity and requiring heavy reification mechanisms that bloat the model.

> Topic Maps maintain strict ontological separation where only topics can have characteristics and play roles in associations - associations themselves are not topics and cannot participate in other associations without explicit reification (converting the association into a topic), preventing uniform recursive treatment and requiring different handling mechanisms for each meta-level.

### RDF / RDF-Star

RDF represents data as subject–predicate–object triples, where predicates act as relation labels, but the same URIs are also used as graph nodes to describe those predicates. This gives predicates a dual role: they label edges in data triples, yet appear as subjects in separate metadata triples that must be accessed using different query patterns. RDF-Star extends this by introducing embedded triple syntax using '<< >>' notation, allowing statement-level metadata annotations, but implements embedded triples by introducing a new RDF type alongside URI, blank node, and literal - the predicate still isn't a traversable node with structural edges. The 25-year evolution from RDF (1999) through RDFS (2004) for class hierarchies, OWL (2004) for logical inference, to SHACL for validation reflects accumulated complexity: each layer addresses limitations of the previous one, creating multiple query mechanisms and reasoning systems that must be learned and coordinated. Standard reification requires four additional triples to refer to the triple for which metadata is needed, while RDF-Star provides syntactic sugar but doesn't fundamentally change the structural relationship between predicates and their metadata.

> RDF and RDF-Star maintain predicates as URIs that function in dual roles - as symbolic edge labels in triples and as subjects in separate metadata triples - requiring different SPARQL query patterns and 25 years of layered extensions (RDFS, OWL, SHACL) to address accumulated limitations, while still lacking direct pointer-based traversal from edges to their type definitions as first-class structural elements.

### Property Graphs

In property graph models like Neo4j, Amazon Neptune or TinkerPop/Gremlin, relationships must always have a start node, end node, and exactly one type, with optional properties. In property-oriented graphs, edges cannot act as nodes. When meta-relationships are needed, developers must introduce explicit intermediate nodes to represent relationships, increasing structural complexity. This pattern bloats the model and breaks structural uniformity - accessing relationship metadata requires converting the relationship into a node concept, creating redundancy and requiring developers to reason about when data should be modeled as nodes versus relationships. Recent extensions like Meta-Property Graphs introduce reification to make sub-structures first-class citizens, but this represents an addition to address the core limitation rather than a fundamental architectural property, and is not part of standard property graph implementations.

> Property graphs treat relationships as second-class citizens - edges cannot reference meta edges, forcing the "intermediate node pattern" where relationships must be explicitly converted to nodes to create meta-relationships, bloating the model and breaking structural uniformity between data and metadata access patterns.

### OpenCog AtomSpace

OpenCog AtomSpace represents knowledge using Nodes (identified by name and type) and Links (identified by outgoing set and type), where almost all atom types are executable with dual roles - representing graph structures while having built-in operational semantics when executed. Links maintain both outgoing and incoming sets explicitly for fast hypergraph traversal, enabling recursive nesting where Links can contain other Links as members. However, fundamental structural semantics rely on hardcoded system types: EvaluationLink with PredicateNode and ListLink for logical predicates (expected by subsystems like PLN), PlusLink for arithmetic that performs symbolic algebra, AndLink/OrLink for boolean logic, InheritanceLink for ontologies  - each implemented as specialized C++ classes with built-in execution behavior. While users can define custom atom types at runtime or compile-time, these create a split between "native" system types with privileged operational behavior (like PlusLink's automatic arithmetic execution) and "user" types that lack such built-in semantics, requiring explicit programming to achieve similar functionality.

> OpenCog AtomSpace achieves recursive meta-relations through Links containing Links, but maintains type system dualism where hardcoded system types (EvaluationLink, PlusLink, ListLink) have built-in execution semantics and privileged operational behavior, while custom user-defined types lack such native functionality - creating a split between first-class "native" relations and second-class "user" relations that must explicitly replicate behavior available automatically to system primitives.

### TypeDB

TypeDB implements the PERA model with strict tripartite separation: entity types (independent objects), relation types (dependent objects playing roles), and attribute types (value-holding properties), where entity and relation types contain freely instantiable objects while attribute types contain non-freely instantiable values. This creates fundamental asymmetry: "only object types are able to implement interfaces: own attribute types and play roles in relation types - it is not possible for an attribute type to own another attribute type, or to play a role in a relation type", preventing attributes from participating in the meta-relationship structure. While relations can depend on role interfaces played by either entities or relations, enabling some recursive meta-relations, the PERA model requires each type to have exactly one super-type - enforcing single inheritance that limits flexibility in modeling complex domain hierarchies. The strong type system, while providing semantic validation and polymorphic querying, lacks homoiconicity: the schema defines types and interfaces declaratively, but you cannot use the graph structure itself to programmatically define logic that modifies the graph-triggers, view updates, and meta-programming must be implemented through external mechanisms rather than as native graph operations.

> TypeDB enforces rigid tripartite separation where only object types (entities and relations) can own attributes or play roles while attribute types are excluded from relations, combined with mandatory single inheritance and lack of homoiconicity - preventing the graph structure from defining its own modification logic and limiting the flexibility needed for uniform recursive meta-programming where semantics, data, and execution logic share identical structural representations.

## MVEG Value Proposition

MVEG addresses a fundamental limitation present across existing graph and knowledge representation systems: edges and their metadata require different access mechanisms. You cannot navigate from an edge to its type definition through the same pointer-based traversal used for data navigation.

Unlike approaches that try to give every edge a unique identity through reification, MVEG takes a more efficient path: the meta vertex on an edge identifies the **relation type**, not the edge instance. Multiple edges can share the same meta vertex, making the model lightweight while supporting infinite meta-levels.

**Key properties:**

- **Structural Homoiconicity:** The definition of a relation type is just another vertex in the graph, accessible through direct pointers.

- **Unified Navigation:** Navigating from an instance to its type, and from a type to its meta-type, uses exactly the same pointer traversal mechanism as navigating data.

- **Semantics as Data:** The meta vertex has its own incoming and outgoing edges representing properties, constraints, and relationships—all queryable through standard graph operations without separate query mechanisms.

- **Recursion without Complexity:** Since meta vertices are shared by edge classes (not unique per instance), the graph remains efficient while supporting unlimited meta-levels. Each level uses identical structural primitives.

> **The practical impact** is not primarily about computational performance—modern implementations of RDF, Topic Maps, and Property Graphs can achieve similar efficiency with proper indexing. 

Rather, MVEG provides **conceptual simplicity**: 

- A single primitive (vertex) instead of multiple ontological categories
- A single navigation mechanism (pointer traversal) instead of separate query patterns for data vs. metadata
- Uniform treatment of semantics and data, enabling triggers, updatable views, and meta-programming as native graph operations

This eliminates the conceptual overhead accumulated through decades of evolution in other paradigms.

## MVEG comparsion to the state of the art

The following table situates the _Meta-Vertex Edge Graph (MVEG)_ within the historical context of knowledge representation and graph data models. It highlights the specific "Structural Gap" in existing approaches—typically a lack of ontological uniformity or executable semantics—and demonstrates how MVEG resolves this through **Structural Homoiconicity** (the physical reification of relation types as navigational vertices).

| Approach / Model | Year (≈) | Relations as nodes | Ontological uniformity | Any number of meta | Approach limitation | MVEG Resolution |
| **Hypergraphs** (standard) | 1970s | ❌ No | ❌ No | ❌ No | Hyperedges are mathematical sets without semantic identity. Metadata exists externally requiring separate query mechanisms. No native support for hyperedges about hyperedges. | Every _edge_ has a _meta vertex_ pointer. Semantics are navigable graph elements, not external annotations. |
| **Hypergraphs** (extension) | 2014 | ⚠️ Positional | ⚠️ Partial | ⚠️ Syntactic | Recursion is syntactic composition (hyperedges as list elements). No uniform semantics - implementation-specific pattern matching required instead of direct pointer traversal. | _Meta vertex_ is an addressable entity with direct pointer. Uniform semantics across all meta-levels through standard graph traversal. |
| **Conceptual Graphs** | 1976 | ✅ Yes | ❌ No | ⚠️ Limited | Bipartite structure: concepts and relations are disjoint ontological categories with separate type hierarchies. Cannot use a relation to describe another relation without conversion. | Single primitive (_vertex_) for both concepts and relations. Relations describe other relations through standard _edge_ mechanisms. |
| **Datalog / HiLog** | 1977 / 1989 | ❌ No | ❌ No | ⚠️ Logical only | Higher-order capability exists at query language level through syntactic embedding into first-order logic. Predicates are logical symbols, not navigable graph nodes with edge structures. | _Meta vertexes_ are physical graph nodes. Higher-order logic is a property of the data structure, enabling O(1) pointer traversal to meta-definitions. |
| **Topic Maps** | 1999 | ⚠️ Partial | ❌ No | ❌ No | TAMD fracture: only topics can have characteristics and play roles. Associations cannot participate in other associations without explicit reification (converting to topic). | No ontological separation. _Meta vertex_ is a regular _vertex_ - relations participate in other relations through standard _edge_ mechanisms without conversion. |
| **RDF / RDF-Star** | 1999 / 2014 | ⚠️ Dual role | ❌ No | ⚠️ Limited | Predicates function as both edge labels and subjects in separate triples, requiring different query patterns. 25 years of layered extensions (RDFS, OWL, SHACL) created accumulated complexity. | _Meta vertex_ is a regular _vertex_ from the start. No dual role - using a relation and describing it both use standard graph traversal with direct pointers. |
| **Property Graphs** | ~2005 | ❌ No | ❌ No | ❌ No | Edges are second-class citizens. Cannot reference other edges. Requires intermediate node pattern to create meta-relationships, bloating the model. | Edges contain pointers to _meta vertexes_ which are first-class vertices. Meta-relationships use the same edge mechanism without intermediate nodes. |
| **OpenCog AtomSpace** | ~2008 | ✅ Yes | ⚠️ Partial | ✅ Yes | Type system dualism: hardcoded system types (EvaluationLink, PlusLink) have built-in execution semantics. Custom types lack such native functionality, creating split between native and user relations. | No system types - all relation types are _meta vertexes_. User-defined relations are structurally identical to any other relation, with semantics defined by graph structure. |
| **TypeDB** | ~2016 | ⚠️ Partial | ❌ No | ⚠️ Limited | Tripartite separation: attributes cannot own attributes or play roles. Single inheritance only. Lacks homoiconicity - cannot use graph structure to define modification logic. | Single _vertex_ primitive unifies entities, relations, and attributes. Graph structure can define its own execution logic (triggers, views) through standard edge mechanisms. |

> RDF, Topic Maps, and Datalog have proven their value, but share inherited limitations: predicates distinct from subjects, associations requiring reification to participate in other associations, edges categorically separate from nodes. MVEG achieves equivalent expressiveness by eliminating these categorical boundaries entirely—using a single uniform primitive where every relation type is simply a vertex, accessible through the same pointer-based traversal used for data.

## Summary and Conclusions

The evolution of knowledge representation reveals a recurring pattern: systems introduce categorical distinctions (predicates vs. subjects, associations vs. topics, edges vs. nodes) to solve specific problems, then require additional mechanisms (reification, intermediate nodes, dual-role constructs) to bridge the gaps those distinctions create.

MVEG demonstrates these boundaries are unnecessary. By treating relation types as vertices accessible through standard pointer traversal, it achieves structural homoiconicity where semantics and data share identical representations. This enables triggers on semantic entities, updatable views across meta-levels, and graph-defined execution logic as native operations—not external bolt-ons.

> The value is conceptual parsimony: one primitive, one navigation mechanism, uniform treatment across all meta-levels. Where other paradigms accumulated complexity over 25 years, MVEG provides a structurally simpler foundation that naturally supports recursive meta-programming without reification or categorical conversions.