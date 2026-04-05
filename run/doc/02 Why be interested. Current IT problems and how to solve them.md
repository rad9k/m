# Why be interested. Current IT problems and how to solve them

-zero unifies requirements, code, data, and architecture into a single queryable graph, making IT systems transparent, consistent, and instantly analyzable.

Modern IT systems are plagued by fragmentation—requirements, architecture, code, and data drift apart, creating inefficiency, risk, and complexity. -zero, powered by the Fractal Graph, unifies all system elements into a single, coherent, queryable structure, allowing instant visualization, real-time updates, and seamless analysis across every layer. This is not just an improvement—it’s a paradigm shift, turning IT chaos into clarity and enabling systems that are smarter, more adaptable, and future-ready from day one.

> in -zero **architecture becomes (higher level of) implementation, requirements become code scaffolding filled with technical details, eliminating requirements ⇔ implementation ⇔ architecture synchronization hell**

-zero is a fundamental game-change. The revolution starts with reshaping the basic universe atom - fundamental data structure. The Fractal Graph brings simplicity, coherence and extreme expression power. And it shines everywhere used. Complex structures becomes more coherent and transparent. Universal properties and behaviour enables same powerfull mechanics (like meta-data, queries and updatable views) and tooling (like diagram view/edit, extensive visualisation) for every asset in the platform.

**_Fractal Graph when applied to old IT concepts like database, query language, model, meta-model, virtual machine and programming language results in fundamental changes among those concepts and system paradigm-shift:_**

> **old** (_current IT_): abstraction is lost in the implementation
>
> **new** (_-zero_): abstraction and its implementation are just two views of the same interconnected Fractal Graph structure

## The problems

Modern IT and especially computer systems creation and maitanance has several fundamental problems:

### Requirements

**_Problem:_ Requirements and implementation become fatally out-of-sync**

- Requirements definition is lost in the implementation - the "why" disappears, leaving only the "how"
- Implementation itself is too low-level to recreate requirements from it
- Requirements documentation, even when created at project start, is never updated as the system evolves
- Edge cases discovered during implementation remain buried in code - they never flow back to update high-level requirements
- After a few iterations, requirements documents become historical artifacts with no connection to actual system behavior

_**Impact:**_ Testing becomes guesswork (what should we actually test?), and future requirements evolution is nearly impossible without archaeological code analysis.

### Domain data model

**_Problem:_ Schema chaos - or no schema at all**

- Implementation starts without defining basic bussiness concepts first
- The same concept gets different names across the codebase (semantic inconsistency)
- Different concepts share the same names (naming collisions)
- Even when teams start with a proper domain model, it immediately detaches from implementation
- After a few iterations, the documented model bears no resemblance to actual data / code structure
- Domain model diagrams aren't maintained because:
	- They become outdated within days
	- Publishing updated diagrams requires manual work nobody has time for

_**Impact:**_ Semantic confusion across the team. Nobody knows what terms mean anymore. Data structures proliferate with unclear relationships.

### Architecture

**_Problem:_ Architecture exists only in tribal knowledge**

- Architecture not defined or documented
- Even if architcture is defined, often it is not published and not available for all interested stakeholders
- Instantly detached from implementation
- After a few sprints, architecture documents become museum pieces - interesting historically, but not trustworthy

_**Impact:**_ Deployment becomes trial-and-error. Security risk analysis is impossible without current architecture. System integration with other systems is problematic. Designing the next version requires reverse-engineering the current one.

### Data

**_Problem:_ The company's most valuable asset becomes a liability**

- Lack of proper and up-to-date domain-model results in multiple data sources with their own schema
- Each data source uses different identifiers for the same entities - reasoning about data uniqueness becomes impossible (identifier hell)
- Data quality is hard to track and maintain without a clear model

_**Impact:**_ Most valuable company's asset - the data is a source of problems and has a risk of not being used effectively.

### Code

**_Problem:_ Mountains of low-level code not connected to business logic**

- Implementation languages operate at too low a level - business abstractions get lost in technical details
- Most code isn't directly connected to core requirements (infrastructure, glue code, boilerplate)
- Additional layers like ORM (Object-Relational Mapping) are needed just to work with data
- Can't mix abstraction levels in data queries
- Meta-programming feels like additional concept, is hard and non-intuitve
- Code visualization as diagrams fails because:
	- Non-functional complex action diagrams due to lack of UX maturity
	- Code updates don't update diagrams automatically - diagrams become outdated immediately
	- Diagrams can't be edited to update code - one-way generation only
- Powerful techniques like code generation and code transformation aren't used due to practical limitations and poor tooling

_**Impact:**_ A lot of low-level code that is not connected with business world - not able to be visulised (_to understand what it does_) and not being able to be connected with requirements (_to identify where the requirements are implemented_).

### Testing, deployment and security

**_Problem:_ Testing without truth, deploying without maps, securing without knowledge**

- No formal definition of requirements being up-to-date, so there is nothing as a base to define up-to-date test scenarios
- Can't identify current architecture, making optimal deployment design impossible
- Security analysis requires reverse-engineering the architecture first - holes remain hidden

_**Impact:**_ Delivery manager drama. At least if they're aware of the problems ☺

### Next Version / System Evolution

**_Problem:_ Building the future on quicksand**

- Nearly impossible to get an accurate, coherent, up-to-date picture of the current system (as-is)
- Can't reason about or design the next version (to-be) without understanding current state
- Without a common as-is domain model, the system wasn't designed with future evolution in mind
- After defining to-be requirements, can't pinpoint what architectural changes are actually needed
- Risk estimation for implementation is largely guesswork
- Hard to execute next version implemention

