# MVEG representation

Let's have a C# code that will generate the following -zero graph:

```ZeroCode
"Class"
	"Attribute"
		<@$VertexTarget :: @Type>
<@Class :: "Person">
	<@Attribute :: "Name">
		<@$EdgeTarget :: @String>
	<@Attribute :: "Surname">
		<@$EdgeTarget :: @String>
	<@Attribute :: "HairLength">
		<@$EdgeTarget :: @Integer>
<@Person :: "">
	<@Name :: "Radek">
	<@Surname :: "Tereszczuk">
	<@HairLength :: "10">
<@Person :: "">
	<@Name :: "Maurycy">
	<@Surname :: "Tereszczuk">
	<@HairLength :: "7">
```

The C# code will create the graph gragment starting from the `Work` _vertex_ being present in the -zero root:

```
IVertex WorkVertex = root.Get(false, "Work");

IVertex ClassVertex = WorkVertex.AddVertex(null, "Class");
IVertex AttributeVertex = ClassVertex.AddVertex(null, "Attribute");

AttributeVertex.AddEdge(root.Get(false, @"System\Meta\Base\Vertex\$VertexTarget"), 
	root.Get(false, @"System\Meta\ZeroUML\Type"));

IVertex PersonVertex = WorkVertex.AddVertex(ClassVertex, "Person");

IVertex NameAttribute = PersonVertex.AddVertex(AttributeVertex, "Name");
NameAttribute.AddEdge(root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), 
	root.Get(false, @"System\Meta\ZeroTypes\String"));

IVertex SurnameAttribute = PersonVertex.AddVertex(AttributeVertex, "Surname");
SurnameAttribute.AddEdge(root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), 
	root.Get(false, @"System\Meta\ZeroTypes\String"));

IVertex HairLengthAttribute = PersonVertex.AddVertex(AttributeVertex, "HairLength");
HairLengthAttribute.AddEdge(root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), 
	root.Get(false, @"System\Meta\ZeroTypes\Integer"));

IVertex RadekInstanceVertex = WorkVertex.AddVertex(PersonVertex, "");
RadekInstanceVertex.AddVertex(NameAttribute, "Radek");
RadekInstanceVertex.AddVertex(SurnameAttribute, "Tereszczuk");
RadekInstanceVertex.AddVertex(HairLengthAttribute, 10);

IVertex MaurycyInstanceVertex = WorkVertex.AddVertex(PersonVertex, "");
MaurycyInstanceVertex.AddVertex(NameAttribute, "Maurycy");
MaurycyInstanceVertex.AddVertex(SurnameAttribute, "Tereszczuk");
MaurycyInstanceVertex.AddVertex(HairLengthAttribute, 7);
```

comments:

- `AddVeretx` creates new _vertex_. If _vertex_ does not have any _incoming edges_ it will be disposed, so when creating new _vertex_, we need to create some _incoming edge_ for the new _vertex_. The new _edge_ is created from the _vertex_ represented by the `IVertex` on what the `AddVertex` is executed. The reference to the _meta vertex_ to be used in the new _edge_ is the first call parameter of the `AddVertex`. The second parameter is the to-be value of the newly created _vertex_.

- The `AddVertex` returns newly created `IVertex`.

- There is also a `AddVertexAndReturnEdge` version of the `AddVertex`. It returns newly created edge as it is sometimes needed instead of `IVertex` returned by `AddVertex`. The newly created `IVertex` is accesible from the `IEdge` returned by `AddVertexAndReturnEdge` by the `To` attribute of the returned `IEdge`.

- `AddEdge` creates new _edge_. So we do not create any new _vertex_ here and only connect two existing _vertexes_ with the newly created _edge_. The newly created _edge_ is created from the `IVertex` on what the `AddEdge` is executed. The to _vertex_ for the new _edge_ is defined by the second call parameter of the `AddEdge`. The reference to the _meta vertex_ to be used in the new _edge_ is the first call parameter of the `AddEdge`.

- The `AddEdge` returns `IEdge` that represents newly created edge.

## IVertex

The `IVertex` structure represents _vertex_. The most important attributes and methods of the `IVertex` are shown below:

