# Formal Text Language

## General flow

code source string + keyword definition -> result graph

## Keyword string

## $$ meta and other special edges

Those meta edges are used in keyword definition.

### ANY

```ZeroCode
<(?<ANY>) :: "new vertex">
```

The `(?<ANY>)` meta edge in keyword definition will match any meta edge to match this keyword in given edge. This is especially usefull when nesting expressions. 

Example:

```ZeroCode
<@$Keyword :: ""(?<left>) +<(?<SUB>) (?<right>)">
	<@(?<ANY>) :: "">
		<@$Is :: @AddRightEdgeesIntoLeftEdges>
		<@LeftExpression :: "(?<left>)">
		<@RightExpression :: "(?<right>)">
```

Above keyword will match following sub graph, even as there is `Next` meta in the root edge. The `Next` meta does not exist in the keyword definition and is matched by `(?<ANY>)`.

```ZeroCode
<@Next :: >
	<@$Is :: @AddRightEdgeesIntoLeftEdges>
	<@LeftExpression :: "A">
	<@RightExpression :: "B">
```

### LAST

```ZeroCode
<(?<LAST>) :: "new vertex">
```

The meta of new current edge is the meta of last (previously) added edge. This is used in import definitions.

```ZeroCode
<@Keyword :: "import (?<name>) (?<link>) meta">
	<@$$ImportMeta :: "import[ ]+%"(?<name>.*)%"[ ]+@(?<link>[^ ]+)[ ]+meta[ ]*\r">
	<@$ImportMeta :: "(?<name>)">
		<@$IsLink :: "$Empty">
		<@$Is :: @$ImportMeta>
	<@(?<LAST>) :: "(?<link>)">
```

### $KeywordGroupDefinition

```ZeroCode
<@$KeywordGroupDefinition :: "keyword group name">
```

Defines keyword group with a given name.

### $$KeywordGroup

`$$KeuwordGroup` meta edge in the keyword defining vertex, assigns given keyword definition to the given _group name_. 
The _group name_ has to be created with `$$KeywordGroupDefiniion` meta edge.

Example:

```ZeroCode
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
```ZeroCode
<@$Keyword :: "method (?<name>) (?<returnType>)((*(+, +)(?<paramType>) (?<paramName>)*))">
	<@Method :: "(?<name>)">
		<@$Is :: @Method>
		<@Output :: "(?<returnType>)">
		<@InputParameter :: "(?<paramName>)">
			<$EdgeTarget :: "(?<paramType>)">
			<@$$KeywordManyRoot :: @$Empty>
```
- Code source string:
```ZeroCode
method "setName" (@String "name", @String "surname")
```
- Result graph:
```ZeroCode							
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
```ZeroCode
<@$Keyword :: "(?<value)">
	<@(?<ANY>) :: "(?<value>)">
		<@$$StartInLocalRoot :: @$Empty>
		<@$Is :: @Query>
		<@NextExpression :: "">
			<@$$LocalRoot :: @GROUP_NAME>
```
- `$$StartInLocalRoot` keyword
```ZeroCode
<@$Keyword :: "%<<(?<expr)>>">
	<@$$KeywordGroup :: @GROUP_NAME>
	<@(?<ANY>) :: "">
		<@$$StartInLocalRoot :: @$Empty>
		<@$Is :: @SetIndex>
		<@Expression :: "(?<expr>)">
```
- Code source string
```ZeroCode
query<<"5">>
```
- Result graph
```ZeroCode
<@$Empty :: "query">
	<@$Is :: @Query>
	<@NextExpression :: "">
		<@$Is :: @SetIndex>
			<@Expression :: "5">
```

### $$EmptyKeyword

Keyword definition having `$$NewVertexKeyword` meta edge will result in keyword creting new vertex values keyword.

Example:

```ZeroCode
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

```ZeroCode
<@$Keyword :: "(?<value>)">
	<@$$NewVertexKeyword :: @$Empty>
	<@(?<ANY>) :: "(?<value>)">
```

### $$ForceNewVertex

```ZeroCode
<@$$ForceNewVertex :: @$Empty>
```

Enforfces to create new vertex string and not link in graph-2-text. This special meta is used in case where `$IsAggregation ::` can not be used.

```ZeroCode
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

```ZeroCode
<@$Keyword :: "@(?<value>)">
	<@$$KeywordGroup :: @ColonEmptyInner2SlashMarkIndexMethodNewLink>
	<@$$KeywordGroup :: @ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>
	<@$$LinkKeyword :: @$Empty>
	<@(?<ANY>) :: "">
		<@$Is :: @Link>
		<@Target :: "(<?<value>)">
```

### $$NonSelfRecursiveParameter

[TBD What is it?]

Example:

```ZeroCode
<@$Keyword :: "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>) :: (?<right_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)">
	<@$$NonSelfRecursiveParameters :: @$Empty>
	<@(?<ANY>) :: "">
		<@$Is :: DoubleColon>
		<@LeftExpression :: "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)">
		<@RightExpression :: "(?<left_ColonEmptyInner2SlashMarkIndexMethodNewLinkBracketCopy>)">
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
