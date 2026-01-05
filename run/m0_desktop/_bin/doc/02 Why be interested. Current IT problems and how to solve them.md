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

For sure a lot of the above problems seems to be solvable by the AI. With the 2026-01-01 state of the art, we would say that above problems stays the same with AI and... 

> ...AI just delivers more junior- and mid- minded set of hands that will automatically deal with the problems.

So maybe instead of putting all the egs in the AI nest, we should try to redefine the IT systems fundaments?

## The paradigm shift

-zero doesn't fix the old paradigm – it offers a new one thanks to MVEG minimal structural extension that brings unlimited complexity handling. MVEG applied to classical ideas like database, query language, model, meta-model, virtual machine and programming language creates emergent solution with coherent properties:

- **One structure** instead of many (requirements docs, architecture models, code, data). Ontological unification: no split between data and metadata.
- **No synchronization problems** and **One truth**. No need to sync assets, as we just have multiple views (diagrams, text) of the same graph structure. Architecture is implementation, implementation is adding details to the requirements.
- **One set of tools** that work everywhere (queries, views, transformations). Unified tooling across all meta-levels.
- **Zero impedance mismatch**. Structural homoiconicity: code and data share identical structures

This isn't incremental improvement. It's rethinking what a software platform can be when built on fundamentally coherent principles where semantics is structural, not external.

The revolution starts with reshaping the basic atom – adding one meta vertex to each edge. Everything else follows.

## -zero promise

| Problem area | Traditional IT problems | The -zero solution |
|--------------|------------------------|-------------------|
| **Requirements** | Requirements: get lost in implementation, never updated, edge cases undocumented, becomes out-of-sync with code | If the requirements contain business objects or process defintion, the implementation is just adding more details to the requirements - adding additional layer of meta data that deals with the implementation details. If the requirements can not be easiely annotated with implementation details, additional intermediate layer will be helpfull in linking requirements with implementation assets. |
| **Domain data model** | No clear domain model, naming chaos, diagrams instantly obsolete (if any), manual publication needed, schema chaos | All data stored in -zero needs a model, so there is always data model that is 100% synced with the reality - data instances. Domain model is first-class graph database citizen - designed visually or in text. Data models can be result of automatic model transformation provided by updatable views functionality - also if the data comes from some other systems, can annotate models with source system fields and have the ETL code being auto-generated after each change. Documentation showes actual model state via HTTP web server. |
| **Architecture** | Undocumented, unpublished, instantly outdated, becomes museum artifact, problematic to analyze |  In -zero architecture models are the same thing as higher abstraction levels of implementation - in fact there is no architecture / implementation boudary anywhere so this is up to the user what parts of the system will be visible in given diagrams. With the use of diagram embedding, combining scales and zoom feature there are infinite number of scenarios how to deal with system visualisation, design and editing. To manage the complexity there are various diagramming items and diagram embedding scenarios to use. Query for security analysis, integration planning. Markdown portal displays current architecture instantly. Ontological unification means no split between "architecture metadata" and "system data". Query semantics same way as data. Mix meta-levels in queries. Triggers on semantic entities can validate architectural constraints automatically. |
| **Data** | Multiple schemas, identifier hell, quality tracking nightmare, most valuable asset becomes problem source | Data schema modelling is a base -zero scenario with rich tooling and use case support. Also -zero database data instances are always connected with their schema, so there is everything in right order by design. As for the identifiers: other source systems existence and mapping rules of different source systems identifiers can become part of the data meta-model and than br used in auto-generated code handling ETL, identifiers comparsion and translation or data quality tracking. |
| **Code** | Low-level implementation loses business abstraction, can't be visualized, is disconnected from requirements. Meta-programming hard and non-intuitive | Graph Virtual Machine works at business abstraction levels where code, data, and meta-models coexist with no impedance mismatch. Code = diagrams = text = same graph. Edit in any form simultaneously. As architecture = implementation, it is more of a very very important qustion "what abstractions to start from", and the rest will follow. Full structural homoiconicity means code and data structures are identical, sharing same ontology. GVM execution structure is what programmer manipulates. Code structure represents execution flow. Metaprogramming is core feature – code can inspect/modify itself naturally. Code generation and transformation is a breese. |
| **Testing, Deployment & Security** | No formal requirements base, can't identify current architecture, security holes hard to analyze, reverse-engineering needed, delivery manager drama | Requirements are up-to-date queryable graph structures connected with the implementation. Architecture, being just implementation more high-level part, is also live, queryable model – analyzable for security or integration scenarios. Can generate deployment scripts directly out of architcture model where there is no split between "architecture metadata" and "system data". |
| **Next Version Evolution** | Hard to get current picture, hard to design future, systems not designed with evolution in mind, hard to estimate risk, system evolution at risk | As-is state is one coherent queryable graph viewable from any angle. System to-be alongside as-is can use any meta-levels shape. Next version implementation itself is much easier as in most cases it will be enough to make changes in one place and have them automatically reused and applied on all the architecture levels, and if that is not the case, the programable updatable views will do the "change in one place, have the effect everywhere" job effectively. |

