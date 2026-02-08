using m0.Foundation;
using m0.Graph;
using m0.Lib.StdView;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;

namespace m0.Lib.REST
{
    public class RemoteServer
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);
        private static readonly object logLock = new object();
        private static readonly string logFilePath = Path.Combine(Environment.CurrentDirectory, "remoterest.log");

        private static void Log(string message, params object[] args)
        {
            try
            {
                string formatted = (args != null && args.Length > 0)
                    ? string.Format(CultureInfo.InvariantCulture, message, args)
                    : message;

                string line = string.Format(CultureInfo.InvariantCulture,
                    "[{0:yyyy-MM-dd HH:mm:ss.fff}] {1}{2}",
                    System.DateTime.Now,
                    formatted,
                    Environment.NewLine);

                lock (logLock)
                {
                    File.AppendAllText(logFilePath, line, Utf8NoBom);
                }
            }
            catch
            {
                // Never fail the remote call due to logging problems.
            }
        }

        public static INoInEdgeInOutVertexVertex CallRemoteRestServer(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex target = VertexOperations.GetTargetFromStackTop(stack);

            IVertex RemoteEndpointPathVertex = GraphUtil.GetQueryOutFirst(target, "RemoteEndpointPath", null);
            IVertex RemoteEndpointParametersVertex = GraphUtil.GetQueryOutFirst(target, "RemoteEndpointParameters", null);

            IVertex PackageVertex = GraphUtil.GetQueryInFirst(target, "Function", null);

            IVertex RemoteRestServerUrlVertex = GraphUtil.GetQueryOutFirst(PackageVertex, "RemoteRestServerUrl", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            string serverUrl = GraphUtil.GetStringValueOrNull(RemoteRestServerUrlVertex);
            string endpointPath = GraphUtil.GetStringValueOrNull(RemoteEndpointPathVertex);
            string parametersJson = GraphUtil.GetStringValueOrNull(RemoteEndpointParametersVertex);

            if (string.IsNullOrEmpty(serverUrl) || string.IsNullOrEmpty(endpointPath))
            {
                Log("[RemoteServer] ERROR: Missing serverUrl or endpointPath. serverUrl={0}, endpointPath={1}",
                    serverUrl ?? "(null)", endpointPath ?? "(null)");
                return newStack;
            }

            // Parse endpoint parameters definition
            string httpMethod = "GET";
            string resolvedPath = endpointPath;
            JsonElement inputParametersArray = default;
            string responseUnwrapProperty = null;

            if (!string.IsNullOrEmpty(parametersJson))
            {
                try
                {
                    JsonDocument paramsDoc = JsonDocument.Parse(parametersJson);
                    JsonElement paramsRoot = paramsDoc.RootElement;

                    if (paramsRoot.TryGetProperty("method", out JsonElement methodElement))
                        httpMethod = methodElement.GetString();

                    if (paramsRoot.TryGetProperty("inputParameters", out JsonElement paramsEl))
                        inputParametersArray = paramsEl.Clone();

                    if (paramsRoot.TryGetProperty("responseUnwrapProperty", out JsonElement unwrapEl))
                        responseUnwrapProperty = unwrapEl.GetString();
                }
                catch (JsonException)
                {
                    // Proceed with defaults
                }
            }

            // Classify all parameters by their HTTP location
            var pathParams = new Dictionary<string, string>();
            var queryParams = new List<KeyValuePair<string, string>>();
            var headerParams = new Dictionary<string, string>();
            var bodyParams = new List<KeyValuePair<string, IVertex>>();

            if (inputParametersArray.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement paramDef in inputParametersArray.EnumerateArray())
                {
                    if (!paramDef.TryGetProperty("name", out JsonElement nameEl) ||
                        !paramDef.TryGetProperty("in", out JsonElement inEl))
                        continue;

                    string paramName = nameEl.GetString();
                    string paramIn = inEl.GetString();

                    // Get parameter value from GVM stack
                    IVertex paramVertex = GraphUtil.GetQueryOutFirst(stack, paramName, null);
                    if (paramVertex == null)
                    {
                        Log("[RemoteServer] WARNING: Parameter '{0}' (in:{1}) not found on stack", paramName, paramIn);
                        continue;
                    }

                    Log("[RemoteServer] Parameter '{0}' (in:{1}) = '{2}' (type:{3})",
                        paramName, paramIn, paramVertex.Value ?? "(null)",
                        paramVertex.Value?.GetType().Name ?? "null");

                    switch (paramIn)
                    {
                        case "path":
                            pathParams[paramName] = paramVertex.Value?.ToString() ?? "";
                            break;
                        case "query":
                            queryParams.Add(new KeyValuePair<string, string>(paramName, paramVertex.Value?.ToString() ?? ""));
                            break;
                        case "header":
                            headerParams[paramName] = paramVertex.Value?.ToString() ?? "";
                            break;
                        case "body":
                            bodyParams.Add(new KeyValuePair<string, IVertex>(paramName, paramVertex));
                            break;
                    }
                }
            }

            // Build request body JSON from collected body parameters
            string requestBodyJson = BuildRequestBodyJson(bodyParams);

            // Substitute path parameters in URL template
            foreach (var kvp in pathParams)
            {
                resolvedPath = resolvedPath.Replace("{" + kvp.Key + "}", Uri.EscapeDataString(kvp.Value));
            }

            // Build query string from query parameters
            if (queryParams.Count > 0)
            {
                string separator = resolvedPath.Contains("?") ? "&" : "?";
                foreach (var kvp in queryParams)
                {
                    resolvedPath += separator + Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value);
                    separator = "&";
                }
            }

            string fullUrl = serverUrl.TrimEnd('/') + resolvedPath;

            Log("[RemoteServer] {0} {1}", httpMethod, fullUrl);
            if (requestBodyJson != null)
                Log("[RemoteServer] Request body: {0}", requestBodyJson);

            // Execute HTTP request
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(httpMethod), fullUrl);

                // Add header parameters
                foreach (var kvp in headerParams)
                {
                    request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }

                // Add request body (use UTF-8 without BOM for clean JSON)
                if (requestBodyJson != null)
                {
                    request.Content = new StringContent(requestBodyJson, Utf8NoBom, "application/json");
                }

                HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                Log("[RemoteServer] Response: {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);

                if (!response.IsSuccessStatusCode)
                {
                    Log("[RemoteServer] ERROR response body: {0}", responseBody);
                    // Do not attempt to map error responses as success data
                    return newStack;
                }

                if (!string.IsNullOrEmpty(responseBody))
                {
                    Log("[RemoteServer] Response body: {0}", responseBody);

                    // If the response was unwrapped from a *Response class, extract the inner property
                    if (responseUnwrapProperty != null)
                    {
                        responseBody = ExtractResponseProperty(responseBody, responseUnwrapProperty);
                    }

                    // Get output type from function definition
                    IVertex outputVertex = GraphUtil.GetQueryOutFirst(target, "Output", null);
                    IVertex outputType = outputVertex != null
                        ? GraphUtil.GetQueryOutFirst(outputVertex, "$EdgeTarget", null)
                        : null;

                    // Find classes root for type resolution (same logic as OpenApiUrlVertexToPackage)
                    IVertex classesRoot = GraphUtil.GetQueryOutFirst(PackageVertex, "$NewClassDefinitions", null) ?? PackageVertex;

                    // Map JSON response to GVM vertices on the new stack
                    JsonToVertex.MapJsonResponseToVertex(responseBody, newStack, outputType, classesRoot);
                }
            }
            catch (Exception ex)
            {
                Log("[RemoteServer] EXCEPTION: {0}", ex);
            }

            return newStack;
        }

        /// <summary>
        /// Extracts a single property value from a JSON response object.
        /// Used when a *Response wrapper class was unwrapped during schema generation.
        /// </summary>
        private static string ExtractResponseProperty(string responseJson, string propertyName)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(responseJson))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                        doc.RootElement.TryGetProperty(propertyName, out JsonElement propertyValue))
                    {
                        // Re-serialize the extracted value to valid JSON
                        var buffer = new ArrayBufferWriter<byte>();
                        using (var writer = new Utf8JsonWriter(buffer))
                        {
                            propertyValue.WriteTo(writer);
                        }
                        return Encoding.UTF8.GetString(buffer.WrittenSpan);
                    }
                }
            }
            catch (JsonException) { }

            return responseJson;
        }

        /// <summary>
        /// Builds a single JSON request body from collected body parameters.
        /// Single complex parameter is serialized using schema-aware serialization
        /// that reads GVM class metadata ($EdgeTarget, $MaxCardinality) for proper typing.
        /// Multiple parameters (e.g. unwrapped from a wrapper class) are combined
        /// into one JSON object with parameter names as keys.
        /// </summary>
        private static string BuildRequestBodyJson(IList<KeyValuePair<string, IVertex>> bodyParams)
        {
            if (bodyParams.Count == 0)
                return null;

            if (bodyParams.Count == 1)
            {
                IVertex vertex = bodyParams[0].Value;

                // For complex GVM class instances (value is ""), use schema-aware serialization
                // that leverages class metadata for correct JSON types (int, array, enum, etc.)
                if (vertex.Value is string s && s == "")
                {
                    string json = SerializeInstanceToSchemaJson(vertex);
                    if (!string.IsNullOrWhiteSpace(json))
                        return json;
                }

                // Fallback: try generic VertexToJson
                string vtjJson = VertexToJson.VertexToJson_Process(vertex);
                if (!string.IsNullOrWhiteSpace(vtjJson))
                    return vtjJson;

                // Final fallback for primitive vertex
                return SerializePrimitiveAsJson(vertex.Value);
            }

            // Multiple body params - combine into a single JSON object
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();
                foreach (var kvp in bodyParams)
                {
                    writer.WritePropertyName(kvp.Key);
                    WriteVertexValueToJson(kvp.Value, writer);
                }
                writer.WriteEndObject();
            }
            return Encoding.UTF8.GetString(buffer.WrittenSpan);
        }

        // ──────────────────────────────────────────────────────────
        //  Schema-aware JSON serializer
        //  Uses GVM class metadata ($EdgeTarget, $MaxCardinality)
        //  to produce correctly typed JSON for REST endpoints.
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Entry point: serializes a GVM class instance vertex to JSON
        /// using class metadata for proper type handling.
        /// </summary>
        private static string SerializeInstanceToSchemaJson(IVertex instanceVertex)
        {
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false }))
            {
                WriteSchemaAwareInstance(instanceVertex, writer, new HashSet<IVertex>());
            }
            return Encoding.UTF8.GetString(buffer.WrittenSpan);
        }

        /// <summary>
        /// Writes a GVM class instance as a JSON object.
        /// Iterates out edges grouped by meta, reads $EdgeTarget and $MaxCardinality
        /// from each attribute/association definition, and serializes values with
        /// correct JSON types (numbers, booleans, arrays, nested objects).
        /// </summary>
        private static void WriteSchemaAwareInstance(IVertex instanceVertex, Utf8JsonWriter writer, HashSet<IVertex> visited)
        {
            if (instanceVertex == null || visited.Contains(instanceVertex))
            {
                writer.WriteNullValue();
                return;
            }
            visited.Add(instanceVertex);

            writer.WriteStartObject();

            IDictionary<object, object> edgesByMeta = instanceVertex.GetOutOdgesByMeta();

            foreach (KeyValuePair<object, object> kvp in edgesByMeta)
            {
                string propertyName = kvp.Key.ToString();

                // Skip special meta keys ($Is, $Inherits, $EdgeTarget, $GraphChangeTrigger, etc.)
                if (string.IsNullOrEmpty(propertyName) || propertyName.StartsWith("$"))
                    continue;

                // Get first edge to access the attribute/association definition vertex
                IEdge firstEdge = null;
                bool hasMultipleEdges = kvp.Value is List_VertexBase;

                if (hasMultipleEdges)
                {
                    foreach (IEdge e in (List_VertexBase)kvp.Value)
                    {
                        firstEdge = e;
                        break;
                    }
                    if (firstEdge == null) continue;
                }
                else
                {
                    firstEdge = (IEdge)kvp.Value;
                }

                if (VertexOperations.IsSpecialVertex(firstEdge.Meta))
                    continue;

                // Read type information from the attribute/association definition
                IVertex meta = firstEdge.Meta;
                IVertex edgeTarget = GraphUtil.GetQueryOutFirst(meta, "$EdgeTarget", null);
                int? maxCardinality = GraphUtil.GetIntegerValue(
                    GraphUtil.GetQueryOutFirst(meta, "$MaxCardinality", null));
                bool isArray = maxCardinality.HasValue && maxCardinality.Value == -1;

                if (isArray)
                {
                    writer.WritePropertyName(propertyName);
                    WriteSchemaAwareArray(kvp.Value, hasMultipleEdges, edgeTarget, writer, visited);
                }
                else if (IsSchemaTypePrimitive(edgeTarget))
                {
                    // Skip empty enum values entirely — server will use the default
                    if (IsSchemaTypeEnum(edgeTarget))
                    {
                        string enumVal = firstEdge.To?.Value?.ToString() ?? "";
                        if (string.IsNullOrEmpty(enumVal))
                            continue;
                    }

                    writer.WritePropertyName(propertyName);
                    WriteTypedPrimitiveValue(firstEdge.To, edgeTarget, writer);
                }
                else
                {
                    // Complex (non-primitive) nested object
                    if (IsVertexEmpty(firstEdge.To))
                        continue; // Omit empty complex objects

                    writer.WritePropertyName(propertyName);
                    WriteSchemaAwareInstance(firstEdge.To, writer, visited);
                }
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Writes an array of items. Handles both List_VertexBase (multiple edges)
        /// and single IEdge (one or zero items) cases.
        /// </summary>
        private static void WriteSchemaAwareArray(object edgesValue, bool hasMultipleEdges,
            IVertex itemType, Utf8JsonWriter writer, HashSet<IVertex> visited)
        {
            writer.WriteStartArray();

            if (hasMultipleEdges)
            {
                foreach (IEdge e in (List_VertexBase)edgesValue)
                {
                    if (VertexOperations.IsSpecialVertex(e.Meta))
                        continue;

                    if (IsSchemaTypePrimitive(itemType))
                        WriteTypedPrimitiveValue(e.To, itemType, writer);
                    else if (!IsVertexEmpty(e.To))
                        WriteSchemaAwareInstance(e.To, writer, visited);
                }
            }
            else
            {
                // Single edge for an array property — may be a placeholder for empty list
                IEdge singleEdge = (IEdge)edgesValue;
                IVertex to = singleEdge.To;

                if (!IsVertexEmpty(to))
                {
                    if (IsSchemaTypePrimitive(itemType))
                        WriteTypedPrimitiveValue(to, itemType, writer);
                    else
                        WriteSchemaAwareInstance(to, writer, visited);
                }
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// Writes a primitive or enum value to JSON, coercing from the GVM string
        /// representation to the correct JSON type based on the $EdgeTarget schema type.
        /// </summary>
        private static void WriteTypedPrimitiveValue(IVertex valueVertex, IVertex typeVertex, Utf8JsonWriter writer)
        {
            if (valueVertex == null)
            {
                writer.WriteNullValue();
                return;
            }

            object value = valueVertex.Value;
            string typeName = typeVertex?.Value?.ToString() ?? "String";

            // If the value is already the correct .NET type, write directly
            if (value is int intVal) { writer.WriteNumberValue(intVal); return; }
            if (value is long longVal) { writer.WriteNumberValue(longVal); return; }
            if (value is double doubleVal) { writer.WriteNumberValue(doubleVal); return; }
            if (value is float floatVal) { writer.WriteNumberValue(floatVal); return; }
            if (value is bool boolVal) { writer.WriteBooleanValue(boolVal); return; }

            // Convert from string representation based on the expected schema type
            string strValue = value?.ToString() ?? "";

            switch (typeName)
            {
                case "Integer":
                    if (long.TryParse(strValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsedLong))
                        writer.WriteNumberValue(parsedLong);
                    else
                        writer.WriteNumberValue(0);
                    break;

                case "Double":
                case "Float":
                    if (double.TryParse(strValue, NumberStyles.Float | NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture, out double parsedDouble))
                        writer.WriteNumberValue(parsedDouble);
                    else
                        writer.WriteNumberValue(0.0);
                    break;

                case "Boolean":
                    writer.WriteBooleanValue(
                        strValue == "1" || strValue.Equals("true", StringComparison.OrdinalIgnoreCase));
                    break;

                default:
                    // String and Enum types — write as JSON string
                    writer.WriteStringValue(strValue);
                    break;
            }
        }

        /// <summary>
        /// Returns true if the type vertex represents a primitive or enum type
        /// (String, Integer, Double, Float, Boolean, DateTime, or an enum).
        /// </summary>
        private static bool IsSchemaTypePrimitive(IVertex typeVertex)
        {
            if (typeVertex == null)
                return true; // Unknown type, fall back to primitive handling

            string typeName = typeVertex.Value?.ToString() ?? "";

            if (typeName == "String" || typeName == "Integer" || typeName == "Double" ||
                typeName == "Float" || typeName == "Boolean" || typeName == "DateTime")
                return true;

            return IsSchemaTypeEnum(typeVertex);
        }

        /// <summary>
        /// Returns true if the type vertex is an enum (inherits from EnumBase).
        /// </summary>
        private static bool IsSchemaTypeEnum(IVertex typeVertex)
        {
            if (typeVertex == null) return false;
            return GraphUtil.ExistQueryOut(typeVertex, "$Inherits", "EnumBase");
        }

        /// <summary>
        /// Returns true if the vertex is empty — either null, has an empty string
        /// value with no data edges (i.e. a placeholder / default instance).
        /// </summary>
        private static bool IsVertexEmpty(IVertex vertex)
        {
            if (vertex == null) return true;

            string value = vertex.Value?.ToString() ?? "";
            if (value != "") return false; // Has a meaningful value

            // Value is "" — check for any non-special data edges
            foreach (IEdge e in vertex.OutEdges)
            {
                if (e.Meta != null && !VertexOperations.IsSpecialVertex(e.Meta))
                    return false; // Has data edges → not empty
            }

            return true; // No data edges → empty placeholder
        }

        // ──────────────────────────────────────────────────────────
        //  Legacy helpers (used by multiple-param body builder)
        // ──────────────────────────────────────────────────────────

        private static void WriteVertexValueToJson(IVertex vertex, Utf8JsonWriter writer)
        {
            object value = vertex.Value;

            if (value is string s && s == "")
            {
                string json = VertexToJson.VertexToJson_Process(vertex);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(json))
                        {
                            doc.RootElement.WriteTo(writer);
                            return;
                        }
                    }
                    catch (JsonException) { }
                }
                writer.WriteNullValue();
                return;
            }

            WritePrimitiveToJson(value, writer);
        }

        private static void WritePrimitiveToJson(object value, Utf8JsonWriter writer)
        {
            if (value is int i) writer.WriteNumberValue(i);
            else if (value is long l) writer.WriteNumberValue(l);
            else if (value is double d) writer.WriteNumberValue(d);
            else if (value is float f) writer.WriteNumberValue(f);
            else if (value is bool b) writer.WriteBooleanValue(b);
            else if (value is string str) writer.WriteStringValue(str);
            else if (value == null) writer.WriteNullValue();
            else writer.WriteStringValue(value.ToString());
        }

        private static string SerializePrimitiveAsJson(object value)
        {
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                WritePrimitiveToJson(value, writer);
            }
            return Encoding.UTF8.GetString(buffer.WrittenSpan);
        }
    }
}
