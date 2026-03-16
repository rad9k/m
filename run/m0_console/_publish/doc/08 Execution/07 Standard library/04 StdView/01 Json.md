# Json

## The MVEG → Json impedance mismatch

Most of the MVEG → Json impedance mismatch comes from the fact that JSON is a tree structure (only one "parent" for a node) and MVEG is a graph (multiple "parents" for _vertex_, cycles allowed). 

> There is another important impedance mismatch factor that is more subtle - Json operates on a key / value concept, where the value is atomic and can not have a "internal" list of key / values. 

This means that althought we can map a MVEG like this:

```ZeroCode
<@META :: "VALUE">
```

to the following Json

```
"META":"VALUE"
```

We are not able to map following MVEG:

```ZeroCode
<@META :: "VALUE">
	<@META :: "SUB">
```

There are a few possible mapping scenarios for above MVEG of which the following is used:

```
{
  "Meta": {
    "Meta": "SUB"
  }
}
```	

## MVEG graph → Json mapping examples

### No meta flat vertexes list (one element)

**MVEG:**

```ZeroCode
"VALUE"
	"one"
```

**Json:**

```
[
  "one"
]
```
comments:

- the value of the staring _vertex_ is ignored
- mind that the `one` value is returned inside of `[ ]` array as we can not have `{"one"}` Json

### No meta flat vertexes list (multiple elements)

**MVEG:**

```ZeroCode
"VALUE"
	"one"
	"two"
	"thre"
```

**Json:**

```
[
  "one",
  "two",
  "thre"
]
```

comments:

- the value of the staring _vertex_ is ignored
- mind that the `one` value is returned inside of `[ ]` array as we can not have `{"one", "two", "three"}` Json

### Adding meta, one vertex

**MVEG:**

```MinusZero
""
	<@System\Meta :: "one">	
```

**Json:**

```
{
  "Meta": "one"
}
```

### Multiple vertexes with meta

**MVEG:**

```ZeroCode
""
	<@System\Meta :: "one">
	<@System\Meta :: "two">
	<@System\Meta :: "three">
```

**Json:**

```
{
  "Meta": [
    "one",
    "two",
    "three"
  ]
}
```

comments:

- if there are more than one _edge_ with the same _meta vertex_, those become a Json array started with a key made of the _meta vertex_ and the values made of the _to vertexes_

### Two levels, without meta

**MVEG:**

```ZeroCode
""
	"one"
		"two"
```

**Json:**

```
[
  [
    "two"
  ]
]
```

comments:

- `one` is ignored as can not have `:"one"["two"]` Json

### Two levels, first with meta (one element)

**MVEG:**

```ZeroCode
""
	<@System\Meta :: "one">
		"two"
```

**Json:**

```
{
  "Meta": [
    "two"
  ]
}
```

comments:

- `one` is ignored as can not have `"Meta":"one"["two"]` json

### Two levels, first with meta (multiple elements)

**MVEG:**

```ZeroCode
""
	<@System\Meta :: "one A">
		"two A"
	<@System\Meta :: "one A">
		"two B"
	<@System\Meta :: "one B">
		"two C"
```

**Json:**

```
{
 "Meta": [
      "two A",    
      "two B",
      "two C"
  ]
}
```

comments:

- `one X` are ignored as can not have `"Meta":"one X"["two X"]` Json

### Two levels, meta and no-meta mix

**MVEG:**

```ZeroCode
""
	"one"
		"two"
	<@System\Meta :: "three">
		"two"
```

**Json:**

```
{
  "": [
    "two"
  ],
  "Meta": [
    "two"
  ]
}
```

comments:

- `three` is ignored as can not have `"Meta":"three"["two"]` Json
- there is `"":` (empty key) before first `"two"`, as the `{}` Json must contain key / value

### Two levels, meta (multiple elements) and no-meta mix

**MVEG:**

```ZeroCode
""
	"one"
		"two"
	<@System\Meta :: "three">
		"four"
	<@System\Meta :: "five">
		"six"
	<@System\Meta :: "seven">
```

**Json:**

```
{
  "": [
    "two"
  ],
  "Meta": [
    "four",
    "six",
    "seven"
  ]
}
```

comments:

- `three` and `five` are ignored as can not have `"Meta":"three"["two"]` Json
- `seven` is not ignored, as it is emited as last array element

### Combined example

**MVEG:**

```ZeroCode
""
	"one A"
		"two A"
	"one B"
		"two B"
			"three B"
	<@System\Meta :: "one C">
		"two C"
		"three C"
	<@System\Meta :: "one D">
		<@System\Meta :: "two D">
			"three D"
		<@System\Meta :: "two D">
			"three D"
			"three D"
			<@System\Meta :: "three D">
	<@System\Meta :: "one E">
		"two E"
			<@System\Meta :: "three E">
			<@System\Meta :: "three E">
```

**Json:**

```
{
  "": [
    "two A",
    "three B"
  ],
  "Meta": [
    "two C",
    "three C",
    {
      "Meta": [
        "three D",
        {
          "": [
            "three D",
            "three D"
          ],
          "Meta": "three D"
        }
      ]
    },
    {
      "Meta": [
        "three E",
        "three E"
      ]
    }
  ]
}
```

comments:
- above example showes most of the above rules in one example