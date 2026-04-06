# Generative UX

## Summary

> When a domain model changes, traditional UIs require extensive manual updates across forms, views, and navigation - this project addresses that cost by generating interfaces directly from the live runtime object graph rather than maintaining hand-written UI code around it. The key innovations are runtime generation (so the UI adapts automatically as the model evolves) and object-centric interaction (exposing not just fields and relations but also domain methods). The result is a framework where a pure domain model is enough to produce a functional UI by default, metadata can progressively refine the experience, and the same model can drive multiple interface targets — reducing UI maintenance to a problem of model design rather than repetitive re-encoding.

## The paper

!PDF2[url](https://tereszczuk.com/files/research/tereszczuk_zero.pdf)

## Why this problem matters

A recurring problem in business software is that when the domain model changes, a surprising amount of UX code has to be updated by hand. A new field, a changed relation, a renamed property, or an added operation often means revisiting forms, lists, detail views, labels, navigation, and validation logic. Much of that work is repetitive and low-value, yet it consumes real engineering time and creates friction between model evolution and product evolution.

This project explores how to reduce that synchronization cost by generating user interfaces directly from the runtime object model rather than maintaining large amounts of hand-written UI code around it.

> The core idea is simple: when the model changes, the UI should adapt from the model instead of being manually rewritten around it.

## Main idea

The project is built around the concept of **“in-memory object graph as fully functional software.”** Instead of treating the domain model as something hidden behind a custom UI layer, it treats the object graph itself as the primary source for interaction. The goal is not just to inspect data, but to let users work with objects directly through generated interfaces.

This leads to a more ambitious form of model-driven UI generation: not only generating screens from data structures, but generating usable interaction surfaces from live domain objects.

## What is new here

### 1. Object-centric UI, not just data-centric UI

Many generated UIs are effectively CRUD layers over records. This project takes a stronger position: in object-oriented systems, behavior matters as much as structure. A useful generated UI should therefore expose not only fields and relations, but also methods.

That means users should be able to:

- inspect objects
- edit object state
- navigate relations
- invoke domain operations directly

This shifts generated UI from “forms over tables” toward a real interface for domain behavior.

### 2. Runtime generation instead of design-time scaffolding

Another key idea is that generation should happen **at runtime**, not only during development. In classic scaffolding approaches, generated code becomes yet another artifact to maintain. Here, the UI is produced from the current runtime model when needed.

That makes the approach much more resilient to model evolution. If a type changes, the system does not necessarily require manual UI rewrites in multiple places.

## Metadata as refinement, not a requirement

A practical strength of the project is that it does **not** assume heavy annotation as a starting point. A pure domain model is enough to generate a functional UI. Metadata can then be added to refine the experience where needed.

This allows a good balance:

- functional UI by default
- better UX through progressive metadata enrichment

Metadata can help with things like:

- labels
- descriptions
- grouping
- tabs
- multiline text
- generation hints
- UI-specific customization

That makes the approach realistic: metadata improves the result, but the system does not depend on it to work at all.

## One model, multiple interfaces

The project also shows that one model can drive multiple UI targets. In the implementation, the same model can generate both web and desktop interfaces.

This is important because it suggests that much of UI structure can be captured once at the semantic level and then reused across different rendering technologies. That reduces duplication and helps keep multiple front ends aligned.

## Beyond forms: supporting navigation and workflows

The project is not limited to generating isolated detail screens. It also introduces a session-based interaction model with:

- visualizers for objects and collections
- navigators for schema and object graph traversal
- relations between session items
- support for master-detail interaction
- intensional and extensional navigation

This shows that generated UI can support real workflows over object graphs, not just static editing forms.

## Broader significance

The broader research question behind this work is how much of enterprise UI maintenance is actually product design, and how much of it is avoidable synchronization overhead caused by model changes.

The project argues that a significant part of this cost can be reduced if:

- the runtime model is treated as the primary source of UI generation
- objects remain first-class all the way to the interface
- behavior is exposed alongside structure
- metadata is optional refinement rather than mandatory ceremony
- model semantics are separated from rendering technology

## Takeaway

This project proposes a more direct relationship between model and interface. Instead of repeatedly re-encoding the same domain structure in hand-written UX code, it explores how a live object model can generate a working interface with much lower maintenance overhead.

>In practical terms, the result is a framework architecture for building business software that is:
>
>- more adaptive to model change
>- less dependent on repetitive UI rewrites
>- more object-oriented in its interaction model
>- more reusable across UI targets
>
>The main conclusion is that generated UI becomes much more powerful when it is **runtime-based, object-centric, and grounded in a semantic schema layer rather than raw data binding alone**.