# Meta-Vertex Edge Graph (MVEG) - Summary

> MVEG = directed cyclic graph where edges use meta vertices instead of edge's labels.

## Example model

```ZeroCode
<@Class :: "Person">
	<@Attribute :: "Name">
	<@Attribute :: "Surname">
	<@Attribute :: "HairLength">
```

Above code defines:
- a `Class` having name `Person` (the class is represented by a _vertex_). This class would be our example model.
- `Name` _vertex_ connected to the `Person` _vertex_ by the edge having `Attribute` as a meta edge
- `Surname` _vertex_ connected to the `Person` _vertex_ by the edge having `Attribute` as a meta edge
- `HairLength` _vertex_ connected the `Person` _vertex_ by the edge having `Attribute` as a meta edge

> The syntax is `<META :: TO>` and `<@VERTEX :: "VALUE">` where:
> - `<META :: TO>` defines a edge having `META` as a meta edge, and having `TO` as a target _vertex_
> - `@VERTEX` defines a link (reference) to _vertex_ represented by a `VERTEX` query
> - `"VALUE"` creates a _new vertex_ and sets its value to `VALUE`

## Example instances

```ZeroCode
<@Person :: "">
	<@Name :: "Radek">
	<@Surname :: "Tereszczuk">
	<@HairLength :: "10">
<@Person :: "">
	<@Name :: "Maurycy">
	<@Surname :: "Tereszczuk">
	<@HairLength :: "7">
```

Above code defines:
- a `Person` class instance (the instance is represented by a _vertex_)
- `Radek` _vertex_ connected to the `Person` instance _vertex_ by the edge having `Name` as a meta edge
- `Tereszczuk` _vertex_ connected to the `Person` instance _vertex_ by the edge having `Surname` as a meta edge
- `10` _vertex_ connected to the `Person` instance _vertex_ by the edge having `HairLength` as a meta edge
- the same, but for `Maurycy`, `Tereszczuk` and `HairLength` of `7`

> Above, there are two data object instances and each of them is reffering to the same _meta vertexes_ → the same model _vertexes_ are used in many places as descriptions of many instance's _edges_

## Model's vertex becomes edge's description in the instance

> Mind that the instance graph has a reference to the model. Each instance's _edge_ has a _meta vertex_ that describes its semantics. This _meta vertex_ is just a normal _vertex_ but defined as part of the model that describes the instance.

**How does it work?**

- in the whole graph, we migh have defined different fragments:
	- one of the graph's fragments contains instances
	- other fragment contains model that describe them
- model is defined by vertexes conected by edges
- instances are also defined by vertexes conected by edges
- vertexes defined in model can be used to describe edges between the instance's vertexes

## How it is connected

## More than labels: adding meta data to the model

Above `Person` class model is very simple. It does not provide any additional information to the concepts (like `Name`) being references from the data instances. For example the above model does not define data-types for the `Attribute`s. Let's add them.

```ZeroCode
<@Class :: "Person">
	<@Attribute :: "Name">
		<@$EdgeTarget :: @String>
	<@Attribute :: "Surname">
		<@$EdgeTarget :: @String>
	<@Attribute :: "HairLength">
		<@$EdgeTarget :: @Integer>
```

Above code defines:
- Same as above: `Person` class; `Name`, `Surname` and `HairLength` attributes
- for the `Name` and `Surname` the vertex value type is `String`
- for the `HairLength` the vertex value is `Integer`
- the _to vertex_  (for example: `Radek`) value type that is being described by the given _meta vertex_ (for example: `Name`) is defined by the `$EdgeTarget` meta having _edge_ from the given _meta vertex_ (for example: `Name`) to the _vertex_ representing given type (for example: `String`)

> The syntax is `<META :: TO>` and `<@VERTEX :: @VERTEX>` where:
> - `<META :: TO>` defines a edge having `META` as meta edge, and having `TO` as target _vertex_
> - `@VERTEX` defines a link (reference) to _vertex_ represented by a `VERTEX` query

## Let's add meta model


Abowe, we have only one meta relation: between `Person` class and it's instances (data). Can we add another meta level? Yes. In our case it will be a model for a class → it will me a meta-model.

```ZeroCode
"Class"
	"Attribute"
		<@$VertexTarget :: @Type>
```

Above code defines:
- A `Class` _vertex_
- The `Class` _vertex_ has _edge_ poiting to the `Attribute` _vertex_
- The `Attribute` _vertex_ has _edge_ poiting to the `Type` _vertex_. The _edge_ has `$VertexTarget` as _meta vertex_.

> `<@$VertexTarget :: @TYPE>` translates into `<@$EdgeTarget :: @IS_OF_TYPE>` on higher meta level. Additionally the `IS_OF_TYPE` _vertex_ have the `<@$Is :: @TYPE>` _edge_, with means that `IS_OF_TYPE` _vertex_ is of `TYPE` _vertex_ type.

## How it is connected - with meta model

The combined result is:

```ZeroCode
"Class"
	"Attribute"
		<@$VertexTarget :: @Type>
<@Class :: "Person">
	<@Attribute :: "Name">
		<@$EdgeTarget :: @String>
	<@Attribute :: "Surname">
		<@$EdgeTarget :: @String>
	<@Attribute :: "HairLength">
		<@$EdgeTarget :: @Integer>
<@Person :: "">
	<@Name :: "Radek">
	<@Surname :: "Tereszczuk">
	<@HairLength :: "10">
<@Person :: "">
	<@Name :: "Maurycy">
	<@Surname :: "Tereszczuk">
	<@HairLength :: "7">
```

The above code (in order):
- Defines a model that describes how to define a `Class`. This is a meta-model.
- Defines a `Person` `Class`. This is a model. The `Person` uses two conecpts, from the meta-model: `Class` and `Attribute` - those are used as _meta vertexes_ in the _edges_ of the `Person` defining graph fragment.
- Defines two `Person` class instances. Both instances share the same concepts from the `Person` class model: `Person`, `Name`, `Surname` and `HairLength`  - those are used as _meta vertexes_ in the _edges_ of the `Person` class instances defining graph fragment.