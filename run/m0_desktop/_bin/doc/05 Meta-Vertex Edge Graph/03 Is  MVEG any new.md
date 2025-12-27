# Is MVEG any new?

There has been several attempts to do something more than pure _edge labeling_. Is _MVEG_ any new than?

The following table situates the _Meta-Vertex Edge Graph (MVEG)_ within the historical context of knowledge representation and graph data models. It highlights the specific "Structural Gap" in existing approaches—typically a lack of ontological uniformity or executable semantics—and demonstrates how MVEG resolves this through **Structural Homoiconicity** (the physical reification of relation types as navigational vertices).

| Approach / Model | Year (≈) | Relations as nodes | Ontological uniformity | Recursive meta-relations | Structural Limitation (The Gap) | MVEG Resolution (Structural Homoiconicity) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Davidsonian Event Semantics** | 1967 | ✅ Yes (events) | ⚠️ Partial | ⚠️ Implicit | **Semantic only.** Provides interpretation logic but lacks a concrete data structure or operational semantics for computation. | **Concrete Implementation.** MVEG reifies events/relations into physical vertices with explicitly defined incoming/outgoing edges, making the semantics executable. |
| **Hypergraphs** (node-edge form) | 1970s | ✅ Yes (hyperedge-nodes) | ⚠️ Partial | ❌ No | **No semantics.** Standard hypergraphs treat edges as sets without distinguished meta layer. Ubergraphs allow recursion but lack a defined execution model. | **Meta-Vertex.** Every edge has a specific _meta vertex_ with handles semantics, |
| **Conceptual Graphs** (J. F. Sowa) | 1976 | ✅ Yes | ❌ No (concept / relation) | ⚠️ Limited | **Ontological Bloat.** Maintains distinct primitives for concepts and relations. Lacks operational semantics, preventing uniform recursion as a programming model. | **Unified Ontology.** MVEG uses a single primitive (_vertex_) for both concepts and relations, simplifying the meta-model to a single point of truth. |
| **Datalog / HiLog** | 1977 / 1989 | ⚠️ Partial (predicates as terms) | ❌ No (predicates vs terms) | ⚠️ Logical only | **Computational vs. Structural.** Relations exist as language-level predicates. Even in HiLog, "predicates as terms" is syntactic sugar; they are not stored as traversable nodes with native "incoming edges" in memory. | **Structural Execution.** MVEG implements higher-order logic physically. Direct support for _incoming edges_ allows O(1) navigation to relation instances. |
| **Topic Maps** (ISO/IEC 13250) | 1999 | ✅ Yes (associations) | ❌ No (topic / assoc / role) | ❌ No | **The "TAMD" Fracture.** Strictly separates Topics, Associations, and Roles. An association cannot play a role in another association without explicit, heavy reification (converting Association to Topic). | **Ontological Parsimony.** MVEG reduces complexity by treating the "meta" level simply as a pointer to another _vertex_, removing the need for distinct "Association" types. |
| **RDF / RDF-Star** | 1999 / 2014 | ❌ No (predicates) | ❌ No (S / P / O) | ⚠️ Limited | **Instance Identification Problem.** Predicates are URIs, not navigational nodes. Describing specific relation instances requires complex nesting (RDF-Star). | **Navigational Meta-Nodes.** The Meta Vertex is a standard vertex. Schema and data are structurally identical, allowing infinite nesting without syntax changes. |
| **Property Graphs** (Neo4j et al.) | ~2005 | ❌ No (edges only) | ❌ No | ❌ No | **Second-class Citizen.** Edge attributes exist, but relations (edges) cannot be nodes in other relations. Requires "intermediate nodes" pattern to simulate meta-edges, bloating the graph. | **First-class Relations.** By pointing a _meta vertex_ to a _vertex_, relations become addressable entities capable of having their own meta-definitions naturally. |
| **OpenCog AtomSpace** | ~2008 | ✅ Yes (Links) | ✅ Yes (Atoms) | ✅ Yes | **Type System Dualism.** Fundamental structural semantics often rely on hardcoded system types (e.g., `ListLink`, `EvaluationLink`). Creating custom structural primitives creates a split between "native" and "user" relations. | **Unified Type Derivation.** MVEG eliminates "system types". A relation's type is solely defined by its pointer to a _meta vertex_. This ensures that user-defined relations are structurally identical to system primitives. |
| **TypeDB** (PERA Model) | ~2016 | ✅ Yes (Relation types) | ❌ No (Entity vs Relation) | ✅ Yes | **Schema Rigidity & Dualism.** While relations are first-class, TypeDB enforces a strict schema separation between Entities and Relations.[10, 11] It lacks homoiconicity: you cannot easily use the graph structure to define the logic that modifies the graph. | **Homoiconic Monism.** MVEG unifies Entity/Relation into a single _vertex_. This lack of rigid typing allows the graph to define its own execution logic dynamically (Code = Data), creating a homoiconic Graph Virtual Machine. |

