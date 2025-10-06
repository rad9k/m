# View

## View on not initialized variable

Code:

```-0
"EXAMPLE"
	variable "ExampleView" @VertexType
	variable "Source" @VertexType
	ExampleView = "Example View"
	ExampleView +< create view{
	}	
	Source +< "TEST"
	Source +< ExampleView :: "Target"
```

Result:
```-0
$Empty
```

Comment:
- View not working and `TEST` not visible. This is not expected.


## View on initialized variable

Code:

```-0
"EXAMPLE"
	variable "ExampleView" @VertexType
	variable "Source" @VertexType
	ExampleView = "Example View"
	ExampleView +< create view{
	}
	Source = "Source"
	Source +< "TEST"
	Source +< ExampleView :: "Target"
```

Result:
```-0
"Source"
	"TEST"
	<@'Example View' :: "Target">
```

Comment:
- Seems OK.

## View on copied variable

Code:

```-0
"EXAMPLE"
	variable "ExampleView" @VertexType
	variable "Source" @VertexType
	ExampleView = "Example View"
	ExampleView +< create view{
	}
	Source = %"Source"
	Source +< "TEST"
	Source +< ExampleView :: "Target"
```

Result:
```-0
"$Empty"
```

Comment:
- This is strange. Also in Execute dialog results showes @Source :: "Source"

## View on copied variable, but added to another variable

Code:

```-0
"EXAMPLE"
	variable "ExampleView" @VertexType
	variable "Source" @VertexType
	variable "x" @VertexType
	ExampleView = "Example View"
	ExampleView +< create view{
	}
	Source = %"Source"
	x +< Source
	Source +< "TEST"
	Source +< ExampleView :: "Target"
```

Result:
```-0
"Source"
	"TEST"
	<@'Example View' :: "Target">
```

Comment:
- Now it works. WTF?

## View on attribute

Code:

```-0
"EXAMPLE"
	variable "o" @tst
	variable "ExampleView" @VertexType
	class "tst"
		attribute "Source" @VertexType
		method "doTest" ()
			Source +< ExampleView :: "Target"
	ExampleView = "Example View"
	ExampleView +< create view{
	}
	o = new @@tst[]
	o.doTest[]
```

Result:
```-0
""
	<@$Is :: @tst>
	<@Source :: "">
		<@'Example View' :: "Target">
```

Comment:
- Seems OK.





