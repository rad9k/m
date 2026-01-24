using m0.Foundation;
using m0.Graph;
using m0.Lib.Net;
using m0.Store.FileSystem;
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
        private static StreamWriter _logWriter;
        private static readonly object _logLock = new object();
        private static string _currentServerTimestamp;

        public static string RestHandler(IVertex handlerVertex, string url_path, string url_rest, IVertex actionVertex, HttpServer server)
        {
            string action = GraphUtil.GetStringValue(actionVertex);
            string requestTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Log request
            if (server.DoRestLog)
                LogToFile($"[REQUEST {requestTimestamp}] {action} {url_path}{url_rest}", server);

            // Call internal handler
            string response = RestHandler_Internal(handlerVertex, url_path, url_rest, actionVertex);

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
                        string logFilename = $"REST-{server.ServerStartTimestamp}.log";

                        FileSystemUtil.CreateDirectoryIfNotExist(MinusZero.Instance.m0DllPath, "http");
                        string httpPath = Path.Combine(MinusZero.Instance.m0DllPath, "http");
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

        private static string RestHandler_Internal(IVertex handlerVertex, string url_path, string url_rest, IVertex actionVertex)
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
                    return HandleOpenApiGet(handlerVertex);
                }
            }

            return "kupka";
        }

        private static string HandleRootGet(IVertex handlerVertex)
        {
            return "OK";
        }

        private static string HandleOpenApiGet(IVertex handlerVertex)
        {
            return OpenApiDocumentationGenerator.GetOpenApiDocumentation(handlerVertex);
        }
    }
}
