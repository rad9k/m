# Why be interested. Current IT problems and how to solve them

> -zero is a fundamental game-change. The revolution starts with reshaping the basic universe atom - fundamental data structure. The Meta-Vertex Edge Graph (MVEG) brings simplicity, coherence and extreme expressing power. And it shines everywhere used. Complex structures becomes more coherent and transparent. Universal properties and behaviour enables same powerfull mechanics (like meta-data, queries and updatable views) and tooling (like diagram view/edit, extensive visualisation).
>
> **_MVEG when applied to old IT concepts like database, query language, model, meta-model, virtual machine and programming language results in fundamental changes among those concepts and system paradigm-shift:_**
>
> **abstraction is lost in the implementation → abstraction and its implementation are just two views of the same interconnected MVEG structure**

## The problems

Modern IT and especially computer systems creation and maitanance has several fundamental problems:

**requirements**

- requirements definition is lost in the implementation
- implementation itself is too low-level to recreate requirements from it
- requirements definition even if documented at the project's start in not updated further
- edge-cases identfied in the implementation are not expresseed and documented as the high-level requirements update

_summary:_ The requirements definition and implementation becmes out-of-sync, making implementation testing and future requirements evolution problematic.

**domain data model**

- implementation starts without defining basic bussiness concepts
- different naming of the same semantic implementation assets
- same names of different semantics implementation assets
- even when starting with domain data model definition it instantly becomes detached from the implementation and after few iterations, becomes out-of-sync
- expressing domain model as a diagram is not a common practice becouse of: 
	- quickly becoming out-of-sync
	- need of manual diagram publication after makeing changes

_summary:_ Schema chaos or even no schema at all.

**architecture**

- architecture not defined or documented
- even it architcture is defined, often it is not published and not aviable for all interested stakeholders
- instantly detached from implementation
- after few sprints, the documented architecture is just a museal artefact

_summary:_ Problematic deployment and security risk analysis. Other system integration and next-version design are problematic also.

**data**

- lack of proper and up-to-date domain-model results in multiple data sources in their own schema
- each data source uses its own identifiers, reasoning about data items uniquicity becomes problematic → identifiers hell
- data quality hard to track and keep healthy

_summary:_ Most valuable company's asset - the data is a source of problems and has a risk of not being used effectively.

**code**

- implementation languages operating on low lowel, thus the bussiness abstraction get lost during implementation
- a lot of code not connected directly to the core requirements
- in order to use the data, additional layers like ORM (object to relational mapping) are needed
- can not mix abstraction levels in data queries
- meta-programming feels like additional concept, is hard and non-intuitve
- if code is visualized as diagram:
	- code update does not update the diagram and diagram becomes outdated
	- diagram can not be edited
- not using powerfull techniques such as code generation and code transformation becouse of practical aspects and tooling limitations

_summary:_ A lot of low level code that is not connected with business world - not able to be visulised (to understand what it does) and is not able to be connected with requirements (to identify where the requirements are implemented).

**testing, deployment and security**

- no formal definition of requirements being up-to-date, so there is nothing as a base to define up-to-date test scenarios
- not being able to identify what is the current architecture to design optimal deployment scenario
- security holes analysis hard as the architecture needs to be reverse engineered

_summary:_ Delivery t manager drama. At least if he is aware ☺

**next version**

- hard to get the as-is up-to date and coherent picture
- hard to reason and design next version (to-be) of the system
- as there is no common as-is domain model, it is not designed with future to-be evolution in mind
- after to-be requirements defined, hard to pin-point architectural changes needed
- hard to estimate to-be implementation risk
- hard to execute next version implemention

_summary:_ System evolution at risk.

## Does AI help?

For sure a lot of the above problems seems to be solvable by the AI. 

> With the 2026-01-01 state of the art, we would say that above problems stays the same with AI. 
>
> AI just delivers more junior- and mid- minded set of hands that will automatically deal with the problems.

So maybe we should redefine the IT fundaments and not only believe that AI improvement is the main progress path?

## -zero promise

The problems described aren't isolated issues – they're symptoms of a fundamental mismatch between how we think about systems and how we implement them. **-zero** doesn't just patch these problems; it eliminates the conditions that create them.