```
 interface IVertex
 {     
     object Identifier { get; }

     object Value {get; set;}

     IList<IEdge> OutEdgesRaw { get;} // without $Inherits

     IList<IEdge> OutEdges { get; }        

     IList<IEdge> InEdgesRaw { get; } // without $Inherits

     IList<IEdge> InEdges { get; }        

     IList<IEdge> MetaInEdgesRaw { get; } // without $Inherits

     INoInEdgeInOutVertexVertex Execute(IExecution exe);

     void QueryOutEdges(object meta, object to, out IEdge result, out IList<IEdge> results);

     void QueryInEdges(object meta, object from, out IEdge result, out IList<IEdge> results);
     
     IVertex AddVertex(IVertex metaVertex, object val);

     IEdge AddVertexAndReturnEdge(IVertex metaVertex, object val);

     IEdge AddEdge(IVertex metaVertex, IVertex destVertex);

     void DeleteEdge(IEdge edge);

     void DeleteEdgesList(IEnumerable<IEdge> edges);

     IVertex Get(bool metaMode, string query);

     IVertex GetAll(bool metaMode, string query);
                     
     IStore Store { get; }     
 }
```

comments:

- `Identifier` is given _vertex_ internal identifier. It is not visible inside of the -zero platform anywhere except Debug type of Visualiser and most imortant: it is not aviable anywhere in the code executed by the GVM. The identifier is used when saving and loading `IStore` graph to the persistant storage (like a file in a filesystem). 

- `Value` provides access to the `IVertex`'s value.

- `OutEdgesRaw` provides access to all of the _outgoing edges_. The `OutEdgesRaw` provides _outgoing edges_ that are _physically_ attached to the _vertex_ - it means that the additional _outgoing edegs_ that are added by the presence of the `$Inherits` _meta edge_ are not included here.

- `OutEdges` provides access to all of the _outgoing edges_. The `OutEdges` provides _outgoing edges_ that are _logically_ attached to the _vertex_ - it means that the additional _outgoing edges_ that are added by the presence of the `$Inherits` _meta edge_ are included here.

- `InEdgesRaw` provides access to all of the _incoming edges_. The `InEdgesRaw` provides _incoming edges_ that are _physically_ attached to the _vertex_ - it means that the additional _outgoing edges_ that are added by the presence of the `$Inherits` _meta edge_ are not included here.

- `InEdges` provides access to all of the _incoming edges_. The `InEdges` provides _incoming edges_ that are _logically_ attached to the _vertex_ - it means that the additional _outgoing edges_ that are added by the presence of the `$Inherits` _meta edge_ are included here.

- `MetaInEdgesRaw` provides access to all of the _meta incoming edges_ - this is a situation when given _vertex_ is used as _meta vertex_ in some _edge_. The `MetaInEdgesRaw` provides _meta incoming edges_ that are _physically_ attached to the _vertex_.

- `Execute` executes GVM code starting from given `IVertex`.

- `QueryOutEdges` uses internal hash-based dictionaries (indexes) for look-up of _outgoing edges_ meeting criteria: having _meta vertex_ with a given value - `meta` parameter (or `null` → meaning no filter for _meta vertex_ value) and having _to vertex_ with a given value - `to` parameter (or `null` → meaning no filter for _to vertex_ value)

- `QueryInEdges` uses internal hash-based dictionaries (indexes) for look-up of _incoming edges_ meeting criteria: having _meta vertex_ with a given value - `meta` parameter (or `null` → meaning no filter for _meta vertex_ value) and having _from vertex_ with a given value - `from` parameter (or `null` → meaning no filter for _from vertex_ value)
     
- `AddVertex` adds new _vertex_ and returns newly created _vertex_.

- `AddVertexAndReturnEdge` adds new _vertex_ and returns newly created _edge_.

- `AddEdge` adds new _edge_ and returns it.

- `DeleteEdge` deletes _edge_.

- `DeleteEdgesList` deleteas _edges_ list.

- `Get` executes a query starting from given `IVertex` using provided `metaMode`. Returns only one `IVertex`.

- `GetAll` executes a query starting from given `IVertex` using provided `metaMode`. Returns a list of `IEdge`.
                     
- `Store` provides access to the `IStore` for given `IVertex`.

## IEdge

The `IEdge` structure represents _edge_. The most important attributes and methods of the `IEdge` are shown below:

```
interface IEdge
{
    IVertex From { get; }
	
    IVertex Meta { get; }
	
    IVertex To { get; }
}
```

comments:

- `From` exposes the _from vertex_

- `Meta` exposes the _meta vertex_

- `To` exposes the _to vertex_