using m0.Foundation;
using m0.Graph;
using m0.Lib.REST;
using m0.ZeroTypes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace m0.Network.Server
{
    public class OpenApiDocumentationGenerator
    {
        /// <summary>
        /// Generates OpenAPI 3.0.1 documentation from GVM vertex structure containing functions and classes.
        /// </summary>
        /// <param name="baseVertex">The starting vertex containing Function and Class definitions</param>
        /// <param name="urlPath">Base URL path for endpoints</param>
        /// <returns>OpenAPI JSON documentation string</returns>
        static public string GetOpenApiDocumentation(IVertex baseVertex, string urlPath)
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
                WritePaths(writer, functions, classes, urlPath);

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
                    // Only collect functions that have "Endpoint" meta edge
                    if (!GraphUtil.ExistQueryOut(edge.To, "Endpoint", null))
                    {
                        continue;
                    }

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
                            Name = GraphUtil.GetStringValue(paramEdge.To),
                            IsArray = false,
                            IsNullable = false
                        };

                        if (VertexOperations.IsMetaVertexOfManyMultiplicity(paramEdge.To))
                            paramInfo.IsArray = true;

                        if (VertexOperations.IsMetaVertexNullable(paramEdge.To))
                            paramInfo.IsNullable = true;

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
                    IsArray = false,
                    IsNullable = false
                };

                if (VertexOperations.IsMetaVertexOfManyMultiplicity(attrEdge.To))                
                    propInfo.IsArray = true;
                
                if (VertexOperations.IsMetaVertexNullable(attrEdge.To))
                    propInfo.IsNullable = true;

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
                    IsArray = false,
                    IsNullable = false
                };

                if (VertexOperations.IsMetaVertexOfManyMultiplicity(aggEdge.To))
                    propInfo.IsArray = true;

                if (VertexOperations.IsMetaVertexNullable(aggEdge.To))
                    propInfo.IsNullable = true;

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
                    IsArray = false,
                    IsNullable = false
                };

                if (VertexOperations.IsMetaVertexOfManyMultiplicity(assocEdge.To))
                    propInfo.IsArray = true;

                if (VertexOperations.IsMetaVertexNullable(assocEdge.To))
                    propInfo.IsNullable = true;

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
                if (TypeConverter.IsPrimitiveType(typeName))
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
                        if (prop.TypeVertex != null && !TypeConverter.IsPrimitiveType(prop.Type) && !collectedNames.Contains(prop.Type))
                        {
                            verticesToProcess.Enqueue(prop.TypeVertex);
                        }
                    }
                }
            }
        }

        private static void WritePaths(Utf8JsonWriter writer, List<FunctionInfo> functions, List<ClassInfo> classes, string urlPath)
        {
            writer.WritePropertyName("paths");
            writer.WriteStartObject();

            // Write function endpoints
            foreach (var function in functions)
            {
                WritePathForFunction(writer, function, classes, urlPath);
            }

            // Write root endpoint
            WriteRootEndpoint(writer, urlPath);

            writer.WriteEndObject();
        }

        private static void WritePathForFunction(Utf8JsonWriter writer, FunctionInfo function, List<ClassInfo> classes, string urlPath)
        {
            string basePath = NormalizeBasePath(urlPath);
            string fullPath = basePath == "" ? "/" + function.Name : basePath + "/" + function.Name;
            writer.WritePropertyName(fullPath);
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
            // Always generate a request wrapper with parameter name
            return "#/components/schemas/" + function.Name + "Request";
        }

        private static string GetResponseSchemaRef(FunctionInfo function, List<ClassInfo> classes)
        {
            // If output is a class type, use that class directly
            if (!string.IsNullOrEmpty(function.OutputType) && 
                !TypeConverter.IsPrimitiveType(function.OutputType) && 
                classes.Any(c => c.Name == function.OutputType))
            {
                return "#/components/schemas/" + function.OutputType;
            }

            // Otherwise, generate a response wrapper
            return "#/components/schemas/" + function.Name + "Response";
        }

        private static void WriteRootEndpoint(Utf8JsonWriter writer, string urlPath)
        {
            string basePath = NormalizeBasePath(urlPath);
            string fullPath = basePath == "" ? "/" : basePath + "/";
            writer.WritePropertyName(fullPath);
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
            // Always need request wrapper with parameter name
            return true;
        }

        private static bool NeedsResponseWrapper(FunctionInfo function, List<ClassInfo> classes)
        {
            // Needs wrapper if output is primitive or not a known class
            if (string.IsNullOrEmpty(function.OutputType))
                return true;

            return TypeConverter.IsPrimitiveType(function.OutputType) || !classes.Any(c => c.Name == function.OutputType);
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

                if (param.IsArray)
                {
                    writer.WriteStartObject();
                    writer.WriteString("type", "array");

                    writer.WritePropertyName("items");
                    if (TypeConverter.IsPrimitiveType(param.Type))
                    {
                        TypeConverter.WriteOpenApiTypeDefinition(writer, param.Type, false);
                    }
                    else
                    {
                        writer.WriteStartObject();
                        writer.WriteString("$ref", "#/components/schemas/" + param.Type);
                        writer.WriteEndObject();
                    }

                    if (param.IsNullable)
                        writer.WriteBoolean("nullable", true);
                    writer.WriteEndObject();
                }
                else
                {
                    // For non-array parameters
                    if (TypeConverter.IsPrimitiveType(param.Type))
                    {
                        TypeConverter.WriteOpenApiTypeDefinition(writer, param.Type, param.IsNullable);
                    }
                    else
                    {
                        // Class type - use $ref
                        writer.WriteStartObject();
                        writer.WriteString("$ref", "#/components/schemas/" + param.Type);
                        writer.WriteEndObject();
                    }
                }
            }

            writer.WriteEndObject(); // properties

            writer.WriteBoolean("additionalProperties", false);

            writer.WriteEndObject();
        }

        private static void WriteResponseSchema(Utf8JsonWriter writer, FunctionInfo function)
        {
            writer.WritePropertyName(function.Name + "Response");
            writer.WriteStartObject();

            // For primitive/atomic types, return as array directly: ["value"]
            if (TypeConverter.IsPrimitiveType(function.OutputType))
            {
                writer.WriteString("type", "array");
                writer.WritePropertyName("items");
                TypeConverter.WriteOpenApiTypeDefinition(writer, function.OutputType);
            }
            else
            {
                // For complex types, wrap in object with "result" property
                writer.WriteString("type", "object");

                writer.WritePropertyName("properties");
                writer.WriteStartObject();

                writer.WritePropertyName("result");
                TypeConverter.WriteOpenApiTypeDefinition(writer, function.OutputType);

                writer.WriteEndObject(); // properties

                writer.WriteBoolean("additionalProperties", false);
            }

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
                    if (TypeConverter.IsPrimitiveType(prop.Type))
                    {
                        TypeConverter.WriteOpenApiTypeDefinition(writer, prop.Type, false);
                    }
                    else
                    {
                        writer.WriteStartObject();
                        writer.WriteString("$ref", "#/components/schemas/" + prop.Type);
                        writer.WriteEndObject();
                    }

                    if (prop.IsNullable)
                        writer.WriteBoolean("nullable", true);
                    writer.WriteEndObject();
                }
                else
                {
                    if (TypeConverter.IsPrimitiveType(prop.Type))
                    {
                        TypeConverter.WriteOpenApiTypeDefinition(writer, prop.Type, prop.IsNullable);
                    }
                    else
                    {
                        writer.WriteStartObject();
                        writer.WriteString("$ref", "#/components/schemas/" + prop.Type);
                        writer.WriteEndObject();
                    }
                }
            }

            writer.WriteEndObject(); // properties

            writer.WriteBoolean("additionalProperties", false);

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
            public bool IsArray { get; set; }
            public bool IsNullable { get; set; }
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
            public bool IsNullable { get; set; }
        }

        private static string NormalizeBasePath(string urlPath)
        {
            if (string.IsNullOrWhiteSpace(urlPath) || urlPath == "/")
                return "";

            string normalized = urlPath.Trim();
            if (!normalized.StartsWith("/"))
                normalized = "/" + normalized;

            if (normalized.EndsWith("/"))
                normalized = normalized.TrimEnd('/');

            return normalized;
        }
    }
}
