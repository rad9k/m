# common user summary

- Windows desktop executable with WPF front-end

- you can draw pretty much any diagrams

- you can create inter diagram dependencies

- you can query diagrams and all their properites

- you can define your own type of diagrams

- you can write and execute programs in some crazy programming language
	- that programming language is just example of new class of programming languages called graph programming languages

- you can create your own graph programming language with unique syntax

- you can view and edit your code as diagrams _(code = diagram)_
	- this is not _generating_ diagrams, the **diagrams are the code**

- you can do crazy stuff like perform algebraic operations on the programming languages body or inherit keywords

- **in the end you should be able to create, execute and maintain super complex back-end REST services**

- there are two example user programs based on -0
	- **ZeroComposer** - complex MIDI sequencer
	- **LoveFlow** - complex economies simulator
	
# modeling phreak tales :)

## what is it?

- **Modelling** environment

- **Meta modelling** environment > create modelling environments

- Model transformation engine

- General purpose execution environment **(graph virtual machine)** 
	- To be used as Back-end mainly 
	- Front-end also, currently only on desktop but web browser front-end is planned

- Semi transacted **Graph DB** (in future distributed)
	- Powerfull **query** language 
	- Graph virtual machine in the db
	- Can do anything with the data

## main properties

- Based on **meta graph** _(abstraction of normal mathematical graph)_

- Any number of _meta_ if you need

- **Same tools** / abstractions / language at **any level** of _meta_ 

- Meta models, models and data instances are same class citizens

- **Interconnections** between _meta_ levels

- **Text = Diagram = Graph**
	- **Universal diagram engine** (can define any _shapes <> graph_ mapping)
	- **Universal text languages engine** (can define any _text <> graph_ mapping)

- Updatable views on graph, thus **auto updatable model transformations**
	- **executable views on code!**

## why anyone could need it?

- **Better software build process** through increasing level of abstraction 
	- Possibly faster / easier to maintain

- Software better suited to solve business cases as the **implementation level of
abstraction is closer to business case**

- Possibly will show **power when working on vNext** of business case
implementation. Thanks to 
	- Making changes 
	- Dependency tracking 
	- Models auto update

- Core IT research 
	- New class of programming langueges (**graph programming languages**)
	- Keywords inheritance?
	- **Alebra of programming languages**
	- To have usable **graphical imperative code representation**

# current state

_updated on 21.08.2023_

The -0 environment works and is pretty much ready to use. Even on production, althought it is not recemended by many resons _(first - not possible to host back end services yet)_.

**The biggest drawback of current project state is the lack of any documentation (besides this file).**

The full documentation (including "how to build") will be provided after current back log will reach 100%. This should happen in 1-2 month time frame and for sure in 2023.

In orginal backlog there was around 50 features, but due the need of releasing of stable public version 1.0, some hard decisions vere made. Current back log (updated):

_bold = feature ready_

1) Code Diagram
	- **MultiContainer** 
	- **CodeBlock**
	- ZeroCode <> diagram (template)

2) Selected View
	- Form on right
	- Code on bottom
	- Diagram supports Edge / Item switch for SelectedEdges

3) Parser update
	- multiplicity bug
	- proper parsing where there are errors now
	- empty line handling
	- m0t support

4) View support in ZeroCode
	- view creation language support
	- update creation language support

5) HTTP Server
	- REST end point creation support
	- markdown Visualiser @WPF
	- Web Server

# contact

For troubleshuting or any other issues related to -0, please contact me at _tereszczuk@gmail.com_