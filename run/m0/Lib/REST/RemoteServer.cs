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
        /// Builds a single JSON request body from one or more body parameters.
        /// Single parameter is serialized directly. Multiple parameters are combined
        /// into one JSON object with parameter names as keys.
        /// </summary>
        private static string BuildRequestBodyJson(IList<KeyValuePair<string, IVertex>> bodyParams)
        {
            if (bodyParams.Count == 0)
                return null;

            if (bodyParams.Count == 1)
            {
                return VertexToJson.VertexToJson_Process(bodyParams[0].Value);
            }

            // Multiple body parameters: combine into a single JSON object
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false }))
            {
                writer.WriteStartObject();

                foreach (var kvp in bodyParams)
                {
                    writer.WritePropertyName(kvp.Key);

                    string paramJson = VertexToJson.VertexToJson_Process(kvp.Value);

                    try
                    {
                        using (JsonDocument paramDoc = JsonDocument.Parse(paramJson))
                        {
                            paramDoc.RootElement.WriteTo(writer);
                        }
                    }
                    catch (JsonException)
                    {
                        // If serialized value is not valid JSON, write as string
                        writer.WriteStringValue(paramJson);
                    }
                }

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(buffer.WrittenSpan);
        }
    }
}
