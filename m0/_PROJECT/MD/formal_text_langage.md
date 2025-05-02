# Formal Text Language

## $$ meta and other special edges

Those meta edges are used in keyword definition.

### ANY

	<(?<ANY>) :: <new vertex>>

The `(?<ANY>)` meta edge in keyword definition will match any meta edge to match this keyword in given edge. This is especially usefull when nesting expressions. 

Example:

	<@$Keyword :: ""(?<left>) +<(?<SUB>) (?<right>)">
		<@(?<ANY>) :: >
			<@$Is :: AddRightEdgeesIntoLeftEdges>
			<@LeftExpression :: "(?<left>)">
			<@RightExpression :: "(?<right>)">

Above keyword will match following sub graph, even as there is `Next` meta in the root edge. The `Next` meta does not exist in the keyword definition and is matched by `(?<ANY>)`.

	<@Next :: >
		<@$Is :: @AddRightEdgeesIntoLeftEdges>
		<@LeftExpression :: "A">
		<@RightExpression :: "B">

### LAST

	<(?<LAST>) :: "new vertex">

The meta of new current edge is the meta of last (previously) added edge. This is used in import definitions.

	<@Keyword :: "import (?<name>) (?<link>) meta">
		<@$$ImportMeta :: "import[ ]+%"(?<name>.*)%"[ ]+@(?<link>[^ ]+)[ ]+meta[ ]*\r">
		<@$ImportMeta :: "(?<name>)">
			<@$IsLink :: "$Empty">
			<@$Is :: @$ImportMeta>
		<@(?<LAST>) :: "(?<link>)">

### $KeywordGroupDefinition

	<@$KeywordGroupDefinition :: "keyword group name">

Defines keyword group.

### $$KeywordGroup

### $$KeywordManyRoot

Example:

	<@$Keyword :: "method (?<name>) (?<returnType>)((*(+, +)(?<paramType>) (?<paramName>)*))">
		<@Method :: "(?<name>)">
			<@$Is :: @Method>
			<@Output :: "(?<returnType>)">
			<@InputParameter :: "(?<paramName>)">
				<$EdgeTarget :: "(?<paramType>)">
				<@$$KeywordManyRoot :: @$Empty>


### $$LocalRoot

### $$StartInLocalRoot

### $$EmptyKeyword

### $$NewVertexKeyword

### $$ForceNewVertex

	<@$$ForceNewVertex :: @$Empty>

Enforfces to create new vertex string and not link in graph-2-text. This special meta is used in case where `$IsAggregation ::` can not be used.

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
