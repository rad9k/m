using m0.Foundation;
using m0.Graph;
using m0.Lib.Net;
using m0.Store.FileSystem;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace m0.Network.Server { 

    public class HttpServer
    {
        private IVertex url_meta = MinusZero.Instance.Root.Get(false, @"System\Lib\Net\HttpHandler\url");

        public IVertex thisVertex = null;

        private WebApplication? _app;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _serverTask;
        private StreamWriter? _logWriter;
        private readonly object _logLock = new object();

        public string BaseUrl { get; private set; }
        public bool IsRunning => _app != null && _serverTask != null && !_serverTask.IsCompleted;

        public HttpServer(IVertex _thisVertex)
        {
            thisVertex = _thisVertex;
        }

        private readonly object _lockObject = new object();

        // New method handling every HTTP request
        private IResult HandleRequest(HttpContext context)
        {            
            // Get URL and HTTP action
            string url = context.Request.Path.ToString();
            var method = context.Request.Method;
            m0.Lib.Net.HttpActionEnum action = m0.Lib.Net.HttpActionEnum.GET;
            switch (method.ToUpperInvariant())
            {
                case "GET": action = HttpActionEnum.GET; break;
                case "POST": action = HttpActionEnum.POST; break;
                case "PUT": action = HttpActionEnum.PUT; break;
                case "DELETE": action = HttpActionEnum.DELETE; break;
                case "PATCH": action = HttpActionEnum.PATCH; break;
                case "HEAD": action = HttpActionEnum.HEAD; break;
                case "OPTIONS": action = HttpActionEnum.HEAD; break;
                case "TRACE": action = HttpActionEnum.TRACE; break;
                default: action = HttpActionEnum.GET; break;
            }

            // Log the HTTP request
            LogHttpRequest(context, method, url);


            IResult result;

            string response = DoHttpMapping(context, url, HttpActionEnumHelper.GetVertex(action), out result);

            if (result != null) 
                return result;

            // Check if response looks like HTML and set appropriate content type
            if (response != null && response.TrimStart().StartsWith("<"))            
                return Results.Content(response, "text/html; charset=utf-8");            
            else            
                return Results.Text(response);            
            
        }

        private void LogHttpRequest(HttpContext context, string method, string url)
        {
            bool doLog = true;
            string logFilename = "";

            IVertex doLogVertex = GraphUtil.GetQueryOutFirst(thisVertex, "DoLog", null);

            if (doLogVertex != null)
                doLog = GraphUtil.GetBooleanValueOrFalse(doLogVertex);

            IVertex logFilenameVertex = GraphUtil.GetQueryOutFirst(thisVertex, "LogFilename", null);

            if (logFilenameVertex != null)
                logFilename = GraphUtil.GetStringValueOrNull(logFilenameVertex);

            if (logFilename == "")
            {
                IVertex portVertex = GraphUtil.GetQueryOutFirst(thisVertex, "Port", null);

                int port = GraphUtil.GetIntegerValueOr0(portVertex);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                logFilename = "http_server_" + port + "_" + timestamp + ".log";
            }

            try
            {
                lock (_logLock)
                {
                    if (_logWriter == null)
                    {
                        FileSystemUtil.CreateDirectoryIfNotExist(MinusZero.Instance.m0DllPath, "http");

                        string httpPath = Path.Combine(MinusZero.Instance.m0DllPath, "http");

                        string logFilePath = Path.Combine(httpPath, logFilename);
                        _logWriter = new StreamWriter(logFilePath, true);
                    }

                    // Standard HTTP log format: IP - - [timestamp] "METHOD /path HTTP/1.1" status_code response_size
                    string remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "-";
                    string timestamp = DateTime.Now.ToString(@"dd\/MM\/yyyy:HH:mm:ss zzz");
                    string userAgent = context.Request.Headers["User-AMgent"].ToString() ?? "-";
                    string referer = context.Request.Headers["Referer"].ToString() ?? "-";
                    
                    // Log in Common Log Format (CLF)
                    string logEntry = $"{remoteIp} - - [{timestamp}] \"{method} {url} HTTP/{context.Request.Protocol}\" - - \"{referer}\" \"{userAgent}\"";
                    
                    _logWriter.WriteLine(logEntry);
                    _logWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                // Silently handle logging errors to not break the server
                Console.WriteLine($"Logging error: {ex.Message}");
            }
        }

        private string DoHttpMapping(HttpContext context, string url, IVertex actionVertexRequested, out IResult result)
        {
            result = null;

            IVertex mappingVertex = GraphUtil.GetQueryOutFirst(thisVertex, "Mapping", null);

            if (mappingVertex == null)
                return null;

            IList<IEdge> mappings = GraphUtil.GetQueryOut(mappingVertex, "HttpMappingEntry", null);

            foreach (IEdge e in mappings)
            {
                IVertex actionVertex = GraphUtil.GetQueryOutFirst(e.To, "Action", null);

                if (actionVertex == null)
                    continue;                

                IVertex pathMaskVertex = GraphUtil.GetQueryOutFirst(e.To, "PathMask", null);

                if (pathMaskVertex == null)
                    continue;

                string pathMask = GraphUtil.GetStringValue(pathMaskVertex);

                int pathMatch = IsPathMatch(pathMask, url);

                if (pathMatch == -1)
                    continue;

                IVertex handlerVertex = GraphUtil.GetQueryOutFirst(e.To, "Handler", null);

                if (handlerVertex == null)
                    continue;

                if (GraphUtil.GetValueAndCompareStrings(actionVertex, "REST"))
                    return REST.RestHandler(handlerVertex, url, actionVertexRequested);

                if (!GraphUtil.GetValueAndCompareStrings(actionVertex, actionVertexRequested))
                    continue;

                if (GraphUtil.ExistQueryOut(handlerVertex, "$Is", "Directory"))
                {
                    // Handle as file request
                    result = HandleFileRequest(context, url.Substring(pathMatch), handlerVertex);
                    return null;
                }

                return CallHandler(handlerVertex, url);
            }

            return "[404]";
        }

        private int IsPathMatch(string pathMask, string url)
        {
            if (pathMask.Contains("*"))
            {
                string startsWith = pathMask.Substring(0, pathMask.IndexOf('*'));

                if (url.StartsWith(startsWith))
                    return startsWith.Length;
                else
                    return -1;
            }
            else {
                if (pathMask == url)
                    return pathMask.Length;
                else
                    return -1;
            }
        }

        private string CallHandler(IVertex handlerVertex, string url)
        {
            lock (_lockObject)
            {
                IVertex parameters = InstructionHelpers.CreateStack();

                parameters.AddVertex(url_meta, url);

                INoInEdgeInOutVertexVertex ret = ZeroCodeExecutonUtil.FuncionCall(handlerVertex, parameters);

                if (ret.OutEdges.Count > 0)
                    return ret.OutEdges[0].To.ToString();
                else
                    return "[null]";
            }
        }

        // FILE HANDLING BEG

        private string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".bmp" => "image/bmp",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                ".ico" => "image/x-icon",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".html" or ".htm" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".ogg" => "audio/ogg",
                ".flac" => "audio/flac",
                ".m4a" => "audio/mp4",
                _ => "application/octet-stream"
            };
        }

        private IResult HandleFileRequest(HttpContext context, string url, IVertex handler)
        {            
            string method = context.Request.Method.ToUpperInvariant();

            if ((method != "GET" && method != "HEAD") 
                || url.Contains("./")
                || url.Contains(@".\")
                || url.Contains("../")
                || url.Contains(@"..\")
                )            
                return Results.StatusCode(405); // Method Not Allowed            

            string[] urlSplit = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            IVertex directoryIterator = handler;

            for (int pos = 0; pos < urlSplit.Length - 1; pos++)
            {
                directoryIterator = GraphUtil.GetQueryOutFirst(directoryIterator, "Directory", urlSplit[pos]);

                if (directoryIterator == null)
                    return Results.StatusCode(404); // Not Found
            }

            IVertex fileVertex = GraphUtil.GetQueryOutFirst(directoryIterator, "File", urlSplit[^1]);

            if (fileVertex == null)
                return Results.StatusCode(404); // Not Found

            
            IVertex fullFilepathVertex = GraphUtil.GetQueryOutFirst(fileVertex, "FullFilename", null);
            string filePath = GraphUtil.GetStringValue(fullFilepathVertex);

            if (!File.Exists(filePath))
                return Results.StatusCode(404); // Not Found
                
            if (method == "HEAD")
            {
                string contentType = GetContentType(filePath);
                var fileInfo = new FileInfo(filePath);
                context.Response.Headers["Content-Type"] = contentType;
                context.Response.Headers["Content-Length"] = fileInfo.Length.ToString();
                return Results.Ok();
            }

            string fileContentType = GetContentType(filePath);
            return HandleFileRequestWithRange(context, filePath, fileContentType);
        }

        private IResult HandleFileRequestWithRange(HttpContext context, string filePath, string contentType)
        {
            var fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            var rangeHeader = context.Request.Headers["Range"].ToString();

            if (string.IsNullOrEmpty(rangeHeader))
            {
                // Normalne żądanie - cały plik
                var fileStream = File.OpenRead(filePath);
                context.Response.Headers["Accept-Ranges"] = "bytes";
                return Results.File(fileStream, contentType, Path.GetFileName(filePath));
            }

            // Parsowanie Range header: "bytes=1234-5678" lub "bytes=1234-"
            var range = rangeHeader.Replace("bytes=", "").Split('-');
            long start = long.Parse(range[0]);
            long end = range.Length > 1 && !string.IsNullOrEmpty(range[1])
                ? long.Parse(range[1])
                : fileSize - 1;

            long contentLength = end - start + 1;

            // Otwórz plik i przeskocz do pozycji start
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            stream.Seek(start, SeekOrigin.Begin);

            // Ustaw nagłówki dla partial content
            context.Response.StatusCode = 206; // Partial Content
            context.Response.Headers["Content-Range"] = $"bytes {start}-{end}/{fileSize}";
            context.Response.Headers["Accept-Ranges"] = "bytes";
            context.Response.Headers["Content-Length"] = contentLength.ToString();
            context.Response.ContentType = contentType;

            // Zwróć fragment pliku
            return Results.Stream(stream, contentType);
        }

        // FILE HANDLING END

        // Remove ConfigureEndpoints and map all HTTP methods to HandleRequest
        public void StartAsync(string url = "http://localhost:5000")
        {
            if (IsRunning)
                throw new InvalidOperationException("Server allready running");

            BaseUrl = url;
            _cancellationTokenSource = new CancellationTokenSource();

            var builder = WebApplication.CreateBuilder();

            // Server configuration
            builder.WebHost.UseUrls(url);

            // Disable logging if not needed
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            _app = builder.Build();

            // Map all HTTP methods to a single handler
            _app.MapMethods("/{**catchall}", new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "TRACE" }, HandleRequest);

            // Run the server in a separate task
            _serverTask = _app.RunAsync(_cancellationTokenSource.Token);
        }

        public async Task StopAsync()
        {
            if (!IsRunning)
                return;

            _cancellationTokenSource?.Cancel();

            if (_serverTask != null)
            {
                try
                {
                    await _serverTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelled
                }
            }

            // Corrected: DisposeAsync should be used instead of Dispose
            if (_app != null)
            {
                await _app.DisposeAsync();
            }

            _cancellationTokenSource?.Dispose();

            // Close and dispose the log writer
            lock (_logLock)
            {
                _logWriter?.Close();
                _logWriter?.Dispose();
                _logWriter = null;
            }

            _app = null;
            _serverTask = null;
            _cancellationTokenSource = null;
        }
    }
}