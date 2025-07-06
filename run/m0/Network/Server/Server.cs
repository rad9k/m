using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace m0.Network.Server { 

    public class HttpServerLibrary
    {
        private WebApplication? _app;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _serverTask;

        public string BaseUrl { get; private set; } = "http://localhost:5000";
        public bool IsRunning => _app != null && _serverTask != null && !_serverTask.IsCompleted;

        public void StartAsync(string url = "http://localhost:5000")
        {
            if (IsRunning)
                throw new InvalidOperationException("Serwer już działa");

            BaseUrl = url;
            _cancellationTokenSource = new CancellationTokenSource();

            var builder = WebApplication.CreateBuilder();

            // Konfiguracja serwera
            builder.WebHost.UseUrls(url);

            // Wyłączenie logowania jeśli nie potrzebne
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            _app = builder.Build();

            // Konfiguracja endpointów
            ConfigureEndpoints(_app);

            // Uruchomienie serwera w osobnym tasku
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
                    // Oczekiwane przy anulowaniu
                }
            }

            //_app?.Dispose();

            _cancellationTokenSource?.Dispose();

            _app = null;
            _serverTask = null;
            _cancellationTokenSource = null;
        }

        private void ConfigureEndpoints(WebApplication app)
        {
            // Prosty endpoint testowy
            app.MapGet("/", () => "Serwer działa!");

            // Endpoint zwracający JSON
            app.MapGet("/api/status", () => new { Status = "OK", Time = DateTime.Now });

            // Endpoint przyjmujący POST
            app.MapPost("/api/data", (DataModel data) =>
            {
              //  return Results.Ok(new { Message = "Dane otrzymane", ReceivedData = data });
            });

            // Endpoint z parametrem
            app.MapGet("/api/hello/{name}", (string name) => $"Witaj {name}!");
        }
    }

    // Prosta klasa modelu dla POST endpoint
    public class DataModel
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }
}