Ontological Unification and Structural Homoiconicity in MVEG
While paradigms such as RDF, Topic Maps, or Higher-Order Logic (HiLog) offer theoretical foundations for meta-modeling, in practice they fail to provide a coherent operational model capable of natively and efficiently handling infinite recursive meta-levels. Existing solutions introduce an artificial division between the data layer (instances) and the schema layer (classes/predicates) or require complex auxiliary constructs (e.g., triple reification), which limits their expressiveness as a general computational substrate.

Meta-Vertex Edge Graph (MVEG) addresses this issue by introducing Structural Homoiconicity.

Unlike logical homoiconicity (known from Lisp, where "code is data"), MVEG transfers this property to the level of the physical graph structure. The definition of an edge in MVEG relies not on external labels, but on a direct pointer to another vertex: "edge's meta vertex that points to vertex".

This fundamental design decision entails key consequences that establish the superiority of MVEG over the State of the Art:

Relations as First-class Citizens: In RDF systems, a predicate is a resource but is often treated differently by storage engines (e.g., in separate indices). In MVEG, the relation type (MetaVertex) is a physical vertex within the graph. This means meta-information is structurally indistinguishable from data. Consequently, any relation can become the subject of another relation without the need for reification mechanisms or "RDF-Star" constructs.

Executable Semantics: Because an edge possesses explicitly defined properties, including a set of incoming edges, MVEG transforms the graph from a static data store into a dynamic runtime environment. Traversal algorithms do not need to refer to an external schema—the schema is "woven" into the structure via MetaVertex pointers. This allows the graph to be treated as the source code for a new class of languages (Graph-Oriented Programming).

Ontological Parsimony (Reduction of Complexity): Unlike Topic Maps, which define multiple primitive types (Topics, Associations, Roles), MVEG reduces the entire ontology to a single, atomic structure: a vertex possessing a value and edges. This "Occam's razor" at the data structure level makes MVEG the only model capable of effectively implementing a native execution environment for HiLog-type logic, eliminating the computational overhead associated with mapping logical predicates onto heterogeneous physical structures.

Conclusion: While other approaches stop at the stage of knowledge representation, MVEG, through structural homoiconicity, blurs the boundaries between data, schema, and code, paving the way for fully recursive, graph-based operating systems.



Among all the above attempts, some, particullary Topic Maps or Datalog seems to be promising, thus in practice they do not provide coherent and simple model that can easiely cover any kind of complex semantics and multiple meta levels schemas. 

> All of the public attempts so fart ultimately stop short of adopting relations as the sole ontological primitive, instead preserving special-purpose constructs or non-recursive abstractions that fundamentally limit expressiveness and preclude their use as a general computational substrate.

**Those attemps has a lot of potential but current implementations due to the complexity often introduces more problems than they solve.**