Of course there are many need conditions and enablers to be meet to deliver the promise, but we believe it is fundamentally possible.

## The fundamental solution: MVEG

At the heart of **-zero** lies the Meta-Vertex Edge Graph (MVEG) – a data structure so fundamentally simple yet expressive that it collapses the artificial boundaries between requirements, models, code, data, and documentation. When everything shares the same underlying structure, synchronization isn't a problem to solve – it's guaranteed by design.

## How -zero solves each problem

| Problem Area | Traditional IT Problems | The -zero Solution |
|--------------|------------------------|-------------------|
| **Requirements** | Requirements get lost in implementation, never updated, edge cases undocumented, becomes out-of-sync with code | Requirements are living graph structures directly connected to implementation. Add detail at any level – it's one coherent structure. Query requirements, generate tests, document edge cases at requirement level. No drift possible. |
| **Domain Data Model** | No clear domain model, naming chaos, diagrams instantly obsolete, manual publication needed, schema chaos | Domain model is first-class graph database citizen. Design visually or in text – both are same graph. Change any view, all update instantly. Documentation serves actual current state via HTTP – always live. |
| **Architecture** | Undocumented, unpublished, instantly outdated, becomes museum artifact, problematic security analysis | Architecture diagrams are live views of actual system. Model at multiple levels with nested diagrams. Query for security analysis, integration planning. Markdown portal displays current architecture instantly – it reads actual system structure. |
| **Data** | Multiple schemas, identifier hell, quality tracking nightmare, data not used effectively | One graph database, one coherent model, one identifier set. Domain model = data at different meta-levels. No ORM, no translation, no impedance mismatch. Business concepts directly represented. |
| **Code** | Low-level implementation loses business abstraction, can't be visualized, disconnected from requirements, lots of non-business code | Graph Virtual Machine executes at business abstraction levels. Code = diagrams = same graph. Edit as diagram, text, or both simultaneously. Full homoiconicity makes metaprogramming natural. Code generation and transformation trivial. |
| **Testing, Deployment & Security** | No formal requirements base, can't identify current architecture, security holes hard to analyze, reverse-engineering needed | Requirements are formal, queryable structures. Generate tests by querying requirements. Architecture is live, queryable model. Analyze programmatically for security. Deploy as console instances in Docker, REST endpoints, CI/CD pipeline. |
| **Next Version Evolution** | Hard to get current picture, hard to design future, hard to estimate risk, system evolution at risk | As-is state is one coherent queryable graph. Model to-be alongside as-is. Use transformations to explore evolutionary paths. Query architectural deltas. Updatable views work for transformations. |

## Why AI still needs -zero

AI can generate code, write documentation, even create diagrams. But it generates **fragments** – disconnected pieces that inherit all the traditional problems. AI produces more content faster, but content without coherent structure just means more chaos, more quickly.

**-zero** provides the coherent structure AI needs to be truly effective. Instead of generating disconnected code files, AI could generate graph structures that are automatically connected to requirements, architecture, and documentation. Instead of creating static diagrams that immediately obsolesce, AI could manipulate the living graph that is your system.

The future isn't AI replacing developers or **-zero** replacing AI – it's AI and developers both working with coherent graph-based systems where requirements, architecture, models, code, and documentation are unified by design.

## The paradigm shift

**-zero** doesn't fix the old paradigm – it offers a new one:
- **One structure** instead of many (requirements docs, models, code, data)
- **One set of tools** that work everywhere (queries, views, transformations)
- **One truth** that can be viewed differently (diagram, text, data, executable)
- **Zero synchronization** problems because there's nothing to synchronize

This isn't incremental improvement. It's rethinking what a software platform can be when built on fundamentally coherent principles.

The revolution starts with reshaping the basic atom. Everything else follows.




# -zero promise

The problems described aren't isolated issues – they're symptoms of a fundamental mismatch between how we think about systems and how we implement them. **-zero** doesn't just patch these problems; it eliminates the conditions that create them.

## The fundamental solution: MVEG

