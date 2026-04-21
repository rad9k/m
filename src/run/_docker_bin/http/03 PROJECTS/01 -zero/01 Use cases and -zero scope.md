# Use cases and -zero scope

The -zero is a complete graph database / back-end execution platform, where you can express your system design and implementation by models and diagrams, graph programing languages or any combination of them.

> The -zero platform is built on Fractal Graph, a core data structure providing extreme simplicity with unlimited semantic expressivity and coherent behavior across all meta-levels. Based on Fractal Graph Database, the platform unifies representations of the underlying graph as diagrams (visual representation with rich primitives and nested views) and text languages (built-in and custom with language algebra supporting text languages inheritance and mixing). Graph can also be executed by the Graph Virtual Machine. GVM operates at high abstraction levels where code runs alongside data and meta-models with no impedance mismatch, achieving full structural homoiconicity - code and data share identical structures, enabling reflection, auto-modification, and metaprogramming as core features rather than extensions.

Two platform instance types support different workflows: 
- console instances for Linux/OS X/Docker environments
- GUI instances (Windows WPF) providing unified drag-and-drop, bulk operations, and advanced editors where all GUI elements are themselves manipulable graph fragments

Built-in GVM libraries support REST endpoint exposure and consumption, HTTP/Markdown web server, and JSON processing → enabling graph-based web services and documentation publishing.

## Use cases

-zero is a modeling/development/execution platform than can be used in unlimited number of scenarios. A few example use cases:
	
### Back-end

-zero isn't ready to serve web front-ends on its own yet, so for now we position it more as a back-end platform exposing and consuming REST APIs. This is where it excels – storing data and logic together. The logic lives directly in the graph database, so there's no impedance mismatch or need for translation layers like ORM. You can design data schemas with a diagram editor and express back-end logic through diagrams, code, or any mix of both (you can even have code and diagrams on screen simultaneously). All the REST plumbing and OpenAPI exposed services specyfication are automatically generated on the fly. Just mark the functions you want to expose with the `Endpoint` meta edge.
	
_CI/CD:_ can use git to have code/diagrams versioning
_deployment:_ -zero console instance running in docker @ server/cloud, exposing REST services
	
### AI pipeline
	
In modern data science and AI, we typically don't rely on a single monolithic component. Instead, we use multiple, usually Python-based components that process data at various stages. Execution of several components defines a pipeline through which the data are being processed, resulting in some model or report in the end. Such pipelines often needs to handle logic, like looping through input files or deciding whether to call an LLM again to fix processing errors. In most cases, representing such pipelines as diagrams works well, and when needed, specific parts can be edited as text code.
	
_CI/CD:_ can use git to have code/diagrams versioning
_deployment:_ 
- -zero desktop instance running on AI expert machine _(desktop scenario)_
- -zero console instance running in docker @ server/cloud _(server scenario)_
	
### Business process orchestration
	
-zero core use case is to use diagrams for designing and executing of business process, such as client risk scoring in a bank. The business process can have very complex logic and have multiple sub-processes being represented in separate or embedded diagrams. The logic can reference -zero's internal database, such as risk profile tables stored in the graph database. Business processes typically orchestrate calls to other systems or microservices via REST API, using their data for high-level processing and business decisions. 
The diagrams aren't simple "no-code tools" - they can express processing of very complex data structures with complex queries, transformations and data structures creation statements. For documentation, -zero has an embedded HTTP web server that instantly displays all changes in business data (like risk profile tables) or diagrams (like risk scoring business proces) through a markdown-based documentation portal.
	
_CI/CD:_ can use git to have code/diagrams versioning
_deployment:_ -zero console instance running in docker @ server/cloud, with network access to the orchestrated systems
	
### Enterprise architecture

-zero started as enterprise diagramming platform and supports data instances, models and meta-models creation, update, maintanace, transformations and instant documentation. Those data and models are just a graph fragments and the diagrams are way of visualising those structures with a wide area of diagram primitives like boxes, lines, lists, embeded forms, code fragments editors, embeded diagrams and domain centric combinations of those. All primitives can be customized with colors, borders, rounding, line types and transparency. While there's a solid collection of built-in diagram types and primitives, creating custom ones is straightforward. 
	
Creating diagrams is just the first step. You can model high-level dependencies and add meta-information, making it easy to manage enterprise-scale complexity by querying models and their metadata. Add automatic model transformations and by modyfing one diagram (and thus underlying model), the updatable view is being automatically modified and the changes are visible on "target" diagram. Such transformation can be high-level simplification of complex structure, complete ontology reversion (for example from product centric to customer centric) or any other transformation that creates value. And those updatable views can work on both directions - when "target" diagram is modified, the "source" one gets updated.
	
In the end the diagrams needs to be presented to the wider audience. Mind that diagrams can be updated frequently. Fortunatelly no need to "publish" them. Just create a markdown based hierarchical documentation site and reference any diagrams from the md files. They'll appear in the documentation portal with all updates visible instantly.	

