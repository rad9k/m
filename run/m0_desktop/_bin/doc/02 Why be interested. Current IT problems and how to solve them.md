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