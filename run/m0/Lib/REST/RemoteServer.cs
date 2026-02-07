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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace m0.Lib.REST
{
    public class RemoteServer
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static INoInEdgeInOutVertexVertex CallRemoteRestServer(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex target = VertexOperations.GetTargetFromStackTop(stack);

            IVertex RemoteEndpointPathVertex = GraphUtil.GetQueryOutFirst(target, "RemoteEndpointPath", null);
            IVertex RemoteEndpointParametersVertex = GraphUtil.GetQueryOutFirst(target, "RemoteEndpointParameters", null);

            IVertex PackageVertex = GraphUtil.GetQueryInFirst(target, "Function", null);

            IVertex RemoteServerUrlVertex = GraphUtil.GetQueryOutFirst(PackageVertex, "RemoteServerUrl", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            string serverUrl = GraphUtil.GetStringValueOrNull(RemoteServerUrlVertex);
            string endpointPath = GraphUtil.GetStringValueOrNull(RemoteEndpointPathVertex);
            string parametersJson = GraphUtil.GetStringValueOrNull(RemoteEndpointParametersVertex);

            if (string.IsNullOrEmpty(serverUrl) || string.IsNullOrEmpty(endpointPath))
                return newStack;

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
                        continue;

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

            // Execute HTTP request
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(httpMethod), fullUrl);

                // Add header parameters
                foreach (var kvp in headerParams)
                {
                    request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }

                // Add request body
                if (requestBodyJson != null)
                {
                    request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (!string.IsNullOrEmpty(responseBody))
                {
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
            catch (Exception)
            {
                // Return empty stack on HTTP or parsing error
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
        /// Single complex parameter is serialized via VertexToJson.
        /// Multiple parameters (e.g. unwrapped from *Request class) are combined
        /// into one JSON object with parameter names as keys.
        /// </summary>
        private static string BuildRequestBodyJson(IList<KeyValuePair<string, IVertex>> bodyParams)
        {
            if (bodyParams.Count == 0)
                return null;

            if (bodyParams.Count == 1)
            {
                // Single body param - try VertexToJson for complex objects
                IVertex vertex = bodyParams[0].Value;
                string json = VertexToJson.VertexToJson_Process(vertex);
                if (!string.IsNullOrWhiteSpace(json))
                    return json;

                // Fallback for primitive vertex (VertexToJson returns empty for primitives)
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

        /// <summary>
        /// Writes a vertex value to a Utf8JsonWriter.
        /// For complex objects (value == ""), uses VertexToJson.
        /// For primitives, writes the value directly.
        /// </summary>
        private static void WriteVertexValueToJson(IVertex vertex, Utf8JsonWriter writer)
        {
            object value = vertex.Value;

            // Empty string value typically means a complex GVM object instance
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
