using m0.Foundation;
using m0.Graph;
using m0.Lib.REST;
using m0.ZeroTypes;
using System;
using System.Buffers;
using System.Collections.Generic;
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

            IVertex jsonRootDefinitionVertex = GraphUtil.GetQueryOutFirst(to, "$JsonRootDefinition", null);

            // Determine where to create new classes
            IVertex newClassDefinitionsVertex = GraphUtil.GetQueryOutFirst(to, "$NewClassDefinitions", null);
            IVertex newClassesRoot = newClassDefinitionsVertex ?? to;
            
            // Collect all vertices where to look for existing classes
            var existingClassesRoots = new List<IVertex>();
            existingClassesRoots.Add(newClassesRoot); // Always check in the new classes root first
            
            IList<IEdge> existingClassDefinitionsEdges = GraphUtil.GetQueryOut(to, "$ExistingClassDefinitions", null);
            foreach (IEdge edge in existingClassDefinitionsEdges)
            {
                if (edge.To != null && !existingClassesRoots.Contains(edge.To))
                    existingClassesRoots.Add(edge.To);
            }
            
            IVertex dataRoot = to;

            var context = new SchemaContext(newClassesRoot, existingClassesRoots, jsonRootDefinitionVertex);

            context.BuildSchema(document.RootElement);
            CreateData(document.RootElement, dataRoot, context);
        }

        /// <summary>
        /// Maps a JSON response string to GVM vertices using a known output type.
        /// Used by remote REST endpoint calls where the output class is already defined.
        /// </summary>
        public static void MapJsonResponseToVertex(string json, IVertex targetVertex, IVertex outputTypeVertex, IVertex classesRoot)
        {
            if (targetVertex == null || string.IsNullOrWhiteSpace(json))
                return;

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                // Response is not valid JSON, add as plain string
                targetVertex.AddVertex(null, json);
                return;
            }

            bool isPrimitive = IsPrimitiveTypeVertex(outputTypeVertex);

            var existingClassesRoots = new List<IVertex>();
            if (classesRoot != null)
                existingClassesRoots.Add(classesRoot);

            IVertex classesRootToUse = classesRoot ?? targetVertex;
            var context = new SchemaContext(classesRootToUse, existingClassesRoots, null);

            JsonElement root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in root.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Null)
                        continue;

                    if (isPrimitive || item.ValueKind != JsonValueKind.Object)
                    {
                        targetVertex.AddVertex(null, TypeConverter.ConvertJsonElementToPrimitive(item));
                    }
                    else
                    {
                        MapJsonObjectToVertex(item, targetVertex, outputTypeVertex, context);
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (isPrimitive)
                {
                    targetVertex.AddVertex(null, root.ToString());
                }
                else
                {
                    MapJsonObjectToVertex(root, targetVertex, outputTypeVertex, context);
                }
            }
            else if (root.ValueKind != JsonValueKind.Null)
            {
                targetVertex.AddVertex(null, TypeConverter.ConvertJsonElementToPrimitive(root));
            }
        }

        private static void MapJsonObjectToVertex(JsonElement obj, IVertex parentVertex, IVertex classVertex, SchemaContext context)
        {
            string className = GraphUtil.GetStringValue(classVertex);
            SchemaClass schemaClass = context.FindOrCreateSchemaClassForVertex(classVertex, className);

            IVertex instanceVertex = parentVertex.AddVertex(classVertex, "");
            instanceVertex.AddEdge(MinusZero.Instance.Is, classVertex);

            PopulateObjectData(obj, instanceVertex, schemaClass, context);
        }

        private static bool IsPrimitiveTypeVertex(IVertex typeVertex)
        {
            if (typeVertex == null)
                return true;

            string typeName = GraphUtil.GetStringValue(typeVertex);

            if (typeName == "String" || typeName == "Integer" || typeName == "Double" ||
                typeName == "Boolean" || typeName == "Float" || typeName == "Decimal")
                return true;

            // Enums are also primitive (stored as string values)
            if (GraphUtil.GetQueryOutCount(typeVertex, "$Inherits", "EnumBase") > 0)
                return true;

            return false;
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
            private readonly IVertex newClassesRoot;
            private readonly IList<IVertex> existingClassesRoots;
            private readonly IVertex jsonRootDefinitionVertex;
            private readonly IVertex zeroTypesRoot;
            private readonly IVertex isJsonArrayMeta;
            private readonly IDictionary<string, SchemaClass> classesByPath = new Dictionary<string, SchemaClass>();
            private readonly IDictionary<string, int> classNameCounts = new Dictionary<string, int>();

            public SchemaContext(IVertex newClassesRoot, IList<IVertex> existingClassesRoots, IVertex jsonRootDefinitionVertex)
            {
                this.newClassesRoot = newClassesRoot;
                this.existingClassesRoots = existingClassesRoots;
                this.jsonRootDefinitionVertex = jsonRootDefinitionVertex;
                zeroTypesRoot = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes");
                isJsonArrayMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$IsJsonArray");
            }

            /// <summary>
            /// Gets the class type for a root-level JSON property from $JsonRootDefinition.
            /// Looks for InputParameter with matching name and returns its $EdgeTarget.
            /// </summary>
            private IVertex GetTypeFromJsonRootDefinition(string propertyName)
            {
                if (jsonRootDefinitionVertex == null)
                    return null;

                // Find InputParameter with matching name
                IVertex inputParameter = GraphUtil.GetQueryOutFirst(jsonRootDefinitionVertex, "InputParameter", propertyName);
                if (inputParameter == null)
                    return null;

                // Get the $EdgeTarget - this is the actual class type
                IVertex edgeTarget = GraphUtil.GetQueryOutFirst(inputParameter, "$EdgeTarget", null);
                return edgeTarget;
            }

            public void BuildSchema(JsonElement rootElement)
            {
                if (rootElement.ValueKind == JsonValueKind.Object)
                {
                    BuildNestedClassesFromObject(rootElement, "Root");
                }
                else if (rootElement.ValueKind == JsonValueKind.Array)
                {
                    BuildNestedClassesFromArray(rootElement, "Root");
                }
            }

            private void BuildNestedClassesFromObject(JsonElement obj, string path)
            {
                foreach (JsonProperty property in obj.EnumerateObject())
                {
                    string propertyPath = path + "." + property.Name;

                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        // At root level, try to get type from $JsonRootDefinition
                        IVertex typeFromDefinition = null;
                        if (path == "Root")
                        {
                            typeFromDefinition = GetTypeFromJsonRootDefinition(property.Name);
                        }

                        if (typeFromDefinition != null && !VertexOperations.IsAtomicType(typeFromDefinition))
                        {
                            // Use the class type from definition
                            BuildObjectClassWithKnownType(property.Value, typeFromDefinition, propertyPath);
                        }
                        else
                        {
                            BuildObjectClass(property.Value, property.Name, propertyPath);
                        }
                    }
                    else if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        // At root level, try to get type from $JsonRootDefinition for array items
                        IVertex typeFromDefinition = null;
                        if (path == "Root")
                        {
                            typeFromDefinition = GetTypeFromJsonRootDefinition(property.Name);
                        }

                        if (typeFromDefinition != null && !VertexOperations.IsAtomicType(typeFromDefinition))
                        {
                            BuildNestedClassesFromArrayWithKnownType(property.Value, typeFromDefinition, propertyPath);
                        }
                        else
                        {
                            BuildNestedClassesFromArray(property.Value, propertyPath);
                        }
                    }
                }
            }

            private void BuildNestedClassesFromArray(JsonElement arrayElement, string path)
            {
                if (arrayElement.GetArrayLength() == 0)
                    return;

                JsonElement firstElement = arrayElement.EnumerateArray().First();

                if (firstElement.ValueKind == JsonValueKind.Object)
                {
                    string className = GetClassNameFromPath(path);
                    SchemaClass itemClass = BuildObjectClass(firstElement, className, path + ".Item");
                    MergeArrayObjectItems(itemClass, arrayElement, path + ".Item");
                }
                else if (firstElement.ValueKind == JsonValueKind.Array)
                {
                    BuildNestedClassesFromArray(firstElement, path + ".Item");
                }
            }

            private void BuildNestedClassesFromArrayWithKnownType(JsonElement arrayElement, IVertex knownClassType, string path)
            {
                if (arrayElement.GetArrayLength() == 0)
                    return;

                JsonElement firstElement = arrayElement.EnumerateArray().First();

                if (firstElement.ValueKind == JsonValueKind.Object)
                {
                    SchemaClass itemClass = BuildObjectClassWithKnownType(firstElement, knownClassType, path + ".Item");
                    MergeArrayObjectItems(itemClass, arrayElement, path + ".Item");
                }
                else if (firstElement.ValueKind == JsonValueKind.Array)
                {
                    BuildNestedClassesFromArray(firstElement, path + ".Item");
                }
            }

            private SchemaClass BuildObjectClassWithKnownType(JsonElement obj, IVertex knownClassType, string path)
            {
                if (classesByPath.TryGetValue(path, out SchemaClass existing))
                {
                    MergeObjectProperties(existing, obj, path);
                    return existing;
                }

                // Use the known class type directly
                SchemaClass schemaClass = CreateClassFromKnownType(knownClassType, path);
                MergeObjectProperties(schemaClass, obj, path);
                return schemaClass;
            }

            private SchemaClass CreateClassFromKnownType(IVertex knownClassType, string path)
            {
                string className = GraphUtil.GetStringValue(knownClassType);
                
                var schemaClass = new SchemaClass
                {
                    Name = className,
                    ClassVertex = knownClassType
                };

                // Cache FIRST to prevent infinite recursion with circular references
                classesByPath[path] = schemaClass;
                classNameCounts[className] = 1; // Mark as used

                // THEN load existing properties from the class
                LoadExistingProperties(schemaClass, knownClassType);

                return schemaClass;
            }

            private string GetClassNameFromPath(string path)
            {
                int lastDot = path.LastIndexOf('.');
                return lastDot >= 0 ? path.Substring(lastDot + 1) : path;
            }

            public SchemaClass GetClassForPath(string path)
            {
                classesByPath.TryGetValue(path, out SchemaClass schemaClass);
                return schemaClass;
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
                // Check if class with this name already exists in any of the existing class roots
                IVertex existingClassVertex = FindExistingClass(classNameHint);
                
                if (existingClassVertex != null)
                {
                    var schemaClass = new SchemaClass
                    {
                        Name = classNameHint,
                        ClassVertex = existingClassVertex
                    };
                    
                    // Cache FIRST to prevent infinite recursion with circular references
                    classesByPath[path] = schemaClass;
                    classNameCounts[classNameHint] = 1; // Mark as used
                    
                    // THEN load existing properties from the class
                    LoadExistingProperties(schemaClass, existingClassVertex);
                    
                    return schemaClass;
                }
                
                // Create new class in newClassesRoot
                string className = CreateUniqueClassName(classNameHint);
                IVertex classVertex = GraphUtil.AddClass(newClassesRoot, className);
                var newSchemaClass = new SchemaClass
                {
                    Name = className,
                    ClassVertex = classVertex
                };
                classesByPath[path] = newSchemaClass;
                return newSchemaClass;
            }

            private IVertex FindExistingClass(string className)
            {
                // Search in all existing class roots (including newClassesRoot which is first in the list)
                foreach (IVertex root in existingClassesRoots)
                {
                    foreach (IEdge edge in root.OutEdges)
                    {
                        string metaValue = GraphUtil.GetStringValue(edge.Meta);
                        if (metaValue == "Class" && GraphUtil.GetStringValue(edge.To) == className)
                        {
                            return edge.To;
                        }
                    }
                }
                return null;
            }

            private void LoadExistingProperties(SchemaClass schemaClass, IVertex classVertex)
            {
                // Load Attributes
                IList<IEdge> attributes = GraphUtil.GetQueryOut(classVertex, "Attribute", null);
                foreach (IEdge attrEdge in attributes)
                {
                    string propertyName = GraphUtil.GetStringValue(attrEdge.To);
                    bool isArray = ZeroTypes.VertexOperations.IsMetaVertexOfManyMultiplicity(attrEdge.To);
                    
                    IVertex edgeTarget = GraphUtil.GetQueryOutFirst(attrEdge.To, "$EdgeTarget", null);
                    
                    var schemaProperty = new SchemaProperty
                    {
                        Name = propertyName,
                        IsArray = isArray,
                        ValueKind = SchemaValueKind.Primitive,
                        PropertyVertex = attrEdge.To,
                        TargetTypeVertex = edgeTarget
                    };
                    schemaClass.Properties[propertyName] = schemaProperty;
                }

                // Load Associations
                IList<IEdge> associations = GraphUtil.GetQueryOut(classVertex, "Association", null);
                foreach (IEdge assocEdge in associations)
                {
                    string propertyName = GraphUtil.GetStringValue(assocEdge.To);
                    bool isArray = ZeroTypes.VertexOperations.IsMetaVertexOfManyMultiplicity(assocEdge.To);
                    
                    IVertex edgeTarget = GraphUtil.GetQueryOutFirst(assocEdge.To, "$EdgeTarget", null);
                    
                    // Find or create target SchemaClass
                    SchemaClass targetClass = null;
                    if (edgeTarget != null)
                    {
                        string targetClassName = GraphUtil.GetStringValue(edgeTarget);
                        targetClass = FindOrCreateSchemaClassForVertex(edgeTarget, targetClassName);
                    }
                    
                    var schemaProperty = new SchemaProperty
                    {
                        Name = propertyName,
                        IsArray = isArray,
                        ValueKind = SchemaValueKind.Object,
                        PropertyVertex = assocEdge.To,
                        TargetClass = targetClass
                    };
                    schemaClass.Properties[propertyName] = schemaProperty;
                }

                // Load Aggregations
                IList<IEdge> aggregations = GraphUtil.GetQueryOut(classVertex, "Aggregation", null);
                foreach (IEdge aggEdge in aggregations)
                {
                    string propertyName = GraphUtil.GetStringValue(aggEdge.To);
                    bool isArray = ZeroTypes.VertexOperations.IsMetaVertexOfManyMultiplicity(aggEdge.To);
                    
                    IVertex edgeTarget = GraphUtil.GetQueryOutFirst(aggEdge.To, "$EdgeTarget", null);
                    
                    // Find or create target SchemaClass
                    SchemaClass targetClass = null;
                    if (edgeTarget != null)
                    {
                        string targetClassName = GraphUtil.GetStringValue(edgeTarget);
                        targetClass = FindOrCreateSchemaClassForVertex(edgeTarget, targetClassName);
                    }
                    
                    var schemaProperty = new SchemaProperty
                    {
                        Name = propertyName,
                        IsArray = isArray,
                        ValueKind = SchemaValueKind.Object,
                        PropertyVertex = aggEdge.To,
                        TargetClass = targetClass
                    };
                    schemaClass.Properties[propertyName] = schemaProperty;
                }
            }

            public SchemaClass FindOrCreateSchemaClassForVertex(IVertex classVertex, string className)
            {
                // Check if we already have this class in our cache
                foreach (var kvp in classesByPath)
                {
                    if (kvp.Value.ClassVertex == classVertex)
                        return kvp.Value;
                }
                
                // Create a wrapper SchemaClass for the existing vertex
                var schemaClass = new SchemaClass
                {
                    Name = className,
                    ClassVertex = classVertex
                };
                
                // Cache it FIRST to prevent infinite recursion with circular references
                string generatedPath = "Existing." + className;
                classesByPath[generatedPath] = schemaClass;
                
                // THEN load its properties recursively
                LoadExistingProperties(schemaClass, classVertex);
                
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
                    SchemaClass targetClass = BuildObjectClass(firstElement, propertyName, path + ".Item");
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
                SchemaClass arrayClass = CreateClass(propertyName, path);
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
                string typeName = TypeConverter.GetGvmTypeNameFromJsonElement(value);
                return GetZeroType(typeName);
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

        private static void CreateData(JsonElement rootElement, IVertex dataRoot, SchemaContext context)
        {
            if (rootElement.ValueKind == JsonValueKind.Object)
                PopulateObjectDataDirectly(rootElement, dataRoot, context, "Root");
            else if (rootElement.ValueKind == JsonValueKind.Array)
                PopulateArrayDataDirectly(rootElement, dataRoot, context, "Root");
            else if (rootElement.ValueKind != JsonValueKind.Null)
                dataRoot.AddVertex(null, TypeConverter.ConvertJsonElementToPrimitive(rootElement));
        }

        private static void PopulateObjectDataDirectly(JsonElement obj, IVertex parentVertex, SchemaContext context, string path)
        {
            foreach (JsonProperty property in obj.EnumerateObject())
            {
                string propertyPath = path + "." + property.Name;

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    SchemaClass schemaClass = context.GetClassForPath(propertyPath);
                    if (schemaClass != null)
                    {
                        IVertex objectVertex = parentVertex.AddVertex(schemaClass.ClassVertex, property.Name);
                        objectVertex.AddEdge(MinusZero.Instance.Is, schemaClass.ClassVertex);
                        PopulateObjectData(property.Value, objectVertex, schemaClass, context);
                    }
                }
                else if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    PopulateArrayDataDirectly(property.Value, parentVertex, context, propertyPath);
                }
                else if (property.Value.ValueKind != JsonValueKind.Null)
                {
                    parentVertex.AddVertex(null, property.Name).AddVertex(null, TypeConverter.ConvertJsonElementToPrimitive(property.Value));
                }
            }
        }

        private static void PopulateArrayDataDirectly(JsonElement arrayElement, IVertex parentVertex, SchemaContext context, string path)
        {
            if (arrayElement.ValueKind != JsonValueKind.Array)
                return;

            SchemaClass itemClass = context.GetClassForPath(path + ".Item");

            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Null)
                    continue;
                if (item.ValueKind == JsonValueKind.Object && itemClass != null)
                {
                    IVertex objectVertex = parentVertex.AddVertex(itemClass.ClassVertex, "");
                    objectVertex.AddEdge(MinusZero.Instance.Is, itemClass.ClassVertex);
                    PopulateObjectData(item, objectVertex, itemClass, context);
                }
                else if (item.ValueKind == JsonValueKind.Array)
                {
                    PopulateArrayDataDirectly(item, parentVertex, context, path + ".Item");
                }
                else
                {
                    parentVertex.AddVertex(null, TypeConverter.ConvertJsonElementToPrimitive(item));
                }
            }
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
            if (value.ValueKind == JsonValueKind.Null)
                return;

            if (schemaProperty.ValueKind == SchemaValueKind.Primitive)
            {
                instanceVertex.AddVertex(schemaProperty.PropertyVertex, TypeConverter.ConvertJsonElementToPrimitive(value));
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
                if (item.ValueKind == JsonValueKind.Null)
                    continue;
                if (schemaProperty.ValueKind == SchemaValueKind.Primitive)
                {
                    instanceVertex.AddVertex(schemaProperty.PropertyVertex, TypeConverter.ConvertJsonElementToPrimitive(item));
                    continue;
                }

                IVertex objectVertex = instanceVertex.AddVertex(schemaProperty.PropertyVertex, "");
                objectVertex.AddEdge(MinusZero.Instance.Is, schemaProperty.TargetClass.ClassVertex);

                if (item.ValueKind == JsonValueKind.Object)
                    PopulateObjectData(item, objectVertex, schemaProperty.TargetClass, context);
            }
        }

    }
}
