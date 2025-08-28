# Formal Text Language

## General flow

code source string + keyword definition -> result graph

## Keyword string

## $$ meta and other special edges

Those meta edges are used in keyword definition.

### ANY

```-0
<(?<ANY>) :: "new vertex">
```

The `(?<ANY>)` meta edge in keyword definition will match any meta edge to match this keyword in given edge. This is especially usefull when nesting expressions. 

Example:

```-0
<@$Keyword :: ""(?<left>) +<(?<SUB>) (?<right>)">
	<@(?<ANY>) :: "">
		<@$Is :: @AddRightEdgeesIntoLeftEdges>
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

Defines keyword group with a given name.

### $$KeywordGroup

`$$KeuwordGroup` meta edge in the keyword defining vertex, assigns given keyword definition to the given _group name_. 
The _group name_ has to be created with `$$KeywordGroupDefiniion` meta edge.

Example:

```-0
<@$Keyword :: "%<<(?<expr)>>">
	<@$$KeywordGroup :: @GROUP_NAME>
	<@(?<ANY>) :: "">
		<@$$StartInLocalRoot :: @$Empty>
		<@$Is :: @SetIndex>
		<@Expression :: "(?<expr>)">
```

### $$KeywordManyRoot

When the keyword string has `(* ... *)` section, there is a need to define what keyword edge will mach the `(* ... *)` section. If given vertex in the keyword definition contains the `$$KeywordManyRoot` meta edge, it makes given edge the `(* ... *)` section root. That means this edge will be present in the result graph as many times as there are `(* ... *)` section maches in the source text.

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
	<@$Is :: @Method>
	<@InputParameter :: "name">
		<@EdgeTarget :: @String>
	<@InputParameter :: "surname">
		<@EdgeTarget :: @String>
```

### $$LocalRoot and $$StartInLocalRoot 

If vertex in keyword definition contains `$$LocalRoot` meta edge, the vertex is defined as _local root_. 

The `$$LocalRoot` meta edge points to _group name_. The _group name_ has to be created with `$$KeywordGroupDefiniion` meta edge. The _group name_ needs to be mached by value of the `$$KeywordGroup` meta edge in the keyword that is supposed to start in the local root.

Edges resulting from keyword definition containing `$$StartInLocalRoot` meta edge will be added to _local root_, instead of the _default root_.

- _local root_ defining keyword
```-0
<@$Keyword :: "(?<value)">
	<@(?<ANY>) :: "(?<value>)">
		<@$$StartInLocalRoot :: @$Empty>
		<@$Is :: @Query>
		<@NextExpression :: "">
			<@$$LocalRoot :: @GROUP_NAME>
```
- `$$StartInLocalRoot` keyword
```-0
<@$Keyword :: "%<<(?<expr)>>">
	<@$$KeywordGroup :: @GROUP_NAME>
	<@(?<ANY>) :: "">
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
	<@NextExpression :: "">
		<@$Is :: @SetIndex>
			<@Expression :: "5">
```

### $$EmptyKeyword

Keyword definition having `$$NewVertexKeyword` meta edge will result in keyword creting new vertex values keyword.

Example:

```-0
<@$Keyword :: "(?<value>)">
	<@$$EmptyKeyword :: @$Empty>
	<@$$KeywordGroup :: @Empty2Inner>
	<@(?<ANY>) :: "(?<value>)">
		<@$Is :: @Query>
		<@NextExpression :: "">
			<@$$LocalRoot :: "Inner">
```

### $$NewVertexKeyword

Keyword definition having `$$NewVertexKeyword` meta edge will result in keyword creting new vertex values keyword.

Example:

```-0
<@$Keyword :: "(?<value>)">
	<@$$NewVertexKeyword :: @$Empty>
	<@(?<ANY>) :: "(?<value>)">
```

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

Keyword definition having `$$LinkKeyword` meta edge will result in keyword creting new vertex values keyword.

Example:

```-0
<@$Keyword :: "@(?<value>)">
	<@$$KeywordGroup :: @ColonEmptyInner2SlashMarkIndexMethodNewLink>
	<@$$KeywordGroup :: @ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopyFunctionCall>
	<@$$LinkKeyword :: @$Empty>
	<@(?<ANY>) :: "">
		<@$Is :: @Link>
		<@Target :: "(<?<value>)">
```

### $$NonSelfRecursiveParameter

[TBD What is it?]

Example:

```-0
<@$Keyword :: "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopyFunctionCall>) :: (?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopyFunctionCall>)">
	<@$$NonSelfRecursiveParameters :: @$Empty>
	<@(?<ANY>) :: "">
		<@$Is :: DoubleColon>
		<@LeftExpression :: "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopyFunctionCall>)">
		<@RightExpression :: "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopyFunctionCall>)">
		<@NextExpression :: "">
			<@$$LocalRoot :: @InnerCreation>
```

### $$Import

### $$ImportDirect

### $$ImportMeta

### $$ImportDirectMeta

### $$NoSequentialExecution

### $$NextAtomRoot

Used in the parsing. [TBD describe in more details]

`StackFrameCreateor` has `$$NextAtomRoot`
