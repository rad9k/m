# View

## View on not initialized variable

Code:
```ZeroCode
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
```ZeroCode
$Empty
```
Comment:
- View not working and `TEST` not visible. This is not expected.
## View on initialized variable

Code:
```ZeroCode
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
```ZeroCode
"Source"
	"TEST"
	<@'Example View' :: "Target">
```
Comment:
- Seems OK.
## View on copied variable

Code:
```ZeroCode
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
```ZeroCode
"$Empty"
```
Comment:
- This is strange. Also in Execute dialog results shows @Source :: "Source"
## View on copied variable, but added to another variable

Code:
```ZeroCode
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
```ZeroCode
"Source"
	"TEST"
	<@'Example View' :: "Target">
```
Comment:
- Now it works. WTF?
## View on attribute

Code:
```ZeroCode
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
```ZeroCode
""
	<@$Is :: @tst>
	<@Source :: "">
		<@'Example View' :: "Target">
```
Comment:
- Seems OK.