At the heart of **-zero** lies the Meta-Vertex Edge Graph (MVEG) – a directed cyclic graph where each edge has an additional vertex (meta vertex) instead of a simple label. This seemingly small structural change has profound implications: semantics becomes a first-class citizen, treated exactly like data. When everything – data, metadata, code, models, requirements – shares the same underlying structure and behavior, synchronization isn't a problem to solve, it's guaranteed by design.

MVEG provides extreme simplicity (minimal structural extension to basic graph) with unlimited semantic expressivity. With just three special meta vertices ($Is, $EdgeTarget, $VertexTarget), you can express any complex multi-level hierarchies and relationships. This reduction of ontological complexity means no separate abstraction frameworks, no external definitions, no unnecessary conceptual machinery.

## How -zero solves each problem

| Problem Area | Traditional IT Problems | The -zero Solution | MVEG Enables |
|--------------|------------------------|-------------------|--------------|
| **Requirements** | Requirements get lost in implementation, never updated, edge cases undocumented, becomes out-of-sync with code | Requirements are living graph structures directly connected to implementation. Add detail at any level – it's one coherent structure. Query requirements, generate tests, document edge cases at requirement level. No drift possible. | Arbitrary meta-levels let requirements and implementation coexist as different abstraction levels of the same graph. Semantic relationships between requirement entities work exactly like data relationships. |
| **Domain Data Model** | No clear domain model, naming chaos, diagrams instantly obsolete, manual publication needed, schema chaos | Domain model is first-class graph database citizen. Design visually or in text – both are same graph. Change any view, all update instantly. Documentation serves actual current state via HTTP – always live. Same identifiers, same semantics everywhere. | Semantics is data, data is semantics – domain models have same properties as data: validation, constraints, graphical/textual representation, persistent storage. Unified tooling works at any meta-level. |
| **Architecture** | Undocumented, unpublished, instantly outdated, becomes museum artifact, problematic security analysis | Architecture diagrams are live views of actual system. Model at multiple levels with nested diagrams and meta-information. Query for security analysis, integration planning. Markdown portal displays current architecture instantly – reading actual system structure, not static export. | Ontological unification: no split between "architecture metadata" and "system data". Query semantics same way as data. Mix meta-levels in queries. Triggers on semantic entities can validate architectural constraints. |
| **Data** | Multiple schemas, identifier hell, quality tracking nightmare, most valuable asset becomes problem source | One graph database, one coherent model, one identifier set. Domain model = data at different meta-levels. Data quality rules as graph constraints. No ORM, no translation, no impedance mismatch. Business concepts directly represented using same identifiers across all contexts. | Semantics as first-class citizen means schema and data follow identical rules. Updatable views on semantic entities keep models consistent. Triggers validate data quality automatically. Recursive relationships work naturally. |
| **Code** | Low-level implementation loses business abstraction, can't be visualized, disconnected from requirements, lots of non-business code, meta-programming hard and non-intuitive | Graph Virtual Machine executes at business abstraction levels where code, data, and meta-models coexist with no impedance mismatch. Code = diagrams = text = same graph. Edit in any form simultaneously. Full homoiconicity makes metaprogramming natural – code can inspect/modify itself. Code generation and transformation trivial. | Structural homoiconicity: code and data structures are identical, sharing same ontology. GVM execution structure is what programmer manipulates. Code structure represents execution flow. Metaprogramming is core feature. Finding functions by query is natural. |
| **Testing, Deployment & Security** | No formal requirements base, can't identify current architecture, security holes hard to analyze, reverse-engineering needed, delivery manager drama | Requirements are formal, queryable graph structures. Generate tests by querying requirements and their connected implementations. Architecture is live, queryable model – analyze programmatically for security. Deploy as console instances in Docker, expose REST endpoints, run in CI/CD. | Mix meta-levels in queries: query requirements (high abstraction) connected to implementation (lower abstraction) in single query. Triggers on architectural semantic entities can validate security constraints automatically. |
| **Next Version Evolution** | Hard to get current picture, hard to design future, not designed with evolution in mind, hard to estimate risk, system evolution at risk | As-is state is one coherent queryable graph viewable from any angle. Model to-be alongside as-is using any meta-level shape. Use model transformations to explore paths (product-centric → customer-centric). Query architectural deltas. Updatable views at semantic level keep transformations consistent bidirectionally. | Arbitrary number and shape of meta-levels enable modeling evolution at appropriate abstraction. Updatable views on semantic entities: changes at deeper semantic levels trigger view updates, keeping models consistent. Ontological relationships work across meta-levels. |

