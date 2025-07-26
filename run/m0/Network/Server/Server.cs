using m0.Foundation;
using m0.Graph;
using m0.Lib.Net;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

        public IVertex mappingVertex = null;

        private WebApplication? _app;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _serverTask;
        private StreamWriter? _logWriter;
        private readonly object _logLock = new object();

        public string BaseUrl { get; private set; }
        public bool IsRunning => _app != null && _serverTask != null && !_serverTask.IsCompleted;

        public HttpServer(IVertex _mapingVertex)
        {
            mappingVertex = _mapingVertex;
        }

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
                case "HEAD": action = HttpActionEnum.HEADOPTIONS; break;
                case "OPTIONS": action = HttpActionEnum.HEADOPTIONS; break;
                case "TRACE": action = HttpActionEnum.TRACE; break;
                default: action = HttpActionEnum.GET; break;
            }
            
            // Log the HTTP request
            LogHttpRequest(context, method, url);
            
            return Results.Text(DoHttpMapping(url, HttpActionEnumHelper.GetVertex(action)));
        }

        private void LogHttpRequest(HttpContext context, string method, string url)
        {
            try
            {
                lock (_logLock)
                {
                    if (_logWriter == null)
                    {
                        string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "http_server.log");
                        _logWriter = new StreamWriter(logFilePath, true);
                    }

                    // Standard HTTP log format: IP - - [timestamp] "METHOD /path HTTP/1.1" status_code response_size
                    string remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "-";
                    string timestamp = DateTime.Now.ToString("dd/MMM/yyyy:HH:mm:ss zzz");
                    string userAgent = context.Request.Headers["User-Agent"].ToString() ?? "-";
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

        private string DoHttpMapping(string url, IVertex actionVertexRequested)
        {
            IList<IEdge> mappings = GraphUtil.GetQueryOut(mappingVertex, "HttpMappingEntry", null);

            foreach (IEdge e in mappings)
            {
                IVertex actionVertex = GraphUtil.GetQueryOutFirst(e.To, "Action", null);

                if (actionVertex == null)
                    continue;

                if (!GraphUtil.GetValueAndCompareStrings(actionVertex, actionVertexRequested))
                    continue;

                IVertex pathMaskVertex = GraphUtil.GetQueryOutFirst(e.To, "PathMask", null);

                if (pathMaskVertex == null)
                    continue;

                string pathMask = GraphUtil.GetStringValue(pathMaskVertex);

                if (!IsPathMatch(pathMask, url))
                    continue;

                IVertex handlerVertex = GraphUtil.GetQueryOutFirst(e.To, "Handler", null);

                if (handlerVertex == null)
                    continue;

                return CallHandler(handlerVertex, url);
            }

            return "[404]";
        }

        private bool IsPathMatch(string pathMask, string url)
        {
            if (pathMask.Contains("*"))
            {
                string startsWith = pathMask.Substring(0, pathMask.IndexOf('*'));

                if (url.StartsWith(startsWith))
                    return true;
                else
                    return false;
            }
            else {
                if (pathMask == url)
                    return true;
                else
                    return false;
            }
        }

        private string CallHandler(IVertex handlerVertex, string url)
        {
            IVertex parameters = InstructionHelpers.CreateStack();

            parameters.AddVertex(url_meta, url);

            INoInEdgeInOutVertexVertex ret = ZeroCodeExecutonUtil.FuncionCall(handlerVertex, parameters);

            if (ret.OutEdges.Count > 0)
                return ret.OutEdges[0].To.ToString();
            else
                return "[null]";
        }

        private string ExecuteHandler(IVertex httpMappingVertex)
        {
            return "kotek";
        }

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