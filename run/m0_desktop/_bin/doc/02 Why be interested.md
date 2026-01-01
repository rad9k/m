# Why be interested

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

and semantic scope of the implementation assets




AI helps solving them but in fact


-zero is not just one feature solution fixing one problem. It tries to fix many 