_**Impact:**_ System evolution itself is at risk. Companies get locked into their current systems, unable to adapt to changing business needs.

## Does AI help?

For sure a lot of the above problems seems to be solvable by the AI. With the 2026-01-01 state of the art, we would say that above problems stays the same with AI and... 

> ...AI just delivers more junior- and mid- minded set of hands that try to automatically deal with the above problems.

So maybe instead of putting all the eggs in the AI nest, we should try to redefine the IT systems fundaments? Perhaps than there will be less work (and errors) for AI powered devs. **This is especially critical when implementing IT systems evolution to catch changeing business needs.**

## The paradigm shift

-zero doesn't fix the old paradigm – it offers a new one thanks to Fractal Graph minimal structural extension that brings unlimited complexity handling. Fractal Graph applied to classical ideas like database, query language, model, meta-model, virtual machine and programming language creates emergent solution with coherent properties:

- **One structure** instead of many (requirements docs, architecture models, code, data). Ontological unification: no split between data and metadata.
- **No synchronization problems** and **One truth**. No need to sync assets, as we just have multiple views (diagrams, text) of the same graph structure.
- **One set of tools** that work everywhere (queries, views, transformations). Unified tooling across all meta-levels.
- **Zero impedance mismatch**. Structural homoiconicity: code and data share identical structures

> Architecture is implementation, implementation is adding details to the requirements.

This isn't incremental improvement. It's rethinking what a software platform can be when built on fundamentally coherent principles where semantics is structural, not external.

## -zero promise

| Problem area | Traditional IT problems | The -zero solution |
|--------------|------------------------|-------------------|
| **Requirements** | Requirements: get lost in implementation, never updated, edge cases undocumented, becomes out-of-sync with code | If the requirements contain business objects or process defintion, the implementation is just adding more details to the requirements - adding additional layer of meta data that deals with the implementation details. If the requirements can not be easiely annotated with implementation details, additional intermediate layer will be helpfull in linking requirements with implementation assets. |
| **Domain data model** | No clear domain model, naming chaos, diagrams instantly obsolete (if any), manual publication needed, schema chaos | All data stored in -zero needs a model that describes them, so there is always data model that is 100% synced with the reality - data instances. Domain model is first-class graph database citizen - designed visually or in text. Data models can be result of automatic model transformation provided by updatable views functionality - also if the data comes from some other systems, can annotate models with source system fields and have the ETL code being auto-generated after each change. Documentation showes actual model state via HTTP web server. |
| **Architecture** | Undocumented, unpublished, instantly outdated, becomes museum artifact, problematic to analyze |  In -zero architecture models are the same thing as higher abstraction levels of implementation - in fact there is no architecture ⇔ implementation boudary anywhere so this is up to the user what parts of the system will be visible in given diagrams. With the use of diagram embedding, combining scales and zoom feature there are infinite number of scenarios how to deal with system visualisation, design and editing. To manage the complexity there are various diagramming items and diagram embedding scenarios to use. Query for security analysis, integration planning. Markdown portal displays current architecture instantly. Ontological unification means no split between "architecture metadata" and "system data". Query semantics same way as data. Mix meta-levels in queries. Triggers on semantic entities can validate architectural constraints automatically. |
| **Data** | Multiple schemas, identifier hell, quality tracking nightmare, most valuable asset becomes problem source | Data schema modelling is a base -zero scenario with rich tooling and use case support. Also -zero database data instances are always connected with their schema, so there is everything in right order by design. As for the identifiers: other source systems existence and mapping rules of different source systems identifiers can become part of the data meta-model and than be used in auto-generated code handling ETL, identifiers comparsion and translation or data quality tracking. |
| **Code** | Low-level implementation loses business abstraction, can't be visualized, is disconnected from requirements. Meta-programming hard and non-intuitive | Graph Virtual Machine works at business abstraction levels where code, data, and meta-models coexist with no impedance mismatch. Code = diagrams = text = same graph. Edit in any form simultaneously. As architecture = implementation, it is more of a very, very important qustion: _"what abstractions to start from"_, and the rest will follow. Full structural homoiconicity means code and data structures are identical, sharing same ontology. GVM execution structure is what programmer manipulates. Code structure represents execution flow. Metaprogramming is a core feature – code can inspect/modify itself naturally. Code generation and transformation is a breese. |
| **Testing, Deployment & Security** | No formal requirements base, can't identify current architecture, security holes hard to analyze, reverse-engineering needed, delivery manager drama | Requirements are up-to-date queryable graph structures connected with the implementation. Architecture, being just implementation more high-level part, is also live, queryable model – analyzable for security or integration scenarios. Can generate deployment scripts directly out of architcture model where there is no split between "architecture metadata" and "system data". |
| **Next Version Evolution** | Hard to get current picture, hard to design future, systems not designed with evolution in mind, hard to estimate risk, system evolution at risk | As-is state is one coherent queryable graph viewable from any angle. System to-be alongside as-is can use any meta-levels shape. Next version implementation itself is much easier as in most cases it will be enough to make changes in one place and have them automatically reused and applied on all the architecture levels, and if that is not the case, the programable updatable views will do the "change in one place, have the effect everywhere" job effectively. |

## Summary

> -zero unifies requirements, code, data, and architecture into a single graph, eliminating synchronization chaos and the drift between documentation and reality. Through the Fractal Graph structure, architecture becomes implementation and requirements become code scaffolding—everything is the same graph viewed from different perspectives. This isn't an improvement of current IT, but a fundamental paradigm shift where abstraction and implementation are no longer separate worlds requiring constant manual alignment.