## Why AI still needs -zero

AI can generate code, write documentation, even create diagrams. But it generates **fragments** – disconnected pieces that inherit all the traditional problems. AI produces more content faster, but content without coherent structure just means more chaos, more quickly.

**-zero** provides the coherent structure AI needs to be truly effective. Instead of generating disconnected code files, AI could generate graph structures that are automatically connected to requirements, architecture, and documentation. The structural homoiconicity means AI-generated code isn't a separate artifact – it's manipulable graph that shares ontology with requirements and data.

Instead of creating static diagrams that immediately obsolesce, AI could manipulate the living graph that is your system, with changes propagating through updatable views across all meta-levels automatically.

The future isn't AI replacing developers or **-zero** replacing AI – it's AI and developers both working with coherent graph-based systems where requirements, architecture, models, code, and documentation are unified by design, sharing the same structural foundation.

## The paradigm shift

**-zero** doesn't fix the old paradigm – it offers a new one where MVEG's fundamental properties create emergent solutions:

- **One structure** instead of many (requirements docs, models, code, data) → enabled by MVEG's minimal structural extension expressing unlimited complexity
- **One set of tools** that work everywhere (queries, views, transformations) → enabled by unified tooling across all meta-levels
- **One truth** that can be viewed differently (diagram, text, data, executable) → enabled by semantics as first-class citizen
- **Zero synchronization** problems → enabled by ontological unification: no split between data and metadata
- **Zero impedance mismatch** → enabled by structural homoiconicity: code and data share identical structures

This isn't incremental improvement. It's rethinking what a software platform can be when built on fundamentally coherent principles where semantics is structural, not external.

The revolution starts with reshaping the basic atom – adding one meta vertex to each edge. Everything else follows.


# -zero promise

The problems described aren't isolated issues – they're symptoms of a fundamental mismatch between how we think about systems and how we implement them. **-zero** doesn't just patch these problems; it eliminates the conditions that create them.

## The fundamental solution: MVEG

At the heart of **-zero** lies the Meta-Vertex Edge Graph (MVEG) – a directed cyclic graph where each edge has an additional vertex (meta vertex) instead of a simple label. This seemingly small structural change has profound implications: semantics becomes a first-class citizen, treated exactly like data. When everything – data, metadata, code, models, requirements – shares the same underlying structure and behavior, synchronization isn't a problem to solve, it's guaranteed by design.

MVEG provides extreme simplicity (minimal structural extension to basic graph) with unlimited semantic expressivity. With just three special meta vertices ($Is, $EdgeTarget, $VertexTarget), you can express any complex multi-level hierarchies and relationships. This reduction of ontological complexity means no separate abstraction frameworks, no external definitions, no unnecessary conceptual machinery.

## How -zero solves each problem

