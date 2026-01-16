using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace m0.Lib.StdView
{
    public class JsonToVertex
    {
        public static INoInEdgeInOutVertexVertex JsonToVertex_Transform(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex from = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex to = GraphUtil.GetQueryOutFirst(stack, "to", null);

            string JsonString = GraphUtil.GetStringValueOrNull(from);

            JsonToVertex_Process(JsonString, to);

            return null;
        }

        public static void JsonToVertex_Process(string json, IVertex to)
        {
            if (to == null || string.IsNullOrWhiteSpace(json))
                return;

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                return;
            }

            IVertex schemaRoot = to.AddVertex(null, "Schema");
            IVertex dataRoot = to.AddVertex(null, "Data");

            var context = new SchemaContext(schemaRoot);

            SchemaClass rootClass = context.BuildRootSchema(document.RootElement);
            CreateData(document.RootElement, dataRoot, rootClass, context);
        }

        private enum SchemaValueKind
        {
            Primitive,
            Object
        }

        private class SchemaProperty
        {
            public string Name { get; set; }
            public bool IsArray { get; set; }
            public SchemaValueKind ValueKind { get; set; }
            public IVertex PropertyVertex { get; set; }
            public SchemaClass TargetClass { get; set; }
            public IVertex TargetTypeVertex { get; set; }
        }

        private class SchemaClass
        {
            public string Name { get; set; }
            public IVertex ClassVertex { get; set; }
            public IDictionary<string, SchemaProperty> Properties { get; } = new Dictionary<string, SchemaProperty>();
        }

        private class SchemaContext
        {
            private readonly IVertex schemaRoot;
            private readonly IVertex zeroTypesRoot;
            private readonly IVertex isJsonArrayMeta;
            private readonly IDictionary<string, SchemaClass> classesByPath = new Dictionary<string, SchemaClass>();
            private readonly IDictionary<string, int> classNameCounts = new Dictionary<string, int>();

            public SchemaContext(IVertex schemaRoot)
            {
                this.schemaRoot = schemaRoot;
                zeroTypesRoot = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes");
                isJsonArrayMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$IsJsonArray");
            }

            public SchemaClass BuildRootSchema(JsonElement rootElement)
            {
                if (rootElement.ValueKind == JsonValueKind.Object)
                    return BuildObjectClass(rootElement, "Root", "Root");

                SchemaClass rootClass = CreateClass("Root", "Root");
                if (rootElement.ValueKind == JsonValueKind.Array)
                    AddArrayProperty(rootClass, "items", rootElement, "Root.items");
                else
                    AddPrimitiveProperty(rootClass, "value", rootElement, "Root.value", isArray: false);

                return rootClass;
            }

            private SchemaClass BuildObjectClass(JsonElement obj, string classNameHint, string path)
            {
                if (classesByPath.TryGetValue(path, out SchemaClass existing))
                {
                    MergeObjectProperties(existing, obj, path);
                    return existing;
                }

                SchemaClass schemaClass = CreateClass(classNameHint, path);
                MergeObjectProperties(schemaClass, obj, path);
                return schemaClass;
            }

            private SchemaClass CreateClass(string classNameHint, string path)
            {
                string className = CreateUniqueClassName(classNameHint);
                IVertex classVertex = GraphUtil.AddClass(schemaRoot, className);
                var schemaClass = new SchemaClass
                {
                    Name = className,
                    ClassVertex = classVertex
                };
                classesByPath[path] = schemaClass;
                return schemaClass;
            }

            private void MergeObjectProperties(SchemaClass schemaClass, JsonElement obj, string path)
            {
                foreach (JsonProperty property in obj.EnumerateObject())
                {
                    if (!schemaClass.Properties.TryGetValue(property.Name, out SchemaProperty schemaProperty))
                        schemaProperty = AddProperty(schemaClass, property.Name, property.Value, path + "." + property.Name);
                    else if (schemaProperty.ValueKind == SchemaValueKind.Object && property.Value.ValueKind == JsonValueKind.Object)
                        MergeObjectProperties(schemaProperty.TargetClass, property.Value, path + "." + property.Name);
                    else if (schemaProperty.ValueKind == SchemaValueKind.Object && property.Value.ValueKind == JsonValueKind.Array)
                        MergeArrayObjectItems(schemaProperty.TargetClass, property.Value, path + "." + property.Name);
                }
            }

            private SchemaProperty AddProperty(SchemaClass schemaClass, string propertyName, JsonElement value, string path)
            {
                if (value.ValueKind == JsonValueKind.Object)
                {
                    SchemaClass targetClass = BuildObjectClass(value, propertyName, path);
                    return AddAssociationProperty(schemaClass, propertyName, targetClass, isArray: false);
                }

                if (value.ValueKind == JsonValueKind.Array)
                    return AddArrayProperty(schemaClass, propertyName, value, path);

                return AddPrimitiveProperty(schemaClass, propertyName, value, path, isArray: false);
            }

            private SchemaProperty AddArrayProperty(SchemaClass schemaClass, string propertyName, JsonElement arrayElement, string path)
            {
                JsonElement firstElement;
                bool hasElement = arrayElement.GetArrayLength() > 0;

                if (hasElement)
                    firstElement = arrayElement.EnumerateArray().First();
                else
                    firstElement = default;

                if (hasElement && firstElement.ValueKind == JsonValueKind.Object)
                {
                    SchemaClass targetClass = BuildObjectClass(firstElement, propertyName + "Item", path + ".Item");
                    MergeArrayObjectItems(targetClass, arrayElement, path + ".Item");
                    return AddAssociationProperty(schemaClass, propertyName, targetClass, isArray: true);
                }

                if (hasElement && firstElement.ValueKind == JsonValueKind.Array)
                {
                    SchemaClass nestedArrayClass = BuildNestedArrayClass(propertyName, arrayElement, path + ".Item");
                    return AddAssociationProperty(schemaClass, propertyName, nestedArrayClass, isArray: true);
                }

                return AddPrimitiveProperty(schemaClass, propertyName, arrayElement, path, isArray: true);
            }

            private void MergeArrayObjectItems(SchemaClass targetClass, JsonElement arrayElement, string path)
            {
                foreach (JsonElement item in arrayElement.EnumerateArray())
                    if (item.ValueKind == JsonValueKind.Object)
                        MergeObjectProperties(targetClass, item, path);
            }

            private SchemaClass BuildNestedArrayClass(string propertyName, JsonElement arrayElement, string path)
            {
                SchemaClass arrayClass = CreateClass(propertyName + "Item", path);
                AddArrayProperty(arrayClass, "items", arrayElement, path + ".items");
                return arrayClass;
            }

            private SchemaProperty AddPrimitiveProperty(SchemaClass schemaClass, string propertyName, JsonElement value, string path, bool isArray)
            {
                int minCardinality = isArray ? (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() == 0 ? 0 : 1) : 1;
                int maxCardinality = isArray ? -1 : 1;

                IVertex targetType = GetTypeVertex(value);
                IVertex attributeVertex = GraphUtil.AddAttribute(schemaClass.ClassVertex, propertyName, targetType, minCardinality, maxCardinality);
                if (isArray)
                    MarkAsJsonArray(attributeVertex);

                var schemaProperty = new SchemaProperty
                {
                    Name = propertyName,
                    IsArray = isArray,
                    ValueKind = SchemaValueKind.Primitive,
                    PropertyVertex = attributeVertex,
                    TargetTypeVertex = targetType
                };
                schemaClass.Properties[propertyName] = schemaProperty;
                return schemaProperty;
            }

            private SchemaProperty AddAssociationProperty(SchemaClass schemaClass, string propertyName, SchemaClass targetClass, bool isArray)
            {
                int minCardinality = isArray ? 0 : 1;
                int maxCardinality = isArray ? -1 : 1;
                IVertex associationVertex = CreateAssociation(schemaClass.ClassVertex, propertyName, targetClass.ClassVertex, minCardinality, maxCardinality);
                if (isArray)
                    MarkAsJsonArray(associationVertex);

                var schemaProperty = new SchemaProperty
                {
                    Name = propertyName,
                    IsArray = isArray,
                    ValueKind = SchemaValueKind.Object,
                    PropertyVertex = associationVertex,
                    TargetClass = targetClass
                };
                schemaClass.Properties[propertyName] = schemaProperty;
                return schemaProperty;
            }

            private void MarkAsJsonArray(IVertex propertyVertex)
            {
                if (propertyVertex == null || isJsonArrayMeta == null)
                    return;

                propertyVertex.AddEdge(isJsonArrayMeta, MinusZero.Instance.Empty);
            }

            private IVertex CreateAssociation(IVertex classVertex, string associationName, IVertex targetClass, int minCardinality, int maxCardinality)
            {
                IVertex root = MinusZero.Instance.Root;

                IVertex associationMeta = root.Get(false, @"System\Meta\ZeroUML\Class\Association");
                IVertex associationVertex = classVertex.AddVertex(associationMeta, associationName);
                associationVertex.AddEdge(MinusZero.Instance.Is, root.Get(false, @"System\Meta\ZeroUML\Class\Association"));
                associationVertex.AddEdge(root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), targetClass);
                associationVertex.AddVertex(root.Get(false, @"System\Meta\Base\Vertex\$MinCardinality"), minCardinality);
                associationVertex.AddVertex(root.Get(false, @"System\Meta\Base\Vertex\$MaxCardinality"), maxCardinality);

                return associationVertex;
            }

            private IVertex GetTypeVertex(JsonElement value)
            {
                switch (value.ValueKind)
                {
                    case JsonValueKind.String:
                        return GetZeroType("String");
                    case JsonValueKind.Number:
                        if (value.TryGetInt64(out _))
                            return GetZeroType("Integer");
                        return GetZeroType("Double") ?? GetZeroType("Float") ?? GetZeroType("Decimal") ?? GetZeroType("String");
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return GetZeroType("Boolean") ?? GetZeroType("String");
                    case JsonValueKind.Null:
                        return GetZeroType("Null") ?? GetZeroType("String");
                    default:
                        return GetZeroType("String");
                }
            }

            private IVertex GetZeroType(string typeName)
            {
                if (zeroTypesRoot == null)
                    return null;

                IVertex typeVertex = zeroTypesRoot.Get(false, typeName);
                return typeVertex ?? zeroTypesRoot.Get(false, "String");
            }

            private string CreateUniqueClassName(string className)
            {
                if (!classNameCounts.TryGetValue(className, out int count))
                {
                    classNameCounts[className] = 1;
                    return className;
                }

                count++;
                classNameCounts[className] = count;
                return className + count.ToString();
            }
        }

        private static void CreateData(JsonElement rootElement, IVertex dataRoot, SchemaClass rootClass, SchemaContext context)
        {
            IVertex rootInstance = dataRoot.AddVertex(rootClass.ClassVertex, "");
            rootInstance.AddEdge(MinusZero.Instance.Is, rootClass.ClassVertex);

            if (rootElement.ValueKind == JsonValueKind.Object)
                PopulateObjectData(rootElement, rootInstance, rootClass, context);
            else if (rootElement.ValueKind == JsonValueKind.Array)
                PopulateArrayData(rootElement, rootInstance, rootClass.Properties["items"], context);
            else
                PopulateSingleValue(rootInstance, rootClass.Properties["value"], rootElement, context);
        }

        private static void PopulateObjectData(JsonElement obj, IVertex instanceVertex, SchemaClass schemaClass, SchemaContext context)
        {
            foreach (JsonProperty property in obj.EnumerateObject())
            {
                if (!schemaClass.Properties.TryGetValue(property.Name, out SchemaProperty schemaProperty))
                    continue;

                if (schemaProperty.IsArray)
                    PopulateArrayData(property.Value, instanceVertex, schemaProperty, context);
                else
                    PopulateSingleValue(instanceVertex, schemaProperty, property.Value, context);
            }
        }

        private static void PopulateSingleValue(IVertex instanceVertex, SchemaProperty schemaProperty, JsonElement value, SchemaContext context)
        {
            if (schemaProperty.ValueKind == SchemaValueKind.Primitive)
            {
                instanceVertex.AddVertex(schemaProperty.PropertyVertex, ConvertPrimitive(value));
                return;
            }

            if (schemaProperty.ValueKind == SchemaValueKind.Object)
            {
                IVertex objectVertex = instanceVertex.AddVertex(schemaProperty.PropertyVertex, "");
                objectVertex.AddEdge(MinusZero.Instance.Is, schemaProperty.TargetClass.ClassVertex);

                if (value.ValueKind == JsonValueKind.Object)
                    PopulateObjectData(value, objectVertex, schemaProperty.TargetClass, context);
            }
        }

        private static void PopulateArrayData(JsonElement arrayElement, IVertex instanceVertex, SchemaProperty schemaProperty, SchemaContext context)
        {
            if (arrayElement.ValueKind != JsonValueKind.Array)
            {
                PopulateSingleValue(instanceVertex, schemaProperty, arrayElement, context);
                return;
            }

            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (schemaProperty.ValueKind == SchemaValueKind.Primitive)
                {
                    instanceVertex.AddVertex(schemaProperty.PropertyVertex, ConvertPrimitive(item));
                    continue;
                }

                IVertex objectVertex = instanceVertex.AddVertex(schemaProperty.PropertyVertex, "");
                objectVertex.AddEdge(MinusZero.Instance.Is, schemaProperty.TargetClass.ClassVertex);

                if (item.ValueKind == JsonValueKind.Object)
                    PopulateObjectData(item, objectVertex, schemaProperty.TargetClass, context);
            }
        }

        private static object ConvertPrimitive(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    return value.GetString();
                case JsonValueKind.Number:
                    if (value.TryGetInt64(out long longValue))
                        return longValue;
                    return value.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    return value.ToString();
            }
        }
    }
}
