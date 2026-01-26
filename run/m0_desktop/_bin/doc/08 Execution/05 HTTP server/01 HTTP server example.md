```ZeroCode
"Y"
	import @System\Lib\Net\HttpActionEnum direct meta
	
	variable "a" @VertexType
	
	function "One"()
	function "Two"()
	
	a +< create http mapping "mapping name"{
		mapping @@GET "FunctionOne" @@One
		mapping @@GET "FunctionTwo" @@Two
	}
```

```
dotnet dev-certs https --trust
```