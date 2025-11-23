```InlineHTML
<a href="gazeta.pl">gazeta</a>
```
```ZeroCode
"code"
	"Code0"
		variable "A" @String
	"Code1"
		variable "A" @String
		variable "B" @String
		variable "C" @String
		variable "D" @String
		A +< "A1"
		A +< "A2"
		A +< "A3"
		B +< "B1"
		B +< "B2"
		B +< "B3"
		A <- "val A"
		B <- "val B"
		C +< A\ <+> B\
		D +< C\ <-> B\
	"Code2"
		variable "Test" @String
		variable "Test2" @String
		variable "Test3" @String
		variable "Magunia" @String "5":"10"
		variable "Radek" @String "2":"2"
		Radek += "Koha"
		Magunia +< Radek
		Test +< Magunia <+> Radek <+> Radek
		Test2 = Test\ <+> "TST"
		Test3 = "TEST#3"
		Test3 += Test\ <-> Radek
	"Code3"
		variable "A" @String
		variable "B" @String
		variable "C" @String
		function "X" (@String "imie")
			variable "B" @String
			B = imie
			A = B
			B = "main b"
		@@Code3\X["Radek"]
	"Code4"
		variable "A" @String
		variable "B" @String
		variable "C" @String
		variable "D" @String
		A = "A"
		A += "A"
		A += "B"
		A += "True"
		A += "True"
		A += "1"
		B = "A"
		B += "B"
		B += "A"
		B += "1"
		B += "0"
		B += "1"
		C = A < B
		D = !C
	"Code5"
		variable "A" @String
		variable "B" @String
		variable "C" @String
		variable "D" @String
		A = "1"
		A += "2"
		A += "3"
		B = ("1" + "2") * "3"
		C = A < "2"
		D = !C
	"Code6"
		variable "A" @String
		variable "B" @String
		variable "C" @String
		variable "D" @String
		function "Add" @Integer(@Integer "x")
			return x + "1"
		A = @@Code6\Add["3"]
	"Code7"
		variable "A" @String
		variable "B" @String
		variable "C" @String
		variable "D" @String
		variable "E" @String
		A = "1"
		A += "2"
		A += "3"
		A += "4"
		B = "1"
		B += "2"
		B += "3"
		B += "4"
		for vertex "X" in A
			for vertex "Y" in B
				C += (X * "4") + Y
		for vertex "X" in A
			for vertex "Y" in B
				for vertex "Z" in C
					D += (Z * "20") + (X * "4") + Y
		for vertex "X" in A
			for vertex "Y" in B
				for vertex "W" in D
					E += (W * "200") + (Z * "20") + (X * "4") + Y
	"Code7b"
		variable "A" @String
		variable "B" @String
		A = "a"
		A += "1"
		A += "2"
		B = "b"
		for vertex "X" in A
			B += X
	"Code8"
		variable "A" @String
		variable "B" @String
		A = "0"
		while A <= "100"
			B += A
			A = A + "1"
	"Code9"
		variable "a" @Vertex
		a = @@System
	"Code10"
		"X"
			"2" * ("2" + "1")
		variable "A" @Integer
		variable "B" @String
		variable "C" @Vertex
		variable "D" @Integer
		A = execute($\:X\)
		B = generate @@System\FormalTextLanguage\FormalTextLanguageProcessing:ZeroCode_VertexAndManyLines\ZeroCode ($\:X\)
		C = parse @@System\FormalTextLanguage\FormalTextLanguageProcessing:ZeroCode_VertexAndManyLines\ZeroCode ("a+b")
		D = execute(parse("A + A"))
	"Code11"
		"x"
			"a"
			"z1"
		"x"
			"b"
				"z2"
			"b"
				"z3"
		variable "a" @String
		a = ($\:x<<"1">>\ <+> $\:x<<"2">>\)
		@@Code11\x<<"1">>\b<<"1">>\z3
	"Code12"
		class "Person"
			attribute "Name" @String "1":"1"
			method "setName" (@String "name")
				Name = name
			method "getName" @String()
				return Name
		variable "person" @Code12\Person
		variable "name" @String
		person = new @@Code12\Person[]
		person.setName["Rad9k"]
		name = person.getName[]
	"Code13"
		variable "mis" @Boolean
		if ("1" + "2") == "3"
			mis = "True"
		function "t" @String(@String "m")
			test "4"
				case "1"
					return "raz"
				case "2"
					return "dwa"
				case "3"
					return "trzy"
				fallback
					return "def"
				case "4"
					if m == "True"
						return "cztery"
			return "kupa"
		variable "a" @String
		a = @@Code13\t[mis]
	"Code14"
		variable "a" @Boolean
		variable "b" @Boolean
		if ("1" + "2") == "3"
			a = "True"
		if "1" + "2" == "3"
			block
				a = "False"
				b = "True"
	"aacode test"
		namedblock "Ablok"
			variable "a" @Boolean
			a = "True"
			b = "True"
		function "Afunc" ()
			variable "a" @Boolean
			a = "True"
			b = "True"
		function "Bfunc" ()
			b = "True"
		function "Cfunc" ()
			return Name
		function "Xfunc" ()
			while a == b
				b = a
				a = b
			while a == b
				a = b
			while a == b
				return Name
		namedblock "Bblok"
			b = "True"
		namedblock "Cblok"
			return Name
		namedblock "Dblok"
			for vertex "X" in x
				for vertex "Y" in y
					X += Y
		block
			a = "False"
			b = "True"
		block
			a = "False"
		block
			return Name
		if a == b
			a = "True"
			b = "True"
		if a == b
			b = "True"
		if a == b
			return Name
		while a == b
			a = "True"
			b = "True"
		while a == b
			b = "True"
		while a == b
			return Name
		namedblock "TESTBLOK"
			a = "True"
	"codeforech"
		for vertex xxx in yyy\kuery
			y += xxx
			b += xxx
	"parsetest"
		while "3" == "4"
			function "Cin" ()
			@@'parsetest'\<<"1">>\Cin[]
		function "B" ()
			function "Bin" ()
			@@'parsetest'\B\Bin[]
		function "A" ()
			for vertex "X" in x
				for vertex "Y" in y
					X += Y
		while "1" == "2"
			for vertex "X" in x
				for vertex "Y" in y
					X += Y
	package "TestPackage"
```