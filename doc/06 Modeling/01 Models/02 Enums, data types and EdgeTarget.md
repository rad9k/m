# Enums, data types and EdgeTarget

## $EdgeTarget

## Data types

## Enums

Having set of possible values for a attribute is common patern in data modeling. In data modelling world such set of values is called a _dictionary_. As -zero is execution platform, we are calling them _enums_.

```ZeroCode
<@Enum :: "ExceptionTypeEnum">
	<@EnumValue :: "Error">
	<@EnumValue :: "Warning">
	<@EnumValue :: "Info">
	<@$Inherits :: @EnumBase>
```

In above example:
- `ExeceptionTypeEnum` _enum_ is defined with the use of:
	- `Enum` as a meta vertex
	- vertex contining _enum_ definition inherits from `EnumBase`
- _enum_ values defined with the use of `EnumValue` meta edges:
	- `Error`
	- `Warning`
	- `Info`