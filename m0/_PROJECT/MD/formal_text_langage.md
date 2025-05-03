# Formal Text Language

## General flow

code source string + keyword definition -> result graph

## Keyword string

## $$ meta and other special edges

Those meta edges are used in keyword definition.

### ANY

```-0
<(?<ANY>) :: <new vertex>>
```

The `(?<ANY>)` meta edge in keyword definition will match any meta edge to match this keyword in given edge. This is especially usefull when nesting expressions. 

Example:

```-0
<@$Keyword :: ""(?<left>) +<(?<SUB>) (?<right>)">
	<@(?<ANY>) :: >
		<@$Is :: AddRightEdgeesIntoLeftEdges>
		<@LeftExpression :: "(?<left>)">
		<@RightExpression :: "(?<right>)">
```

Above keyword will match following sub graph, even as there is `Next` meta in the root edge. The `Next` meta does not exist in the keyword definition and is matched by `(?<ANY>)`.

```-0
<@Next :: >
	<@$Is :: @AddRightEdgeesIntoLeftEdges>
	<@LeftExpression :: "A">
	<@RightExpression :: "B">
```

### LAST

```-0
<(?<LAST>) :: "new vertex">
```

The meta of new current edge is the meta of last (previously) added edge. This is used in import definitions.

```-0
<@Keyword :: "import (?<name>) (?<link>) meta">
	<@$$ImportMeta :: "import[ ]+%"(?<name>.*)%"[ ]+@(?<link>[^ ]+)[ ]+meta[ ]*\r">
	<@$ImportMeta :: "(?<name>)">
		<@$IsLink :: "$Empty">
		<@$Is :: @$ImportMeta>
	<@(?<LAST>) :: "(?<link>)">
```

### $KeywordGroupDefinition

```-0
<@$KeywordGroupDefinition :: "keyword group name">
```

Defines keyword group.

### $$KeywordGroup

### $$KeywordManyRoot

When the keyword string has `(* ... *)` section, there is a need to define what keyword edge will mach the `(* ... *)` section. The `$$KeywordManyRoot` special meta being present in given edge's child edge, makes given edge the `(* ... *)` section root. That means this edge will be present in the result graph as many times as there are `(* ... *)` section maches in the source text.

Example:
 
- Keyword definition:
```-0
<@$Keyword :: "method (?<name>) (?<returnType>)((*(+, +)(?<paramType>) (?<paramName>)*))">
	<@Method :: "(?<name>)">
		<@$Is :: @Method>
		<@Output :: "(?<returnType>)">
		<@InputParameter :: "(?<paramName>)">
			<$EdgeTarget :: "(?<paramType>)">
			<@$$KeywordManyRoot :: @$Empty>
```
- Code source string:
```-0
method "setName" (@String "name", @String "surname")
```
- Result graph:
```-0							
<@Method :: "setName">
	<@$Is :: Method>
	<@InputParameter :: "name">
		<@EdgeTarget :: @String>
	<@InputParameter :: "surname">
		<@EdgeTarget :: @String>
```

### $$LocalRoot and $$StartInLocalRoot 

In keyword definition, edge containing child edge with the `$$LocalRoot` meta, defines _local root_. 

The vertex value of

Edges resulting from keyword definition containing edges that are having child edge with `$$StartInLocalRoot` as meta will be added to _local root_, instead of the _default root_.

- _local root_ defining keyword
```-0
<@$Keyword :: "(?<value)">
	<@$$EmptyKeyword :: @$Empty>
	<@(?<ANY>) :: "(?<value>)">
		<@$$StartInLocalRoot :: @$Empty>
		<@$Is :: @Query>
		<@NextExpression :: @$Empty>
			<@LocalRoot :: "GROUP_NAME">
```
- `$$StartInLocalRoot` keyword
```-0
<@$Keyword :: "%<<(?<expr)>>">
	<@$$KeywordGroup :: "GROUP_NAME"_>
	<@(?<ANY>) :: @$Empty>
		<@$$StartInLocalRoot :: @$Empty>
		<@$Is :: @SetIndex>
		<@Expression :: "(?<expr>)">
```
- Code source string
```-0
query<<"1">>
```
- Result graph
```-0
<@$Empty :: "a">
	<@$Is :: @Query>
	<@NextExpression :: @$Empty>
		<@$Is :: @SetIndex>
			<@Expression :: "5">
```

### $$EmptyKeyword

### $$NewVertexKeyword

### $$ForceNewVertex

```-0
<@$$ForceNewVertex :: @$Empty>
```

Enforfces to create new vertex string and not link in graph-2-text. This special meta is used in case where `$IsAggregation ::` can not be used.

```-0
<@Keyword :: "attribute (?<name>) (?<type>) (?<MinCardinality>):(?<MaxCardinality>) <<(?<MinValue>):(?<MaxValue>)>>">
	<@Attribte :: "(?<name>)">
		<@$EdgeTarget :: @(?<type>)>
		<@MinValue :: "(?<MinValue>)">
			<@$$ForceNewVertex :: @$Empty>
		<@MaxValue :: "(?<MaxValue>)">
			<@$$ForceNewVertex :: @$Empty>
		<@$MinCardinality :: "(?<MinCardinality>)">
			<@$$ForceNewVertex :: @$Empty>
		<@$MaxCardinality :: "(?<MaxCardinality>)">
			<@$$ForceNewVertex :: @$Empty>
		<@$IsAggregation :: @$Empty>
		<@$Is :: @Attribute>
```

### $$LinkKeyword

### $$NonSelfRecursiveParameter

### $$Import

### $$ImportDirect

### $$ImportMeta

### $$ImportDirectMeta

### $$NoSequentialExecution

### $$NextAtomRoot

Used in the parsing. [TBD describe in more details]

`StackFrameCreateor` has `$$NextAtomRoot`
