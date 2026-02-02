using m0.Foundation;
using m0.Graph;
using m0.Lib.Net;
using m0.Store.FileSystem;
using m0.User.Process.UX;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Network.Server
{
    public class REST
    {
        private static IVertex JsonToVertex_meta = MinusZero.Instance.Root.Get(false, @"System\Lib\StdView\JsonToVertex");
        private static IVertex VertexToJson_meta = MinusZero.Instance.Root.Get(false, @"System\Lib\StdView\VertexToJson");
        private static IVertex NewClassDefinitions_meta = MinusZero.Instance.Root.Get(false, @"System\Lib\StdView\Json\$NewClassDefinitions");
        private static IVertex JsonRootDefinition_meta = MinusZero.Instance.Root.Get(false, @"System\Lib\StdView\Json\$JsonRootDefinition");

        private static StreamWriter _logWriter;
        private static readonly object _logLock = new object();
        private static string _currentServerTimestamp;

        public static string RestHandler(IVertex handlerVertex, string url_path, string url_rest, IVertex actionVertex, string requestBody, HttpServer server)
        {
            string action = GraphUtil.GetStringValue(actionVertex);
            string requestTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Log request
            if (server.DoRestLog)
            {
                string bodyLog = string.IsNullOrEmpty(requestBody) ? "" : $"\n{requestBody}";
                LogToFile($"[REQUEST {requestTimestamp}] {action} {url_path}{url_rest}{bodyLog}", server);
            }

            // Call internal handler
            string response = RestHandler_Internal(handlerVertex, url_path, url_rest, actionVertex, requestBody);

            string responseTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Log response
            if (server.DoRestLog)
                LogToFile($"[RESPONSE {responseTimestamp}]\n{response}", server);

            return response;
        }

        private static void LogToFile(string message, HttpServer server)
        {
            try
            {
                lock (_logLock)
                {
                    // If server timestamp changed (new server instance), close old writer
                    if (_currentServerTimestamp != server.ServerStartTimestamp && _logWriter != null)
                    {
                        _logWriter.Close();
                        _logWriter.Dispose();
                        _logWriter = null;
                    }

                    if (_logWriter == null)
                    {
                        _currentServerTimestamp = server.ServerStartTimestamp;
                        string logFilename;
                        
                        if (server.RestLogFilename == null || server.RestLogFilename == "")
                            logFilename = "REST_server_" + server.Port + "_" + server.ServerStartTimestamp + ".log";
                        else
                            logFilename = server.RestLogFilename;

                            FileSystemUtil.CreateDirectoryIfNotExist(MinusZero.Instance.m0DllPath, "log");
                        string httpPath = Path.Combine(MinusZero.Instance.m0DllPath, "log");
                        string logFilePath = Path.Combine(httpPath, logFilename);

                        _logWriter = new StreamWriter(logFilePath, true);
                    }

                    _logWriter.WriteLine(message);
                    _logWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"REST logging error: {ex.Message}");
            }
        }

        private static string RestHandler_Internal(IVertex handlerVertex, string url_path, string url_rest, IVertex actionVertex, string requestBody)
        {
            // Handle root endpoint "/" - return simple success response for GET
            if (string.IsNullOrEmpty(url_rest) || url_rest == "/")
            {
                if (GraphUtil.GetValueAndCompareStrings(actionVertex, HttpActionEnumHelper.GetVertex(HttpActionEnum.GET)))
                {
                    return HandleRootGet(handlerVertex);
                }
            }

            // Handle /openapi.json endpoint - return OpenAPI documentation
            if (url_rest == "/openapi.json" || url_rest == "/openapi" || url_rest == "openapi.json" || url_rest == "openapi")
            {
                if (GraphUtil.GetValueAndCompareStrings(actionVertex, HttpActionEnumHelper.GetVertex(HttpActionEnum.GET)))
                {
                    return HandleOpenApiGet(handlerVertex, url_path);
                }
            }

            // Handle POST requests - function calls
            if (GraphUtil.GetValueAndCompareStrings(actionVertex, HttpActionEnumHelper.GetVertex(HttpActionEnum.POST)))
            {
                return HandleFunctionPost(handlerVertex, url_rest, requestBody);
            }

            return "handler not found";
        }

        private static string HandleFunctionPost(IVertex handlerVertex, string url_rest, string requestBody)
        {
            // Extract function name from url_rest (remove leading '/')
            string functionName = url_rest;
            if (functionName.StartsWith("/"))
                functionName = functionName.Substring(1);
            
            // Remove trailing '/' if present
            if (functionName.EndsWith("/"))
                functionName = functionName.Substring(0, functionName.Length - 1);

            if (string.IsNullOrEmpty(functionName))
                return "{\"error\": \"Function name not specified\"}";

            // Find the function by name
            IVertex functionVertex = GraphUtil.GetQueryOutFirst(handlerVertex, "Function", functionName);

            if (functionVertex == null)
                return $"{{\"error\": \"Function '{functionName}' not found\"}}";

            // Check if function has Endpoint meta edge
            if (!GraphUtil.ExistQueryOut(functionVertex, "Endpoint", null))
                return $"{{\"error\": \"Function '{functionName}' is not exposed as endpoint\"}}";

            // Call the MVEG handler
            return CallMVEGHandler(handlerVertex, functionVertex, requestBody);
        }

        private static string HandleRootGet(IVertex handlerVertex)
        {
            return "OK";
        }

        private static string HandleOpenApiGet(IVertex handlerVertex, string url_path)
        {
            return OpenApiDocumentationGenerator.GetOpenApiDocumentation(handlerVertex, url_path);
        }

        private static string CallMVEGHandler(IVertex handlerVertex, IVertex functionVertex, string inputJson)
        {
            IList<IEdge> ExistingClassDefinitions = GraphUtil.GetQueryOut(handlerVertex, "$ExistingClassDefinitions", null);

            IEdge NewClassDefinitions = GraphUtil.GetQueryOutFirstEdge(functionVertex, "$NewClassDefinitions", null);

            IVertex InputMVEGRootVertex = MinusZero.Instance.CreateTempVertex();

            foreach (IEdge exsisingClassDefinitionsEdge in ExistingClassDefinitions)            
                InputMVEGRootVertex.AddEdge(exsisingClassDefinitionsEdge.Meta, exsisingClassDefinitionsEdge.To);

            if (NewClassDefinitions != null)
                InputMVEGRootVertex.AddEdge(NewClassDefinitions_meta, NewClassDefinitions.To);
            else
                InputMVEGRootVertex.AddVertex(NewClassDefinitions_meta, "");

            InputMVEGRootVertex.AddEdge(JsonRootDefinition_meta, functionVertex);

            //

            IVertex parametersStack = InstructionHelpers.CreateStack();
            

            IVertex InputJsonVertex = MinusZero.Instance.CreateTempVertex();

            InputJsonVertex.Value = inputJson;

            InputJsonVertex.AddEdge(JsonToVertex_meta, InputMVEGRootVertex);

            foreach (IEdge parameterEdge in InputMVEGRootVertex)
            {
                string parameterName = GraphUtil.GetStringValue(parameterEdge.To);

                IVertex functionParameter = GraphUtil.GetQueryOutFirst(functionVertex, "InputParameter", parameterName);

                if (functionParameter != null)
                    parametersStack.AddEdge(functionParameter, parameterEdge.To);                                    
            }

            //

            IVertex returnStack = ZeroCodeExecutonUtil.FuncionCall(functionVertex, parametersStack);

            IVertex returnVertex = MinusZero.Instance.CreateTempVertex();

            foreach (IEdge e in returnStack)
                returnVertex.AddEdge(null, e.To);

            IVertex return_Json = returnVertex.AddVertex(VertexToJson_meta, "");


  
            return GraphUtil.GetStringValue(return_Json);
        }
    }
}
