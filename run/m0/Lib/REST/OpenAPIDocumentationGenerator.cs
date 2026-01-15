using m0.Foundation;
using m0.Graph;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace m0.Lib.REST
{
    public class OpenApiDocumentationGenerator
    {
        // Primitive type names that should generate Request/Response wrappers
        private static readonly HashSet<string> PrimitiveTypes = new HashSet<string>
        {
            "String", "Integer", "Float", "Double", "Boolean", "Decimal"
        };

        /// <summary>
        /// Generates OpenAPI 3.0.1 documentation from GVM vertex structure containing functions and classes.
        /// </summary>
        /// <param name="baseVertex">The starting vertex containing Function and Class definitions</param>
        /// <returns>OpenAPI JSON documentation string</returns>
        static public string GetOpenApiDocumentation(IVertex baseVertex)
        {
            var buffer = new ArrayBufferWriter<byte>();
            var options = new JsonWriterOptions
            {
                Indented = true,
                SkipValidation = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            using (var writer = new Utf8JsonWriter(buffer, options))
            {
                writer.WriteStartObject();

                // OpenAPI version
                writer.WriteString("openapi", "3.0.1");

                // Info section
                writer.WritePropertyName("info");
                writer.WriteStartObject();
                writer.WriteString("title", GraphUtil.GetStringValue(baseVertex));
                writer.WriteString("version", "1.0.0");
                writer.WriteEndObject();

                // Collect functions
                var functions = CollectFunctions(baseVertex);
                
                // Collect only classes that are referenced by functions (and their dependencies)
                var classes = new List<ClassInfo>();
                CollectReferencedClasses(functions, classes);

                // Write paths section
                WritePaths(writer, functions, classes);

                // Write components section
                WriteComponents(writer, functions, classes);

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(buffer.WrittenSpan);
        }

        private static List<FunctionInfo> CollectFunctions(IVertex baseVertex)
        {
            var functions = new List<FunctionInfo>();

            foreach (IEdge edge in baseVertex.OutEdges)
            {
                string metaValue = GraphUtil.GetStringValue(edge.Meta);
                if (metaValue == "Function")
                {
                    var functionInfo = new FunctionInfo
                    {
                        Name = GraphUtil.GetStringValue(edge.To)
                    };

                    // Get output type
                    IVertex outputVertex = GraphUtil.GetQueryOutFirst(edge.To, "Output", null);
                    if (outputVertex != null)
                    {
                        functionInfo.OutputType = GetTypeName(outputVertex);
                        functionInfo.OutputTypeVertex = outputVertex;
                    }

                    // Get input parameters
                    IList<IEdge> inputParams = GraphUtil.GetQueryOut(edge.To, "InputParameter", null);
                    foreach (IEdge paramEdge in inputParams)
                    {
                        var paramInfo = new ParameterInfo
                        {
                            Name = GraphUtil.GetStringValue(paramEdge.To)
                        };

                        IVertex edgeTarget = GraphUtil.GetQueryOutFirst(paramEdge.To, "$EdgeTarget", null);
                        if (edgeTarget != null)
                        {
                            paramInfo.Type = GetTypeName(edgeTarget);
                            paramInfo.TypeVertex = edgeTarget;
                        }

                        functionInfo.InputParameters.Add(paramInfo);
                    }

                    functions.Add(functionInfo);
                }
            }

            return functions;
        }

        private static ClassInfo CollectClassFromVertex(IVertex classVertex)
        {
            var classInfo = new ClassInfo
            {
                Name = GraphUtil.GetStringValue(classVertex),
                SourceVertex = classVertex
            };

            // Get attributes
            IList<IEdge> attributes = GraphUtil.GetQueryOut(classVertex, "Attribute", null);
            foreach (IEdge attrEdge in attributes)
            {
                var propInfo = new PropertyInfo
                {
                    Name = GraphUtil.GetStringValue(attrEdge.To),
                    IsArray = false
                };

                IVertex edgeTarget = GraphUtil.GetQueryOutFirst(attrEdge.To, "$EdgeTarget", null);
                if (edgeTarget != null)
                {
                    propInfo.Type = GetTypeName(edgeTarget);
                    propInfo.TypeVertex = edgeTarget;
                }

                classInfo.Properties.Add(propInfo);
            }

            // Get aggregations (arrays)
            IList<IEdge> aggregations = GraphUtil.GetQueryOut(classVertex, "Aggregation", null);
            foreach (IEdge aggEdge in aggregations)
            {
                var propInfo = new PropertyInfo
                {
                    Name = GraphUtil.GetStringValue(aggEdge.To),
                    IsArray = true
                };

                IVertex edgeTarget = GraphUtil.GetQueryOutFirst(aggEdge.To, "$EdgeTarget", null);
                if (edgeTarget != null)
                {
                    propInfo.Type = GetTypeName(edgeTarget);
                    propInfo.TypeVertex = edgeTarget;
                }

                classInfo.Properties.Add(propInfo);
            }

            // Get associations (arrays of references to other classes)
            IList<IEdge> associations = GraphUtil.GetQueryOut(classVertex, "Association", null);
            foreach (IEdge assocEdge in associations)
            {
                var propInfo = new PropertyInfo
                {
                    Name = GraphUtil.GetStringValue(assocEdge.To),
                    IsArray = true
                };

                IVertex edgeTarget = GraphUtil.GetQueryOutFirst(assocEdge.To, "$EdgeTarget", null);
                if (edgeTarget != null)
                {
                    propInfo.Type = GetTypeName(edgeTarget);
                    propInfo.TypeVertex = edgeTarget;
                }

                classInfo.Properties.Add(propInfo);
            }

            return classInfo;
        }

        private static string GetTypeName(IVertex typeVertex)
        {
            string value = GraphUtil.GetStringValue(typeVertex);
            
            // Handle case when type is referenced by edge (e.g., @Integer)
            // The vertex value might be the type name directly
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            return "String"; // Default fallback
        }

        private static void CollectReferencedClasses(List<FunctionInfo> functions, List<ClassInfo> classes)
        {
            var collectedNames = new HashSet<string>(classes.Select(c => c.Name));
            var verticesToProcess = new Queue<IVertex>();

            // Collect type vertices from functions
            foreach (var function in functions)
            {
                if (function.OutputTypeVertex != null)
                {
                    verticesToProcess.Enqueue(function.OutputTypeVertex);
                }
                foreach (var param in function.InputParameters)
                {
                    if (param.TypeVertex != null)
                    {
                        verticesToProcess.Enqueue(param.TypeVertex);
                    }
                }
            }

            // Collect type vertices from existing classes
            foreach (var classInfo in classes.ToList())
            {
                foreach (var prop in classInfo.Properties)
                {
                    if (prop.TypeVertex != null)
                    {
                        verticesToProcess.Enqueue(prop.TypeVertex);
                    }
                }
            }

            // Process vertices and collect referenced classes
            while (verticesToProcess.Count > 0)
            {
                IVertex typeVertex = verticesToProcess.Dequeue();
                string typeName = GetTypeName(typeVertex);

                // Skip primitive types
                if (PrimitiveTypes.Contains(typeName))
                {
                    continue;
                }

                // Skip already collected classes
                if (collectedNames.Contains(typeName))
                {
                    continue;
                }

                // Check if this vertex represents a class (has Attribute, Aggregation, or Association)
                IList<IEdge> attributes = GraphUtil.GetQueryOut(typeVertex, "Attribute", null);
                IList<IEdge> aggregations = GraphUtil.GetQueryOut(typeVertex, "Aggregation", null);
                IList<IEdge> associations = GraphUtil.GetQueryOut(typeVertex, "Association", null);

                if (attributes.Count > 0 || aggregations.Count > 0 || associations.Count > 0)
                {
                    // This is a class, collect it
                    ClassInfo newClass = CollectClassFromVertex(typeVertex);
                    classes.Add(newClass);
                    collectedNames.Add(typeName);

                    // Add its property types to process queue
                    foreach (var prop in newClass.Properties)
                    {
                        if (prop.TypeVertex != null && !PrimitiveTypes.Contains(prop.Type) && !collectedNames.Contains(prop.Type))
                        {
                            verticesToProcess.Enqueue(prop.TypeVertex);
                        }
                    }
                }
            }
        }

        private static void WritePaths(Utf8JsonWriter writer, List<FunctionInfo> functions, List<ClassInfo> classes)
        {
            writer.WritePropertyName("paths");
            writer.WriteStartObject();

            // Write function endpoints
            foreach (var function in functions)
            {
                WritePathForFunction(writer, function, classes);
            }

            // Write root endpoint
            WriteRootEndpoint(writer);

            writer.WriteEndObject();
        }

        private static void WritePathForFunction(Utf8JsonWriter writer, FunctionInfo function, List<ClassInfo> classes)
        {
            writer.WritePropertyName("/" + function.Name);
            writer.WriteStartObject();

            writer.WritePropertyName("post");
            writer.WriteStartObject();

            // Tags
            writer.WritePropertyName("tags");
            writer.WriteStartArray();
            writer.WriteStringValue("Expose");
            writer.WriteEndArray();

            // Request body
            WriteRequestBody(writer, function, classes);

            // Responses
            WriteResponses(writer, function, classes);

            writer.WriteEndObject(); // post
            writer.WriteEndObject(); // path
        }

        private static void WriteRequestBody(Utf8JsonWriter writer, FunctionInfo function, List<ClassInfo> classes)
        {
            writer.WritePropertyName("requestBody");
            writer.WriteStartObject();

            writer.WritePropertyName("content");
            writer.WriteStartObject();

            string schemaRef = GetRequestSchemaRef(function, classes);

            // application/json
            WriteContentTypeWithSchemaRef(writer, "application/json", schemaRef);
            // text/json
            WriteContentTypeWithSchemaRef(writer, "text/json", schemaRef);
            // application/*+json
            WriteContentTypeWithSchemaRef(writer, "application/*+json", schemaRef);

            writer.WriteEndObject(); // content
            writer.WriteEndObject(); // requestBody
        }

        private static void WriteResponses(Utf8JsonWriter writer, FunctionInfo function, List<ClassInfo> classes)
        {
            writer.WritePropertyName("responses");
            writer.WriteStartObject();

            // 200 Success
            writer.WritePropertyName("200");
            writer.WriteStartObject();
            writer.WriteString("description", "Success");

            writer.WritePropertyName("content");
            writer.WriteStartObject();

            string responseSchemaRef = GetResponseSchemaRef(function, classes);
            WriteContentTypeWithSchemaRef(writer, "text/plain", responseSchemaRef);
            WriteContentTypeWithSchemaRef(writer, "application/json", responseSchemaRef);
            WriteContentTypeWithSchemaRef(writer, "text/json", responseSchemaRef);

            writer.WriteEndObject(); // content
            writer.WriteEndObject(); // 200

            // 400 Bad Request
            writer.WritePropertyName("400");
            writer.WriteStartObject();
            writer.WriteString("description", "Bad Request");

            writer.WritePropertyName("content");
            writer.WriteStartObject();

            WriteContentTypeWithSchemaRef(writer, "text/plain", "#/components/schemas/ErrorResponse");
            WriteContentTypeWithSchemaRef(writer, "application/json", "#/components/schemas/ErrorResponse");
            WriteContentTypeWithSchemaRef(writer, "text/json", "#/components/schemas/ErrorResponse");

            writer.WriteEndObject(); // content
            writer.WriteEndObject(); // 400

            writer.WriteEndObject(); // responses
        }

        private static void WriteContentTypeWithSchemaRef(Utf8JsonWriter writer, string contentType, string schemaRef)
        {
            writer.WritePropertyName(contentType);
            writer.WriteStartObject();
            writer.WritePropertyName("schema");
            writer.WriteStartObject();
            writer.WriteString("$ref", schemaRef);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        private static string GetRequestSchemaRef(FunctionInfo function, List<ClassInfo> classes)
        {
            // If function has single input parameter of class type, use that class directly
            if (function.InputParameters.Count == 1)
            {
                var param = function.InputParameters[0];
                if (!PrimitiveTypes.Contains(param.Type) && classes.Any(c => c.Name == param.Type))
                {
                    return "#/components/schemas/" + param.Type;
                }
            }

            // Otherwise, generate a request wrapper
            return "#/components/schemas/" + function.Name + "Request";
        }

        private static string GetResponseSchemaRef(FunctionInfo function, List<ClassInfo> classes)
        {
            // If output is a class type, use that class directly
            if (!string.IsNullOrEmpty(function.OutputType) && 
                !PrimitiveTypes.Contains(function.OutputType) && 
                classes.Any(c => c.Name == function.OutputType))
            {
                return "#/components/schemas/" + function.OutputType;
            }

            // Otherwise, generate a response wrapper
            return "#/components/schemas/" + function.Name + "Response";
        }

        private static void WriteRootEndpoint(Utf8JsonWriter writer)
        {
            writer.WritePropertyName("/");
            writer.WriteStartObject();

            writer.WritePropertyName("get");
            writer.WriteStartObject();

            writer.WritePropertyName("tags");
            writer.WriteStartArray();
            writer.WriteStringValue("RestTest");
            writer.WriteEndArray();

            writer.WritePropertyName("responses");
            writer.WriteStartObject();

            writer.WritePropertyName("200");
            writer.WriteStartObject();
            writer.WriteString("description", "Success");
            writer.WriteEndObject();

            writer.WriteEndObject(); // responses
            writer.WriteEndObject(); // get
            writer.WriteEndObject(); // path
        }

        private static void WriteComponents(Utf8JsonWriter writer, List<FunctionInfo> functions, List<ClassInfo> classes)
        {
            writer.WritePropertyName("components");
            writer.WriteStartObject();

            writer.WritePropertyName("schemas");
            writer.WriteStartObject();

            // Write request/response schemas for functions with primitive types
            foreach (var function in functions)
            {
                bool needsRequestWrapper = NeedsRequestWrapper(function, classes);
                bool needsResponseWrapper = NeedsResponseWrapper(function, classes);

                if (needsRequestWrapper)
                {
                    WriteRequestSchema(writer, function);
                }

                if (needsResponseWrapper)
                {
                    WriteResponseSchema(writer, function);
                }
            }

            // Write ErrorResponse schema
            WriteErrorResponseSchema(writer);

            // Write class schemas
            foreach (var classInfo in classes)
            {
                WriteClassSchema(writer, classInfo);
            }

            writer.WriteEndObject(); // schemas
            writer.WriteEndObject(); // components
        }

        private static bool NeedsRequestWrapper(FunctionInfo function, List<ClassInfo> classes)
        {
            // Needs wrapper if not single class parameter
            if (function.InputParameters.Count != 1)
                return true;

            var param = function.InputParameters[0];
            return PrimitiveTypes.Contains(param.Type) || !classes.Any(c => c.Name == param.Type);
        }

        private static bool NeedsResponseWrapper(FunctionInfo function, List<ClassInfo> classes)
        {
            // Needs wrapper if output is primitive or not a known class
            if (string.IsNullOrEmpty(function.OutputType))
                return true;

            return PrimitiveTypes.Contains(function.OutputType) || !classes.Any(c => c.Name == function.OutputType);
        }

        private static void WriteRequestSchema(Utf8JsonWriter writer, FunctionInfo function)
        {
            writer.WritePropertyName(function.Name + "Request");
            writer.WriteStartObject();

            writer.WriteString("type", "object");

            writer.WritePropertyName("properties");
            writer.WriteStartObject();

            foreach (var param in function.InputParameters)
            {
                writer.WritePropertyName(param.Name);
                WriteTypeDefinition(writer, param.Type);
            }

            writer.WriteEndObject(); // properties

            writer.WriteBoolean("additionalProperties", false);

            writer.WriteEndObject();
        }

        private static void WriteResponseSchema(Utf8JsonWriter writer, FunctionInfo function)
        {
            writer.WritePropertyName(function.Name + "Response");
            writer.WriteStartObject();

            writer.WriteString("type", "object");

            writer.WritePropertyName("properties");
            writer.WriteStartObject();

            writer.WritePropertyName("result");
            WriteTypeDefinition(writer, function.OutputType);

            writer.WriteEndObject(); // properties

            writer.WriteBoolean("additionalProperties", false);

            writer.WriteEndObject();
        }

        private static void WriteErrorResponseSchema(Utf8JsonWriter writer)
        {
            writer.WritePropertyName("ErrorResponse");
            writer.WriteStartObject();

            writer.WriteString("type", "object");

            writer.WritePropertyName("properties");
            writer.WriteStartObject();

            writer.WritePropertyName("error");
            writer.WriteStartObject();
            writer.WriteString("type", "string");
            writer.WriteBoolean("nullable", true);
            writer.WriteEndObject();

            writer.WriteEndObject(); // properties

            writer.WriteBoolean("additionalProperties", false);

            writer.WriteEndObject();
        }

        private static void WriteClassSchema(Utf8JsonWriter writer, ClassInfo classInfo)
        {
            writer.WritePropertyName(classInfo.Name);
            writer.WriteStartObject();

            writer.WriteString("type", "object");

            writer.WritePropertyName("properties");
            writer.WriteStartObject();

            foreach (var prop in classInfo.Properties)
            {
                writer.WritePropertyName(prop.Name);

                if (prop.IsArray)
                {
                    writer.WriteStartObject();
                    writer.WriteString("type", "array");

                    writer.WritePropertyName("items");
                    writer.WriteStartObject();
                    writer.WriteString("$ref", "#/components/schemas/" + prop.Type);
                    writer.WriteEndObject();

                    writer.WriteBoolean("nullable", true);
                    writer.WriteEndObject();
                }
                else
                {
                    WriteTypeDefinition(writer, prop.Type);
                }
            }

            writer.WriteEndObject(); // properties

            writer.WriteBoolean("additionalProperties", false);

            writer.WriteEndObject();
        }

        private static void WriteTypeDefinition(Utf8JsonWriter writer, string typeName)
        {
            writer.WriteStartObject();

            switch (typeName)
            {
                case "Integer":
                    writer.WriteString("type", "integer");
                    writer.WriteString("format", "int32");
                    break;
                case "Float":
                case "Double":
                    writer.WriteString("type", "number");
                    writer.WriteString("format", "double");
                    break;
                case "Boolean":
                    writer.WriteString("type", "boolean");
                    break;
                case "String":
                default:
                    writer.WriteString("type", "string");
                    writer.WriteBoolean("nullable", true);
                    break;
            }

            writer.WriteEndObject();
        }

        // Helper classes to store collected information
        private class FunctionInfo
        {
            public string Name { get; set; } = string.Empty;
            public string OutputType { get; set; } = string.Empty;
            public IVertex OutputTypeVertex { get; set; }
            public List<ParameterInfo> InputParameters { get; set; } = new List<ParameterInfo>();
        }

        private class ParameterInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public IVertex TypeVertex { get; set; }
        }

        private class ClassInfo
        {
            public string Name { get; set; } = string.Empty;
            public IVertex SourceVertex { get; set; }
            public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
        }

        private class PropertyInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public IVertex TypeVertex { get; set; }
            public bool IsArray { get; set; }
        }
    }
}