| Problem Area | Traditional IT Problems | The -zero Solution |
|--------------|------------------------|-------------------|
| **Requirements** | Requirements get lost in implementation, never updated, edge cases undocumented, becomes out-of-sync with code | Requirements are living graph structures directly connected to implementation. Add detail at any level – it's one coherent structure. Query requirements, generate tests, document edge cases at requirement level. No drift possible because arbitrary meta-levels let requirements and implementation coexist as different abstraction levels of the same graph. Semantic relationships between requirement entities work exactly like data relationships. |
| **Domain Data Model** | No clear domain model, naming chaos, diagrams instantly obsolete, manual publication needed, schema chaos | Domain model is first-class graph database citizen. Design visually or in text – both are same graph. Change any view, all update instantly. Documentation serves actual current state via HTTP – always live. Same identifiers, same semantics everywhere. Because semantics is data and data is semantics, domain models have same properties as data: validation, constraints, graphical/textual representation, persistent storage. Unified tooling works at any meta-level. |
| **Architecture** | Undocumented, unpublished, instantly outdated, becomes museum artifact, problematic security analysis | Architecture diagrams are live views of actual system. Model at multiple levels with nested diagrams and meta-information. Query for security analysis, integration planning. Markdown portal displays current architecture instantly – reading actual system structure, not static export. Ontological unification means no split between "architecture metadata" and "system data". Query semantics same way as data. Mix meta-levels in queries. Triggers on semantic entities can validate architectural constraints automatically. |
| **Data** | Multiple schemas, identifier hell, quality tracking nightmare, most valuable asset becomes problem source | One graph database, one coherent model, one identifier set. Domain model = data at different meta-levels. Data quality rules as graph constraints. No ORM, no translation, no impedance mismatch. Business concepts directly represented using same identifiers across all contexts. Semantics as first-class citizen means schema and data follow identical rules. Updatable views on semantic entities keep models consistent. Triggers validate data quality automatically. Recursive relationships work naturally. |
| **Code** | Low-level implementation loses business abstraction, can't be visualized, disconnected from requirements, lots of non-business code, meta-programming hard and non-intuitive | Graph Virtual Machine executes at business abstraction levels where code, data, and meta-models coexist with no impedance mismatch. Code = diagrams = text = same graph. Edit in any form simultaneously. Full structural homoiconicity means code and data structures are identical, sharing same ontology. GVM execution structure is what programmer manipulates. Code structure represents execution flow. Metaprogramming is core feature – code can inspect/modify itself naturally. Code generation and transformation trivial. Finding functions by query is natural. |
| **Testing, Deployment & Security** | No formal requirements base, can't identify current architecture, security holes hard to analyze, reverse-engineering needed, delivery manager drama | Requirements are formal, queryable graph structures. Generate tests by querying requirements and their connected implementations. Architecture is live, queryable model – analyze programmatically for security. Deploy as console instances in Docker, expose REST endpoints, run in CI/CD. Mix meta-levels in queries: query requirements (high abstraction) connected to implementation (lower abstraction) in single query. Triggers on architectural semantic entities can validate security constraints automatically. |
| **Next Version Evolution** | Hard to get current picture, hard to design future, not designed with evolution in mind, hard to estimate risk, system evolution at risk | As-is state is one coherent queryable graph viewable from any angle. Model to-be alongside as-is using any meta-level shape. Use model transformations to explore paths (product-centric → customer-centric). Query architectural deltas. Updatable views at semantic level keep transformations consistent bidirectionally. Arbitrary number and shape of meta-levels enable modeling evolution at appropriate abstraction. Updatable views on semantic entities: changes at deeper semantic levels trigger view updates, keeping models consistent. Ontological relationships work across meta-levels. |

## Why AI still needs -zero

AI can generate code, write documentation, even create diagrams. But it generates **fragments** – disconnected pieces that inherit all the traditional problems. AI produces more content faster, but content without coherent structure just means more chaos, more quickly.

**-zero** provides the coherent structure AI needs to be truly effective. Instead of generating disconnected code files, AI could generate graph structures that are automatically connected to requirements, architecture, and documentation. The structural homoiconicity means AI-generated code isn't a separate artifact – it's manipulable graph that shares ontology with requirements and data.

Instead of creating static diagrams that immediately obsolesce, AI could manipulate the living graph that is your system, with changes propagating through updatable views across all meta-levels automatically.

The future isn't AI replacing developers or **-zero** replacing AI – it's AI and developers both working with coherent graph-based systems where requirements, architecture, models, code, and documentation are unified by design, sharing the same structural foundation.

## The paradigm shift

**-zero** doesn't fix the old paradigm – it offers a new one where MVEG's fundamental properties create emergent solutions:

- **One structure** instead of many (requirements docs, models, code, data) → enabled by MVEG's minimal structural extension expressing unlimited complexity
- **One set of tools** that work everywhere (queries, views, transformations) → enabled by unified tooling across all meta-levels
- **One truth** that can be viewed differently (diagram, text, data, executable) → enabled by semantics as first-class citizen
- **Zero synchronization** problems → enabled by ontological unification: no split between data and metadata
- **Zero impedance mismatch** → enabled by structural homoiconicity: code and data share identical structures

This isn't incremental improvement. It's rethinking what a software platform can be when built on fundamentally coherent principles where semantics is structural, not external.

The revolution starts with reshaping the basic atom – adding one meta vertex to each edge. Everything else follows.