_CI/CD:_ can use git to have diagrams versioning
_deployment:_ -zero desktop instance running on the architects machines, documentation server running -zero console instance exposing the HTTP server
	
### Platform for Platforms

Most sufficiently complex software systems, as they mature, independently converge on the same set of needs: an internal scripting or automation language, a growing customization layer that gradually evolves into a user-definable type system, form scaffolding auto-generated from types or metadata, the ability to express advanced user intent as a diagram - a workflow, a decision tree, a process. Also there is a need for some mechanism for persisting complex structured state, and REST-based integration with external systems, both as a client and as a server.
	
These aren't incidental features. They are the natural gravitational endpoints of complex platform development — and they are precisely what -zero is built on, at its core and what is enabled for power-user for further customisation.
This makes -zero an ideal foundation for building complex platforms. Rather than reinventing each of these capabilities in isolation, a platform built on -zero inherits scripting (with possibility to create own text languages), a meta-level type system with user-defined extensions, automatic form scaffolding, diagram editors - including ability to create own diagram types, a graph database for state persistence, and full REST plumbing — all coherently unified. When building platform on -zero, the result is a system that doesn't grow toward these features over years of accumulation, but starts with them as first-class primitives.

_CI/CD:_ can use git to have code/diagrams versioning
_deployment:_  -zero desktop instance running user's machine

## -zero scope

The -zero platform is built on several interconnected core capabilities:

### Fractal Graph
_The Foundation_

* Core data structure with extreme simplicity and unlimited semantic expressivity
* Coherent behavior, properties, and tooling across all data instances and meta-levels

### Graph Database
_Zero-Configuration Persistence_

* Simple binary, text, or JSON file storage with graph edges across separate stores
* Complex querying with automatic indexing, triggers, transactions, and updatable views
* Built directly on MVEG

### Modeling
_Unlimited Meta-levels_

* Represent complex data and knowledge (static properties, dynamic processes, anything expressible in language or mathematics) as Fractal Graph
* Meta-modeling with any number of levels and custom relations between instances, models, and meta-models
* Model querying and transformations (built-in and custom)
* Built-in and easily defined custom meta-models (defined as a graph fragments)

### Diagrams
_Graph = Diagram_

* Graph IS diagram: visual representation of any data and models
* Rich library of diagram primitives (defined as a graph fragments)
* Custom diagram types easily defined as graph fragments
* Nested diagrams and multiple views of the same data

### Text Languages
_Graph = Text_

* Graph IS text: built-in and custom graph ⇔ text languages
* Language algebra supporting inheritance (whole languages or individual keywords) and language mixing
* Language definitions are graph fragments

### Graph Virtual Machine (GVM)
_Graph = Executable_

* Graph IS code: executes graph programming languages natively
* Exposes computer resources (file system, MIDI) as graph
* High abstraction level operation:
  * Basic atom is set of meta-vertex extended edges
  * Cross-cutting concerns support by design
  * Extensive graph inheritance and updatable views
  * Advanced querying built into execution platform
  * Code runs alongside data instances and meta-models in Graph DB (no impedance mismatch)
* Turing complete, currently UML-inspired OOP paradigm (extensible to any execution paradigm)
* Full structural homoiconicity: code and data structures are identical, enabling reflection, auto-modification, and being based on the same shared ontology
* Developers access platform features directly: metaprogramming, transactions, triggers, updatable views, persistent storage
* Rich library: exposing and consuming REST endpoints, HTTP server, JSON, Markdown

### Platform Integration
_Unified Experience_

* Core principle: diagram = graph = text, respected everywhere
* Updatable views work across all modalities
* Two -zero instance types:
  * Console instance (Linux/OS X/Docker)
  * GUI instance (Windows WPF):
    * Unified drag-and-drop across all components
    * Bulk operations on edges/vertexes
    * Multiple graphical representations of same graph fragment
    * Diagram viewer/editor, advanced text editor, complex form scaffolding, flexible 2D visualization
    * All GUI elements are manipulable graph fragments
* Markdown-based hierarchical document HTTP server, displaying actual versions of all the referenced diagrams

## Summary

> -zero is a complete graph database and execution platform where system design and implementation can be expressed through models, diagrams, text and graph programming languages code, or any combination thereof. The platform is built on Fractal Graph, a structure that provides extreme simplicity with unlimited semantic expressivity, unifying graph representation as visual diagrams, text, and executable code. -zero example use cases are: backend exposing REST API, AI/data processing pipeline, business process orchestration, and an environment for enterprise architecture with instant documentation via built-in HTTP server.

## Future

...and yes. We are just getting started. There are many, many, many interesting next steps for -zero platform, for example:
- modeling and meta-modeling vs implementation research. _automating the "what?" → "how?"_
- front-end. _GVM running in the browser_
- distributing GVM. _edges and access / queries between GVM instances across network_
- mulithreaded GVM. _GVM supporting massive mulithreaded processing_

**the next steps depends on your input and feedback. it is your move now**