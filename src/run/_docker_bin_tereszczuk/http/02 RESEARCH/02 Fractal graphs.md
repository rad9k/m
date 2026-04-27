# Fractal Graphs

## Summary

> Fractal Graphs extend directed graphs by describing every edge with a meta-vertex, making semantics intrinsic to the structure rather than external — so data, metadata, and meta-models are all represented uniformly within a single, self-consistent framework. This enables structural homoiconicity, where data, code, and architecture become different projections of the same underlying graph, eliminating the need to synchronize separate representations across layers. The [-zero platform](http://tereszczuk.com/-zero) is a practical implementation of this idea, unifying a graph database, modeling environment, diagram editor, textual languages, and a Graph Virtual Machine into one coherent system where requirements, implementation, and documentation coexist without fragmentation.

## Introduction

Fractal Graphs are a novel mathematical and computational structure designed to unify data and semantics within a single, coherent framework. They extend directed cyclic graphs through a minimal yet fundamental modification: every edge is described by a meta-vertex, turning relationships into first-class, navigable entities.

This structural shift eliminates the traditional separation between data and its description. In most existing systems, semantics are external-implemented through labels, schemas, or auxiliary models. In Fractal Graphs, semantics become intrinsic to the structure itself. They can be queried, transformed, and recursively extended using exactly the same mechanisms as data.

The result is a system with fractal depth: each relation can lead to another level of abstraction, enabling arbitrarily deep meta-modeling without introducing new primitives or ontological layers. Data, metadata, and meta-models are represented uniformly, forming a single, self-consistent structure.

From a theoretical perspective, Fractal Graphs introduce structural homoiconicity, where data, semantics, and execution share the same representation. This removes the need for separate abstraction layers and enables recursive reasoning, reflection, and transformation directly within the structure.

From a practical perspective, this enables a new class of systems in which traditionally separate artifacts-data, code, architecture, and documentation-can be unified into a single, queryable graph.

## Core Properties

### Minimal Structural Extension

- Based on a simple directed cyclic graph
- The only addition: a meta vertex on every edge

### Very Simple Data Model

- A minimal set of concepts
- Capable of expressing highly complex data and semantic structures

### Semantics as a First-Class Citizen

- Data and semantics are treated identically
- Semantics can be queried the same way as data
- Multiple meta levels can be mixed within a single query

### Semantics Is Data. Data Is Semantics.

Both follow the same rules for:
- validation & constraints
- graphical and textual representation
- persistent storage
- relationships to other assets
- programmable event triggers
- updatable views with trigger-based consistency

### Unified Tooling

The same tools apply at every meta level (instance, meta model, meta-meta model, etc.) for:
viewing, editing, querying, and transformation.

### Ontological Unification

No split between "data" and "metadata" — expressing semantics and expressing data are the same operation, including recursive relationships between semantic entities.

### Arbitrary Meta Levels

- Any number of meta levels supported
- Semantics can be defined at any level
- Abstraction hierarchies of any shape
- Cross-level relationships between entities

### Reduction of Ontological Complexity

All expressive power is available without separate abstraction frameworks or external definitions.
Only **three special meta vertices** are required to express any multi-meta hierarchy:
- `$Is`
- `$EdgeTarget`
- `$VertexTarget`

### Structural Homoiconicity

Fractal Graph can serve as the foundation for graph programming languages where:
- code and data structures are identical
- execution structure = directly manipulable structure
- reflection and self-modification are native
- metaprogramming is a core feature, not an add-on

## Why "Fractal"?

Each meta vertex leads to another conceptual level, which in turn can have its own meta vertices — producing a potentially **infinitely deep**, fractal-like structure. In practice, up to three depth levels are commonly used.

## Fractal Graphs core use cases

Fractal Graphs is a simple theoretical concept with highly practical real-world applications.

### Datasets

Fractal Graphs are well-suited for representing datasets at the most concrete level — the instance level. They provide a natural and clean way to model raw data without any structural workarounds.

### Data Modeling

Fractal Graphs excel at modeling complex data structures. No tricks, hacks, special cases, or ad-hoc workarounds are needed — models built on Fractal Graphs remain clean, coherent, and principled regardless of complexity.

### Meta Models

By their very nature, Fractal Graphs are not limited to instance-level models. They seamlessly support meta models — and meta models of meta models — to an arbitrary depth. This recursive "going deeper" property is precisely what gives them the name *fractal*.

### Relationships Between Meta Levels

Because Fractal Graphs unify the way relationships are expressed across meta levels — from instance-to-model up to model-to-meta-model — multiple meta levels can coexist within a single coherent data structure. This structure can carry rich information about inter-level relationships, which opens the door to advanced applications such as **meta model algebra**.

### Expressing Semantics

Wherever there is a need to express the semantics of relationships, Fractal Graphs shine. Generalizing semantic expression is, in fact, the core purpose of this mathematical structure — making it a natural fit for any domain where meaning, not just structure, needs to be captured.

### An Exciting Direction — Fractal Graph Based Deep Neural Networks (FGBDNN)

The elegant mathematical simplicity of Fractal Graphs opens up possibilities well beyond data modeling. One particularly intriguing direction is using Fractal Graphs to formally express the behavior of neurons in artificial neural networks.

There is significant unexplored potential here — the main constraint being the need to ensure differentiability of the resulting model, which does impose some boundaries. But the space is wide open and waiting to be explored. *If this topic interests you, let's talk!*

## Fractal Graphs added value

Across these domains, the key advantage is not only expressive power but conceptual unification. Instead of synchronizing multiple representations of a system (requirements, models, code, data), Fractal Graphs allow them to exist as different projections of the same structure.

Ultimately, Fractal Graphs propose a shift from fragmented system design to self-describing, executable knowledge structures-where complexity is managed not by adding layers, but by eliminating the boundaries between them.

## -zero - Implementation of Fractal Graphs

>-zero is a computational platform built as a practical implementation of Fractal Graphs, transforming the theoretical model into a unified environment for designing, executing, and evolving complex systems.
>
>-zero is a public domain software with full documentation and code available here: [http://tereszczuk.com/-zero](http://tereszczuk.com/-zero)

At its core, -zero is a graph database and execution platform where **data, semantics, code, and architecture coexist within a single structure**. Instead of separating concerns across multiple layers and technologies, the platform represents all system elements as fragments of one Fractal Graph, ensuring consistency, transparency, and immediate traceability.

This enables a fundamental shift in how systems are built. In -zero, **requirements evolve into architecture, and architecture into implementation**, without losing their connections. There is no need to synchronize documentation, models, and code-they are simply different views of the same underlying graph.

The platform combines several capabilities into a coherent whole:

### Graph Database

Persistent storage with querying, constraints, triggers, and updatable views, all operating directly on Fractal Graph structures
 
### Modeling Environment

Support for unlimited meta-levels, enabling seamless transitions between data, models, and meta-models  

### Diagrammatic Interface

Visual representation where diagrams are not abstractions but direct views of the underlying graph  

### Textual Languages

Built-in and extensible languages that map directly to graph structures, allowing hybrid visual–text workflows  

### Graph Virtual Machine (GVM)

Execution layer where graph structures act as code, enabling computation directly on the same structure that stores data and semantics  

Through this integration, -zero eliminates the traditional fragmentation of IT systems. There is no distinction between "data layer," "application logic," or "architecture description"-all are expressed within the same formal system.

**Practical outcomes include:**

- elimination of impedance mismatch between data and code  
- real-time synchronization between requirements, implementation, and documentation  
- ability to query and transform systems across all abstraction levels  
- native support for metaprogramming, reflection, and system evolution  
- unified tooling across modeling, execution, and visualization

## -zero key application domains

As -zero is ment to change IT fragments, it's proper usage and possible domains are quite broad. Some of the impacted areas are:

### Graph databases and back-end systems

Fractal Graphs enable storage of both data and logic within the same structure, eliminating impedance mismatch between database and application layers. APIs, schemas, and execution logic can be derived directly from the graph.

### Business process orchestration

High-level processes-such as risk scoring, decision systems, or workflow automation-can be modeled, executed, and continuously updated as graph structures, with direct linkage between business semantics and operational logic.

### Enterprise architecture and system modeling

Fractal Graphs provide a unified representation of systems across all abstraction levels. Architecture, data models, and implementation become different views of the same underlying structure, enabling real-time consistency, analysis, and transformation.

### Programming languages and execution models

As a foundation for graph-based computation (e.g., Graph Virtual Machines), Fractal Graphs support fully homoiconic programming environments where code and data are structurally identical, enabling native metaprogramming, reflection, and self-modifying systems.

### Complex platform development

Most sufficiently complex software systems, as they mature, independently converge on the same set of needs: internal scripting, a user-definable type system, form scaffolding, diagram-based expression of user intent, structured state persistence, and REST integration with external systems. These are not incidental features — they are the natural gravitational endpoints of platform evolution, and precisely what Fractal Graphs enabled while implemented as -zero. A platform built on -zero inherits all of these as first-class primitives, coherently unified, rather than accumulating them piecemeal over years of development.

>The -zero project is not an incremental improvement over existing platforms. It represents a shift toward **self-describing, executable systems**, where the structure used to model reality is the same structure used to implement and run it.
>
>As an implementation of Fractal Graphs, -zero demonstrates that the theoretical unification of data and semantics can be realized in practice-enabling systems that are more coherent, adaptable, and fundamentally easier to reason about.