# Views

## Graph
```ZeroCode
<@View :: "">
	<@$Is :: @View>
	<@FromTriggerQuery :: "query body">
	<@FromTriggerFilter :: @GraphChangeEnum\ValueChange>
	<@FromToTransformFunction :: @f>
	<@ToTriggerQuery :: "query body">
	<@ToTriggerFilter :: @GraphChangeEnum\ValueChange>
	<@ToFromTransformFunction :: @f>
<@$GraphChangeTrigger :: "CreateView">
	<@$Is :: @GraphChangeTrigger>
	<@ChangeTypeFilter :: @OnlyNonTransactedRootVertexEvents>
	<@ChangeTypeFilter :: @MetaEdgeAdded>
	<@Listener :: @CreateViewMetaEdgeAdded>
```
## About

_View_ is defined as _meta edge_. When this _meta edge_ is added to given vertex (the given vertex will be called from now the _source_ vertex), the _view_ is created in the _target_ vertex.

## View abstract definition

- _**view**_
	- type:
		- fire and forget (_no triggers_)		
			- The view is generated when edge is added.
		- updatable (_has to have triggers_)
	- direction:
		- _source_ to _target_
		- _target_ to _source_
		- both
- _**source**_
	- trigger query _(0..*)_
	- trigger filters _(0..*)_
	- transform function _(0..1)_
- _**target**_
	- trigger query _(0..*)_
	- trigger filters _(0..*)_
	- transform function _(0..1)_

__After the "Possible view definitions" table (below) evaluation we come to conclusion, that just trigger queries and transform function are enough to express above.__
## Possible view definitions
|comment                       |type           |direction       |source trigger|source transform function|target trigger|target transform function|
|:-----------------------------|:--------------|:---------------|:-------------|:------------------------|:-------------|:------------------------|
|one time from source to target|fire and forget|source to target|NO            |YES                      |NO            |NO                       |
|updatable source to target    |updatable      |source to target|YES           |YES                      |NO            |NO                       |
|updatable target to source    |updatable      |target to source|NO            |NO                       |YES           |YES                      |
|updatable both                |updatable      |both            |YES           |YES                      |YES           |YES                      |
## Create view Graph
```ZeroCode
<(?<ANY>) :: "">
	<@$Is :: CreateView>
	<@Name :: "view name">
	<@CreateViewInner :: "">
		<@Is :: @CreateViewInner>
		<@Expression :: "">
			<@Is :: @FromTriggerQuery>
			<@Query :: "query body">
		<@Expression :: "">
			<@Is :: @FromTriggerFilter>
			<@Value :: @ChangeTypeFilterEnum\Value>
		<@Expression :: "">
			<@Is :: @FromToTransformFunction>
			<@Target :: @ListenerFunction>
		<@Expression :: "">
			<@Is :: @ToTriggerQuery>
			<@Query :: "query body">
		<@Expression :: "">
			<@Is :: @ToTriggerFilter>
			<@Value :: @ChangeTypeFilterEnum\Value>
		<@Expression :: "">
			<@Is :: @ToFromTransformFunction>
			<@Target :: @ListenerFunction>
```
## Transform function parameters

- event @Event
	- if `event == ~00` than transform function is expected to generate whole source/target (so this is not a _transform_ function but a _generate_)
- from @VertexType
- meta @VertexType
- to @VertexType
## How to define source / target trigger

For the _source_ / _target_ trigger to be defined, it is enough to define trigger filter respectively for the _source_ or _target_.
So it means that there:
- should be at last one `FromTriggerFilter` present to define _source_ trigger.
- should be at last one `ToTriggerFilter` present to define _target_ trigger.
> Please note that lack of existence of `FromTriggerQuery` or `ToTriggerQuery` does not make _source_ / _target_ trigger to be not defined.
>
> In case trigger queries are not defined, it means that only _source_ or _target_ vertex are the view triggering events source.
## create view syntax
```ZeroCode
create view {
	from query "query body"
	from filter @@GraphChangeFilterEnum\ValueChange
	from to transform @@TransformFunction
	to query "query body"
	to filter @@GraphChangeFilterEnum\ValueChange
	to from transform @@TransformFunction
}
```
above expression will create view at local stack, so that means it can be used in following way
```ZeroCode
AddHere +< view "view name"{
	...
}
```
## example
```ZeroCode
"EXAMPLE"
	import @System\Lib\Std direct meta
	variable "ExampleView" @VertexType
	variable "Source" @VertexType
	"set"
		"value"
	"summary"
		"min"
		"max"
		"count"
	function "ExampleView_FromToTransform" (@Event "viewEvent", @VertexType "from", @VertexType "meta", @VertexType "to")
		to +< @@summary ::{
			@@summary\count :: from\set\value<>
			@@summary\min :: @@Min[from\set\value]
			@@summary\max :: @@Max[from\set\value]
		}
	ExampleView = "Example View"
	ExampleView +< create view{
		from to transform @@ExampleView_FromToTransform
	}
	Source = "Source"
	Source +< @@set :: "first set"{
		@@set\value :: "1"
		@@set\value :: "2"
		@@set\value :: "3"
	}
	Source +< @@set :: "second set"{
		@@set\value :: "10"
		@@set\value :: "20"
		@@set\value :: "30"
		@@set\value :: "40"
	}
	Source +< ExampleView :: "Target"
```
Above code results in the following:
```ZeroCode
"Source"
	<@set :: "first set">
		<@value :: "1">
		<@value :: "2">
		<@value :: "3">
	<@set :: "second set">
		<@value :: "10">
		<@value :: "20">
		<@value :: "30">
		<@value :: "40">
	<@'Example View' :: "Target">
		<@summary :: "$Empty">
			<@count :: "7">
			<@min :: "1">
			<@max :: "40">
```