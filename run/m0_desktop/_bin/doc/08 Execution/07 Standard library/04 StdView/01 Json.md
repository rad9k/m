# Json

## The MVEG → Json impedance mismatch

Most of the MVEG → Json impedance mismatch comes from the fact that JSON is a tree structure (only one "parent" for a node) and MVEG is a graph (multiple "parents" for _vertex_, cycles allowed). Althought, there is another important impedance mismatch factor that is more subtle - Json operates on a key / value concept, where the value is atomic and can not have a "internal" list of key / values. This means that althought we can map a MVEG like this:

```
<@META :: "VALUE">
```

to the following Json

```
"META":"VALUE"
```

we are not able to map following MVEG:

```
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

### No meta flat vertexes list

**MVEG:**

```MinusZero
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

### No meta flat vertexes list

**MVEG:**

```MinusZero
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

```MinusZero
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

- if there are more than one _edge_ with the same _meta vertex_, those become a json array started with a key made of the _meta vertex_ and the values made of the _to vertexes_

### simplest mapping

**MVEG:**

```MinusZero
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

- `one` is ignored as can not have `:"one"["two"]` json

### simplest mapping

**MVEG:**

```MinusZero
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

### simplest mapping

**MVEG:**

```MinusZero
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

- `one X` are ignored as can not have `"Meta":"one X"["two X"]` json

### simplest mapping

**MVEG:**

```MinusZero
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

- `three` is ignored as can not have `"Meta":"three"["two"]` json

### simplest mapping

**MVEG:**

```MinusZero
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

- `three` and `five` are ignored as can not have `"Meta":"three"["two"]` json
- `seven` is not ignored, as it is emited as last array element

### simplest mapping

**MVEG:**

```MinusZero
```

**Json:**

```
```

### simplest mapping

**MVEG:**

```MinusZero
```

**Json:**

```
```


_______________________
""
	<@System\Meta :: "one A">
		"two A"
	<@System\Meta :: "one A">
		"two B"
	<@System\Meta :: "one B">
		"two C"
zwraca
{
  "Meta": [
    [
      "two A"
    ],
    [
      "two B"
    ],
    [
      "two C"
    ]
  ]
}

a powinien zwracac
{
 "Meta": [
      "two A",    
      "two B",
      "two C"
  ]
}