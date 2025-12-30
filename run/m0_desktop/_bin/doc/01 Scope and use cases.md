# Scope and use cases

> The -zero platform is built on Meta-Vertex Edge Graph (MVEG), a core data structure providing extreme simplicity with unlimited semantic expressivity and coherent behavior across all meta-levels. Based on MVEG Graph Database, the platform unifies representations of the underlying graph as diagrams (visual representation with rich primitives and nested views) and text languages (built-in and custom with language algebra supporting inheritance and mixing). Graph can also be executed by the Graph Virtual Machine. GVM operates at high abstraction levels where code runs alongside data and meta-models with no impedance mismatch, achieving full structural homoiconicity - code and data share identical structures, enabling reflection, auto-modification, and metaprogramming as core features rather than extensions.

Multiple platform instance types support different workflows: console instances for Linux/Mac/Docker environments, and GUI instances (Windows WPF) providing unified drag-and-drop, bulk operations, and advanced editors where all GUI elements are themselves manipulable graph fragments. Built-in GVM libraries support REST endpoint exposure and consumption, HTTP/Markdown web server, and JSON processing → enabling graph-based web services and document publishing.

## Scope

The -zero platform is built on several interconnected core capabilities:

**Meta-Vertex Edge Graph (MVEG)** - The Foundation
* Core data structure with extreme simplicity and unlimited semantic expressivity
* Coherent behavior, properties, and tooling across all data instances and meta-levels

**Graph Database** - Zero-Configuration Persistence
* Simple binary, text, or JSON file storage with graph edges across separate stores
* Complex querying with automatic indexing, triggers, transactions, and updatable views
* Built directly on MVEG

**Modeling** - Unlimited Meta-levels
* Represent complex data and knowledge (static properties, dynamic properties, anything expressible in language or mathematics) as MVEG
* Meta-modeling with any number of levels and custom relations between instances, models, and meta-models
* Model transformations (built-in and custom)
* Built-in and easily defined custom meta-models (all as graph fragments)

**Diagrams** - Graph = Diagram
* Graph IS diagram: visual representation of any data and models
* Rich library of diagram primitives, all as graph fragments
* Custom diagram types easily defined as graph fragments
* Nested diagrams and multiple views of the same data

**Text Languages** - Graph = Text
* Graph IS text: built-in and custom graph ⇔ text languages
* Language algebra supporting inheritance (whole languages or individual keywords) and language mixing
* Language definitions are graph fragments

**Graph Virtual Machine (GVM)** - Graph = Executable
* Graph IS code: executes graph programming languages natively
* Exposes computer resources (file system, MIDI) as graph
* High abstraction level operation:
  * Basic atom is set of meta-vertex extended edges
  * Cross-cutting concerns support by design
  * Extensive graph inheritance and views
  * Advanced querying built into execution platform
  * Code runs alongside data instances and meta-models in Graph DB (no impedance mismatch)
* Turing complete, currently UML-inspired OOP paradigm (extensible to any execution paradigm)
* Full structural homoiconicity: code and data structures are identical, enabling reflection, auto-modification, and being based on the same shared ontology
* Developers access platform features directly: metaprogramming, transactions, triggers, views, persistent storage
* Rich library: exposing and consuming REST endpoints, HTTP server, JSON, Markdown

**Platform Integration** - Unified Experience
* Core principle: diagram = graph = text, respected everywhere
* Updatable views work across all modalities
* Multiple instance types:
  * Console instance (Linux/Mac/Docker)
  * GUI instance (Windows WPF):
    * Unified drag-and-drop across all components
    * Bulk operations on edges/vertexes
    * Multiple graphical representations of same graph fragment
    * Diagram viewer/editor, advanced text editor, complex form scaffolding, flexible 2D visualization
    * All GUI elements are manipulable graph fragments
* Markdown-based complex document HTTP server

## Use cases