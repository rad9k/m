using m0.Foundation;
using m0.Graph;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace m0.Lib.StdView
{
    public class OpenApiUrlVertexToPackage
    {
        static IVertex root = MinusZero.Instance.Root;

        static IVertex RemoteEndpointUrl_meta = root.Get(false, @"System\Lib\Net\Rest\RemoteEndpointUrl");
        static IVertex RemoteEndpointParameters_meta = root.Get(false, @"System\Lib\Net\Rest\RemoteEndpointParameters");
        static IVertex Function_meta = root.Get(false, @"System\Meta\ZeroUML\Function");
        static IVertex InputParameter_meta = root.Get(false, @"System\Meta\ZeroUML\Function\InputParameter");
        static IVertex Output_meta = root.Get(false, @"System\Meta\ZeroUML\Function\Output");
        static IVertex EdgeTarget_meta = root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget");
        static IVertex MinCardinality_meta = root.Get(false, @"System\Meta\Base\Vertex\$MinCardinality");
        static IVertex MaxCardinality_meta = root.Get(false, @"System\Meta\Base\Vertex\$MaxCardinality");

        static IVertex ZeroTypesRoot = root.Get(false, @"System\Meta\ZeroTypes");

        private static readonly HttpClient httpClient = new HttpClient();

        public static INoInEdgeInOutVertexVertex OpenApiUrlVertexToPackage_Transform(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex from = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex to = GraphUtil.GetQueryOutFirst(stack, "to", null);

            string OpenApiUrl = GraphUtil.GetStringValueOrNull(from);

            OpenApiUrlVertexToPackage_Process(OpenApiUrl, to);

            return null;
        }

        private static void OpenApiUrlVertexToPackage_Process(string openApiUrl, IVertex to)
        {
            if (to == null || string.IsNullOrWhiteSpace(openApiUrl))
                return;

            // Fetch OpenAPI specification from URL
            string openApiJson = FetchOpenApiSpec(openApiUrl);
            if (string.IsNullOrEmpty(openApiJson))
                return;

            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(openApiJson);
            }
            catch (JsonException)
            {
                return;
            }

            // Determine base URL from openApiUrl (strip the path to openapi.json)
            string baseUrl = GetBaseUrl(openApiUrl, document.RootElement);

            // Determine where to create new classes
            IVertex newClassDefinitionsVertex = GraphUtil.GetQueryOutFirst(to, "$NewClassDefinitions", null);
            IVertex newClassesRoot = newClassDefinitionsVertex ?? to;

            // Collect all vertices where to look for existing classes
            var existingClassesRoots = new List<IVertex>();
            existingClassesRoots.Add(newClassesRoot);

            IList<IEdge> existingClassDefinitionsEdges = GraphUtil.GetQueryOut(to, "$ExistingClassDefinitions", null);
            foreach (IEdge edge in existingClassDefinitionsEdges)
            {
                if (edge.To != null && !existingClassesRoots.Contains(edge.To))
                    existingClassesRoots.Add(edge.To);
            }

            // Create context for schema processing
            var context = new OpenApiContext(newClassesRoot, existingClassesRoots, document.RootElement);

            // Detect OpenAPI version and process schemas
            context.ProcessSchemas();

            // Process paths and create functions
            ProcessPaths(document.RootElement, to, baseUrl, context);
        }

        private static string FetchOpenApiSpec(string url)
        {
            try
            {
                var task = httpClient.GetStringAsync(url);
                task.Wait();
                return task.Result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetBaseUrl(string openApiUrl, JsonElement rootElement)
        {
            // Try to get base URL from OpenAPI spec first
            if (rootElement.TryGetProperty("servers", out JsonElement servers) && servers.GetArrayLength() > 0)
            {
                // OpenAPI 3.x
                var firstServer = servers.EnumerateArray().First();
                if (firstServer.TryGetProperty("url", out JsonElement serverUrl))
                {
                    return serverUrl.GetString();
                }
            }
            else if (rootElement.TryGetProperty("host", out JsonElement host))
            {
                // OpenAPI 2.0 (Swagger)
                string scheme = "https";
                if (rootElement.TryGetProperty("schemes", out JsonElement schemes) && schemes.GetArrayLength() > 0)
                {
                    scheme = schemes.EnumerateArray().First().GetString();
                }
                string basePath = "";
                if (rootElement.TryGetProperty("basePath", out JsonElement basePathElement))
                {
                    basePath = basePathElement.GetString();
                }
                return $"{scheme}://{host.GetString()}{basePath}";
            }

            // Fallback: derive from openApiUrl
            Uri uri = new Uri(openApiUrl);
            return $"{uri.Scheme}://{uri.Host}{(uri.Port != 80 && uri.Port != 443 ? ":" + uri.Port : "")}";
        }

        private static void ProcessPaths(JsonElement rootElement, IVertex to, string baseUrl, OpenApiContext context)
        {
            if (!rootElement.TryGetProperty("paths", out JsonElement paths))
                return;

            foreach (JsonProperty pathProperty in paths.EnumerateObject())
            {
                string path = pathProperty.Name;

                foreach (JsonProperty methodProperty in pathProperty.Value.EnumerateObject())
                {
                    string method = methodProperty.Name.ToUpperInvariant();

                    // Skip non-HTTP method properties like "parameters", "servers", etc.
                    if (!IsHttpMethod(method))
                        continue;

                    JsonElement operation = methodProperty.Value;

                    // Create function for this endpoint
                    CreateFunctionForEndpoint(to, baseUrl, path, method, operation, context);
                }
            }
        }

        private static bool IsHttpMethod(string method)
        {
            return method == "GET" || method == "POST" || method == "PUT" ||
                   method == "DELETE" || method == "PATCH" || method == "HEAD" ||
                   method == "OPTIONS" || method == "TRACE";
        }

        private static void CreateFunctionForEndpoint(IVertex to, string baseUrl, string path, string method,
            JsonElement operation, OpenApiContext context)
        {
            // Get function name
            string functionName = GetFunctionName(operation, method, path);

            // Create function vertex
            IVertex functionVertex = to.AddVertex(Function_meta, functionName);
            functionVertex.AddEdge(MinusZero.Instance.Is, Function_meta);

            // Add input parameters
            AddInputParameters(functionVertex, operation, context);

            // Add output
            AddOutput(functionVertex, operation, context);

            // Add RemoteEndpointUrl
            string fullEndpointUrl = baseUrl.TrimEnd('/') + path;
            functionVertex.AddVertex(RemoteEndpointUrl_meta, fullEndpointUrl);

            // Add RemoteEndpointParameters (JSON with operation details)
            string endpointParametersJson = CreateEndpointParametersJson(method, path, operation);
            functionVertex.AddVertex(RemoteEndpointParameters_meta, endpointParametersJson);
        }

        private static string GetFunctionName(JsonElement operation, string method, string path)
        {
            // Prefer operationId if available
            if (operation.TryGetProperty("operationId", out JsonElement operationId))
            {
                return operationId.GetString();
            }

            // Generate name from method and path
            string sanitizedPath = path
                .Replace("/", "_")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("-", "_")
                .Trim('_');

            return $"{method.ToLowerInvariant()}_{sanitizedPath}".Trim('_');
        }

        private static void AddInputParameters(IVertex functionVertex, JsonElement operation, OpenApiContext context)
        {
            // Process parameters from operation
            if (operation.TryGetProperty("parameters", out JsonElement parameters))
            {
                foreach (JsonElement parameter in parameters.EnumerateArray())
                {
                    AddParameterFromOpenApi(functionVertex, parameter, context);
                }
            }

            // Process requestBody (OpenAPI 3.x)
            if (operation.TryGetProperty("requestBody", out JsonElement requestBody))
            {
                AddRequestBodyParameter(functionVertex, requestBody, context);
            }
        }

        private static void AddParameterFromOpenApi(IVertex functionVertex, JsonElement parameter, OpenApiContext context)
        {
            string parameterName = "";
            if (parameter.TryGetProperty("name", out JsonElement nameElement))
            {
                parameterName = nameElement.GetString();
            }

            if (string.IsNullOrEmpty(parameterName))
                return;

            // Check if this is a reference
            if (parameter.TryGetProperty("$ref", out JsonElement refElement))
            {
                parameter = context.ResolveReference(refElement.GetString());
                if (parameter.ValueKind == JsonValueKind.Undefined)
                    return;

                if (parameter.TryGetProperty("name", out nameElement))
                {
                    parameterName = nameElement.GetString();
                }
            }

            // Get parameter type
            IVertex parameterType = GetParameterType(parameter, context);

            // Check if it's an array
            bool isArray = false;
            if (parameter.TryGetProperty("schema", out JsonElement schema))
            {
                if (schema.TryGetProperty("type", out JsonElement typeElement) && typeElement.GetString() == "array")
                {
                    isArray = true;
                }
            }

            // Create input parameter vertex
            IVertex inputParameterVertex = functionVertex.AddVertex(InputParameter_meta, parameterName);
            inputParameterVertex.AddEdge(MinusZero.Instance.Is, InputParameter_meta);
            inputParameterVertex.AddEdge(EdgeTarget_meta, parameterType);

            // Set cardinality
            inputParameterVertex.AddVertex(MinCardinality_meta, IsParameterRequired(parameter) ? 1 : 0);
            inputParameterVertex.AddVertex(MaxCardinality_meta, isArray ? -1 : 1);
        }

        private static void AddRequestBodyParameter(IVertex functionVertex, JsonElement requestBody, OpenApiContext context)
        {
            // Check if this is a reference
            if (requestBody.TryGetProperty("$ref", out JsonElement refElement))
            {
                requestBody = context.ResolveReference(refElement.GetString());
                if (requestBody.ValueKind == JsonValueKind.Undefined)
                    return;
            }

            // Get the schema from content (typically application/json)
            JsonElement schema = default;
            if (requestBody.TryGetProperty("content", out JsonElement content))
            {
                if (content.TryGetProperty("application/json", out JsonElement jsonContent))
                {
                    if (jsonContent.TryGetProperty("schema", out JsonElement schemaElement))
                    {
                        schema = schemaElement;
                    }
                }
                else
                {
                    // Try first available content type
                    foreach (JsonProperty contentType in content.EnumerateObject())
                    {
                        if (contentType.Value.TryGetProperty("schema", out JsonElement schemaElement))
                        {
                            schema = schemaElement;
                            break;
                        }
                    }
                }
            }

            if (schema.ValueKind == JsonValueKind.Undefined)
                return;

            IVertex parameterType = GetTypeFromSchema(schema, context, "RequestBody");

            // Create input parameter for body
            IVertex inputParameterVertex = functionVertex.AddVertex(InputParameter_meta, "body");
            inputParameterVertex.AddEdge(MinusZero.Instance.Is, InputParameter_meta);
            inputParameterVertex.AddEdge(EdgeTarget_meta, parameterType);

            bool isRequired = false;
            if (requestBody.TryGetProperty("required", out JsonElement requiredElement))
            {
                isRequired = requiredElement.GetBoolean();
            }

            bool isArray = schema.TryGetProperty("type", out JsonElement typeElement) && typeElement.GetString() == "array";

            inputParameterVertex.AddVertex(MinCardinality_meta, isRequired ? 1 : 0);
            inputParameterVertex.AddVertex(MaxCardinality_meta, isArray ? -1 : 1);
        }

        private static IVertex GetParameterType(JsonElement parameter, OpenApiContext context)
        {
            // OpenAPI 3.x uses "schema" for type definition
            if (parameter.TryGetProperty("schema", out JsonElement schema))
            {
                return GetTypeFromSchema(schema, context, "Parameter");
            }

            // OpenAPI 2.0 - type is directly in parameter or in "items" for body
            if (parameter.TryGetProperty("in", out JsonElement inElement) && inElement.GetString() == "body")
            {
                if (parameter.TryGetProperty("schema", out JsonElement bodySchema))
                {
                    return GetTypeFromSchema(bodySchema, context, "BodyParameter");
                }
            }

            // Simple type in OpenAPI 2.0
            if (parameter.TryGetProperty("type", out JsonElement typeElement))
            {
                return GetZeroTypeFromOpenApiType(typeElement.GetString(), parameter);
            }

            return GetZeroType("String");
        }

        private static IVertex GetTypeFromSchema(JsonElement schema, OpenApiContext context, string fallbackName)
        {
            // Check for $ref
            if (schema.TryGetProperty("$ref", out JsonElement refElement))
            {
                string refPath = refElement.GetString();
                return context.GetOrCreateClassFromRef(refPath);
            }

            // Check type
            if (schema.TryGetProperty("type", out JsonElement typeElement))
            {
                string type = typeElement.GetString();

                if (type == "array")
                {
                    // Get items type
                    if (schema.TryGetProperty("items", out JsonElement items))
                    {
                        return GetTypeFromSchema(items, context, fallbackName + "Item");
                    }
                    return GetZeroType("String");
                }

                if (type == "object")
                {
                    // Create inline class if it has properties
                    if (schema.TryGetProperty("properties", out JsonElement properties))
                    {
                        return context.CreateInlineClass(fallbackName, schema);
                    }
                    return GetZeroType("String"); // Generic object
                }

                return GetZeroTypeFromOpenApiType(type, schema);
            }

            // If no type but has $ref in allOf/oneOf/anyOf
            if (schema.TryGetProperty("allOf", out JsonElement allOf))
            {
                foreach (JsonElement item in allOf.EnumerateArray())
                {
                    if (item.TryGetProperty("$ref", out JsonElement itemRef))
                    {
                        return context.GetOrCreateClassFromRef(itemRef.GetString());
                    }
                }
            }

            return GetZeroType("String");
        }

        private static IVertex GetZeroTypeFromOpenApiType(string openApiType, JsonElement schema)
        {
            string format = "";
            if (schema.TryGetProperty("format", out JsonElement formatElement))
            {
                format = formatElement.GetString();
            }

            switch (openApiType)
            {
                case "integer":
                    return GetZeroType("Integer");
                case "number":
                    return GetZeroType("Double");
                case "boolean":
                    return GetZeroType("Boolean");
                case "string":
                    if (format == "date" || format == "date-time")
                        return GetZeroType("String"); // Could use DateTime type if available
                    return GetZeroType("String");
                default:
                    return GetZeroType("String");
            }
        }

        private static IVertex GetZeroType(string typeName)
        {
            if (ZeroTypesRoot == null)
                return null;

            return ZeroTypesRoot.Get(false, typeName) ?? ZeroTypesRoot.Get(false, "String");
        }

        private static bool IsParameterRequired(JsonElement parameter)
        {
            if (parameter.TryGetProperty("required", out JsonElement required))
            {
                return required.GetBoolean();
            }

            // Path parameters are always required
            if (parameter.TryGetProperty("in", out JsonElement inElement))
            {
                return inElement.GetString() == "path";
            }

            return false;
        }

        private static void AddOutput(IVertex functionVertex, JsonElement operation, OpenApiContext context)
        {
            IVertex outputType = GetOutputType(operation, context);

            IVertex outputVertex = functionVertex.AddVertex(Output_meta, "");
            outputVertex.AddEdge(MinusZero.Instance.Is, Output_meta);
            outputVertex.AddEdge(EdgeTarget_meta, outputType);
        }

        private static IVertex GetOutputType(JsonElement operation, OpenApiContext context)
        {
            if (!operation.TryGetProperty("responses", out JsonElement responses))
                return GetZeroType("String");

            // Try to get 200, 201, or 2xx response
            JsonElement successResponse = default;
            if (responses.TryGetProperty("200", out successResponse) ||
                responses.TryGetProperty("201", out successResponse) ||
                responses.TryGetProperty("2XX", out successResponse) ||
                responses.TryGetProperty("default", out successResponse))
            {
                // Resolved
            }
            else
            {
                // Get first response
                foreach (JsonProperty response in responses.EnumerateObject())
                {
                    if (response.Name.StartsWith("2"))
                    {
                        successResponse = response.Value;
                        break;
                    }
                }
            }

            if (successResponse.ValueKind == JsonValueKind.Undefined)
                return GetZeroType("String");

            // Check if this is a reference
            if (successResponse.TryGetProperty("$ref", out JsonElement refElement))
            {
                successResponse = context.ResolveReference(refElement.GetString());
                if (successResponse.ValueKind == JsonValueKind.Undefined)
                    return GetZeroType("String");
            }

            // OpenAPI 3.x - content
            if (successResponse.TryGetProperty("content", out JsonElement content))
            {
                JsonElement schema = default;
                if (content.TryGetProperty("application/json", out JsonElement jsonContent))
                {
                    if (jsonContent.TryGetProperty("schema", out JsonElement schemaElement))
                    {
                        schema = schemaElement;
                    }
                }
                else
                {
                    // Try first available content type
                    foreach (JsonProperty contentType in content.EnumerateObject())
                    {
                        if (contentType.Value.TryGetProperty("schema", out JsonElement schemaElement))
                        {
                            schema = schemaElement;
                            break;
                        }
                    }
                }

                if (schema.ValueKind != JsonValueKind.Undefined)
                {
                    return GetTypeFromSchema(schema, context, "Response");
                }
            }

            // OpenAPI 2.0 - schema directly in response
            if (successResponse.TryGetProperty("schema", out JsonElement responseSchema))
            {
                return GetTypeFromSchema(responseSchema, context, "Response");
            }

            return GetZeroType("String");
        }

        private static string CreateEndpointParametersJson(string method, string path, JsonElement operation)
        {
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false }))
            {
                writer.WriteStartObject();

                writer.WriteString("method", method);
                writer.WriteString("path", path);

                // Write parameters if present
                if (operation.TryGetProperty("parameters", out JsonElement parameters))
                {
                    writer.WritePropertyName("parameters");
                    parameters.WriteTo(writer);
                }

                // Write requestBody if present (OpenAPI 3.x)
                if (operation.TryGetProperty("requestBody", out JsonElement requestBody))
                {
                    writer.WritePropertyName("requestBody");
                    requestBody.WriteTo(writer);
                }

                // Write responses
                if (operation.TryGetProperty("responses", out JsonElement responses))
                {
                    writer.WritePropertyName("responses");
                    responses.WriteTo(writer);
                }

                // Write security if present
                if (operation.TryGetProperty("security", out JsonElement security))
                {
                    writer.WritePropertyName("security");
                    security.WriteTo(writer);
                }

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(buffer.WrittenSpan);
        }

        /// <summary>
        /// Context for processing OpenAPI schemas and creating GVM classes.
        /// </summary>
        private class OpenApiContext
        {
            private readonly IVertex newClassesRoot;
            private readonly IList<IVertex> existingClassesRoots;
            private readonly JsonElement rootElement;
            private readonly IDictionary<string, IVertex> classesByRef = new Dictionary<string, IVertex>();
            private readonly IDictionary<string, int> classNameCounts = new Dictionary<string, int>();
            private readonly bool isOpenApi3;

            public OpenApiContext(IVertex newClassesRoot, IList<IVertex> existingClassesRoots, JsonElement rootElement)
            {
                this.newClassesRoot = newClassesRoot;
                this.existingClassesRoots = existingClassesRoots;
                this.rootElement = rootElement;

                // Detect version
                isOpenApi3 = rootElement.TryGetProperty("openapi", out _);
            }

            public void ProcessSchemas()
            {
                JsonElement schemas = default;

                if (isOpenApi3)
                {
                    // OpenAPI 3.x: components/schemas
                    if (rootElement.TryGetProperty("components", out JsonElement components))
                    {
                        if (components.TryGetProperty("schemas", out JsonElement schemasElement))
                        {
                            schemas = schemasElement;
                        }
                    }
                }
                else
                {
                    // OpenAPI 2.0 (Swagger): definitions
                    if (rootElement.TryGetProperty("definitions", out JsonElement definitions))
                    {
                        schemas = definitions;
                    }
                }

                if (schemas.ValueKind != JsonValueKind.Object)
                    return;

                foreach (JsonProperty schemaProperty in schemas.EnumerateObject())
                {
                    string schemaName = schemaProperty.Name;
                    string refPath = isOpenApi3
                        ? $"#/components/schemas/{schemaName}"
                        : $"#/definitions/{schemaName}";

                    // Create class if not exists
                    GetOrCreateClassFromRef(refPath);
                }
            }

            public IVertex GetOrCreateClassFromRef(string refPath)
            {
                if (string.IsNullOrEmpty(refPath))
                    return GetZeroType("String");

                // Check cache
                if (classesByRef.TryGetValue(refPath, out IVertex existingClass))
                {
                    return existingClass;
                }

                // Extract schema name from ref path
                string schemaName = ExtractSchemaNameFromRef(refPath);

                // Check existing classes in all roots
                IVertex existingClassVertex = FindExistingClass(schemaName);
                if (existingClassVertex != null)
                {
                    classesByRef[refPath] = existingClassVertex;
                    return existingClassVertex;
                }

                // Resolve the reference to get schema definition
                JsonElement schema = ResolveReference(refPath);
                if (schema.ValueKind == JsonValueKind.Undefined)
                {
                    return GetZeroType("String");
                }

                // Create new class
                IVertex classVertex = CreateClassFromSchema(schemaName, schema);
                classesByRef[refPath] = classVertex;

                return classVertex;
            }

            public IVertex CreateInlineClass(string suggestedName, JsonElement schema)
            {
                string className = CreateUniqueClassName(suggestedName);
                return CreateClassFromSchema(className, schema);
            }

            private IVertex CreateClassFromSchema(string className, JsonElement schema)
            {
                // Check if it's actually a simple type
                if (schema.TryGetProperty("type", out JsonElement typeElement))
                {
                    string type = typeElement.GetString();
                    if (type != "object" && type != "array")
                    {
                        return GetZeroTypeFromOpenApiType(type, schema);
                    }
                }

                // Create the class
                IVertex classVertex = GraphUtil.AddClass(newClassesRoot, className);

                // Process properties
                if (schema.TryGetProperty("properties", out JsonElement properties))
                {
                    // Get required properties list
                    HashSet<string> requiredProps = new HashSet<string>();
                    if (schema.TryGetProperty("required", out JsonElement required))
                    {
                        foreach (JsonElement reqProp in required.EnumerateArray())
                        {
                            requiredProps.Add(reqProp.GetString());
                        }
                    }

                    foreach (JsonProperty property in properties.EnumerateObject())
                    {
                        string propertyName = property.Name;
                        JsonElement propertySchema = property.Value;

                        AddPropertyToClass(classVertex, propertyName, propertySchema, requiredProps.Contains(propertyName));
                    }
                }

                // Handle allOf for inheritance/composition
                if (schema.TryGetProperty("allOf", out JsonElement allOf))
                {
                    foreach (JsonElement item in allOf.EnumerateArray())
                    {
                        if (item.TryGetProperty("properties", out JsonElement itemProperties))
                        {
                            HashSet<string> requiredProps = new HashSet<string>();
                            if (item.TryGetProperty("required", out JsonElement required))
                            {
                                foreach (JsonElement reqProp in required.EnumerateArray())
                                {
                                    requiredProps.Add(reqProp.GetString());
                                }
                            }

                            foreach (JsonProperty property in itemProperties.EnumerateObject())
                            {
                                string propertyName = property.Name;
                                JsonElement propertySchema = property.Value;
                                AddPropertyToClass(classVertex, propertyName, propertySchema, requiredProps.Contains(propertyName));
                            }
                        }
                    }
                }

                return classVertex;
            }

            private void AddPropertyToClass(IVertex classVertex, string propertyName, JsonElement propertySchema, bool isRequired)
            {
                bool isArray = false;
                IVertex propertyType;
                JsonElement actualSchema = propertySchema;

                // Check for $ref
                if (propertySchema.TryGetProperty("$ref", out JsonElement refElement))
                {
                    propertyType = GetOrCreateClassFromRef(refElement.GetString());
                }
                else if (propertySchema.TryGetProperty("type", out JsonElement typeElement))
                {
                    string type = typeElement.GetString();

                    if (type == "array")
                    {
                        isArray = true;
                        if (propertySchema.TryGetProperty("items", out JsonElement items))
                        {
                            if (items.TryGetProperty("$ref", out JsonElement itemRef))
                            {
                                propertyType = GetOrCreateClassFromRef(itemRef.GetString());
                            }
                            else
                            {
                                propertyType = GetTypeFromSchemaInternal(items, propertyName + "Item");
                            }
                        }
                        else
                        {
                            propertyType = GetZeroType("String");
                        }
                    }
                    else if (type == "object")
                    {
                        if (propertySchema.TryGetProperty("properties", out _))
                        {
                            propertyType = CreateInlineClass(propertyName, propertySchema);
                        }
                        else
                        {
                            propertyType = GetZeroType("String");
                        }
                    }
                    else
                    {
                        propertyType = GetZeroTypeFromOpenApiType(type, propertySchema);
                    }
                }
                else if (propertySchema.TryGetProperty("allOf", out JsonElement allOf))
                {
                    // Try to find $ref in allOf
                    propertyType = GetZeroType("String");
                    foreach (JsonElement item in allOf.EnumerateArray())
                    {
                        if (item.TryGetProperty("$ref", out JsonElement itemRef))
                        {
                            propertyType = GetOrCreateClassFromRef(itemRef.GetString());
                            break;
                        }
                    }
                }
                else
                {
                    propertyType = GetZeroType("String");
                }

                // Determine if it's a primitive or complex type
                bool isPrimitive = IsPrimitiveType(propertyType);

                int minCardinality = isRequired ? 1 : 0;
                int maxCardinality = isArray ? -1 : 1;

                if (isPrimitive)
                {
                    GraphUtil.AddAttribute(classVertex, propertyName, propertyType, minCardinality, maxCardinality);
                }
                else
                {
                    GraphUtil.AddAssociation(classVertex, propertyName, propertyType, minCardinality, maxCardinality);
                }
            }

            private IVertex GetTypeFromSchemaInternal(JsonElement schema, string fallbackName)
            {
                if (schema.TryGetProperty("$ref", out JsonElement refElement))
                {
                    return GetOrCreateClassFromRef(refElement.GetString());
                }

                if (schema.TryGetProperty("type", out JsonElement typeElement))
                {
                    string type = typeElement.GetString();

                    if (type == "object" && schema.TryGetProperty("properties", out _))
                    {
                        return CreateInlineClass(fallbackName, schema);
                    }

                    return GetZeroTypeFromOpenApiType(type, schema);
                }

                return GetZeroType("String");
            }

            private bool IsPrimitiveType(IVertex typeVertex)
            {
                if (typeVertex == null)
                    return true;

                string typeName = GraphUtil.GetStringValue(typeVertex);
                return typeName == "String" || typeName == "Integer" || typeName == "Double" ||
                       typeName == "Boolean" || typeName == "Float" || typeName == "Decimal";
            }

            public JsonElement ResolveReference(string refPath)
            {
                if (string.IsNullOrEmpty(refPath) || !refPath.StartsWith("#/"))
                    return default;

                string[] parts = refPath.Substring(2).Split('/');
                JsonElement current = rootElement;

                foreach (string part in parts)
                {
                    string decodedPart = Uri.UnescapeDataString(part.Replace("~1", "/").Replace("~0", "~"));
                    if (!current.TryGetProperty(decodedPart, out current))
                    {
                        return default;
                    }
                }

                return current;
            }

            private string ExtractSchemaNameFromRef(string refPath)
            {
                // #/components/schemas/User -> User
                // #/definitions/User -> User
                int lastSlash = refPath.LastIndexOf('/');
                if (lastSlash >= 0 && lastSlash < refPath.Length - 1)
                {
                    return refPath.Substring(lastSlash + 1);
                }
                return refPath;
            }

            private IVertex FindExistingClass(string className)
            {
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

            private string CreateUniqueClassName(string classNameHint)
            {
                if (!classNameCounts.TryGetValue(classNameHint, out int count))
                {
                    classNameCounts[classNameHint] = 1;
                    return classNameHint;
                }

                count++;
                classNameCounts[classNameHint] = count;
                return classNameHint + count.ToString();
            }
        }
    }
}
