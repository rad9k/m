# Variables

## "non existent" variables usage

Code:

```-0
"Y"
	variable "a" @VertexType
	variable "b" @VertexType
	variable "c" @VertexType
	variable "d" @VertexType
	variable "e" @VertexType
	variable "f" @VertexType

	a = "a"
	b += "b"
	c +< "c"
	d <- "d"
	e <+< "e"
	f <<< "f"
```

Result:
```-0
""
	<@a :: "a">
	<@b :: "">
	<@b :: "b">
	<@c :: "">
		"c"
	<@d :: "d">
	<@e :: "">
		"e"
	<@f :: "">
		"f"
```
Comment:

Seems OK.

## initialized variable usage

Code:

```-0
"Y"
	variable "a" @VertexType
	variable "b" @VertexType
	variable "c" @VertexType
	variable "d" @VertexType
	variable "e" @VertexType
	variable "f" @VertexType

	a = "init"
	b = "init"
	c = "init"
	d = "init"
	e = "init"
	f = "init"

	a = "a"
	b += "b"
	c +< "c"
	d <- "d"
	e <+< "e"
	f <<< "f"
```

Result:
```-0
""
	<@a :: "a">
	<@b :: "init">
	<@b :: "b">
	<@c :: "init">
		"c"
	<@d :: "d">
	<@e :: "">
		"e"
	<@f :: "">
		"f"
```

Comment:
- `<+<` is not `AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex` as the `init` disapeared
- `<<<` is not `AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex ` as the `init` disapeared

## "copied variable usage

Code:

```-0
"Y"
	variable "a" @VertexType
	variable "b" @VertexType
	variable "c" @VertexType
	variable "d" @VertexType
	variable "e" @VertexType
	variable "f" @VertexType

	a = %"copy"
	b = %"copy"
	c = %"copy"
	d = %"copy"
	e = %"copy"
	f = %"copy"

	a = "a"
	b += "b"
	c +< "c"
	d <- "d"
	e <+< "e"
	f <<< "f"
```

Result:
```-0
""
	<@a :: "a">
	<@b :: "copy">
	<@b :: "b">
	<@c :: "copy">
		"c"
	<@d :: "d">
	<@e :: "">
		"e"
	<@f :: "">
		"f"
```

Comment:
- `<+<` is not `AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphAsIsInLeftVertex` as the `copy` disapeared
- `<<<` is not `AddRightEdgesIntoFirstLeftEdgeAndSetStoreForSubGraphIncludingLinksAsIsInLeftVertex ` as the `copy` disapeared