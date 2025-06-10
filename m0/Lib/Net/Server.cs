/*using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Dodaj usługi
builder.Services.AddSingleton<IRequestLogger, FileRequestLogger>();

var app = builder.Build();

// Pobierz logger
var requestLogger = app.Services.GetRequiredService<IRequestLogger>();

// Jeden uniwersalny handler dla wszystkich requestów
app.MapMethods("/{**path}",
    new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" },
    async (HttpContext context, string? path) =>
    {
        var startTime = DateTime.UtcNow;

        // Pobierz informacje o requeście
        var httpMethod = context.Request.Method;
        var resourceUrl = GetFullUrl(context);

        // Zaloguj request
        await requestLogger.LogRequestAsync(context, startTime, httpMethod, resourceUrl);

        // Przetwórz request i zwróć odpowiedź
        var response = ProcessRequest(httpMethod, path ?? "", context);

        return response;
    });

// Uruchom serwer
app.Run("http://localhost:5000");

// Funkcja pomocnicza do budowania pełnego URL
static string GetFullUrl(HttpContext context)
{
    var request = context.Request;
    return $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
}

// Główna logika przetwarzania requestów
static IResult ProcessRequest(string httpMethod, string path, HttpContext context)
{
    // Przykładowa logika routingu w zależności od metody HTTP i ścieżki
    return httpMethod.ToUpper() switch
    {
        "GET" => HandleGetRequest(path, context),
        "POST" => HandlePostRequest(path, context),
        "PUT" => HandlePutRequest(path, context),
        "DELETE" => HandleDeleteRequest(path, context),
        "PATCH" => HandlePatchRequest(path, context),
        "HEAD" => HandleHeadRequest(path, context),
        "OPTIONS" => HandleOptionsRequest(path, context),
        _ => Results.StatusCode(405) // Method Not Allowed
    };
}

static IResult HandleGetRequest(string path, HttpContext context)
{
    return path.ToLower() switch
    {
        "" or "/" => Results.Ok(new { Message = "Hello World! Server is running.", Method = "GET", Path = "/" }),
        "api/users" => Results.Ok(new[]
        {
            new { Id = 1, Name = "Jan Kowalski" },
            new { Id = 2, Name = "Anna Nowak" }
        }),
        var p when p.StartsWith("api/users/") => HandleUserById(p),
        "api/status" => Results.Ok(new
        {
            Status = "OK",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        }),
        _ => Results.NotFound(new { Error = "Resource not found", Path = path })
    };
}

static IResult HandleUserById(string path)
{
    var segments = path.Split('/');
    if (segments.Length >= 3 && int.TryParse(segments[2], out int userId))
    {
        return Results.Ok(new { Id = userId, Name = $"User {userId}" });
    }
    return Results.BadRequest(new { Error = "Invalid user ID" });
}

static IResult HandlePostRequest(string path, HttpContext context)
{
    return path.ToLower() switch
    {
        "api/users" => Results.Created($"/api/users/{Random.Shared.Next(1000)}",
            new { Id = Random.Shared.Next(1000), Name = "New User", Created = DateTime.UtcNow }),
        _ => Results.NotFound(new { Error = "POST endpoint not found", Path = path })
    };
}

static IResult HandlePutRequest(string path, HttpContext context)
{
    return path.ToLower() switch
    {
        var p when p.StartsWith("api/users/") => Results.Ok(new { Message = "User updated", Path = path }),
        _ => Results.NotFound(new { Error = "PUT endpoint not found", Path = path })
    };
}

static IResult HandleDeleteRequest(string path, HttpContext context)
{
    return path.ToLower() switch
    {
        var p when p.StartsWith("api/users/") => Results.Ok(new { Message = "User deleted", Path = path }),
        _ => Results.NotFound(new { Error = "DELETE endpoint not found", Path = path })
    };
}

static IResult HandlePatchRequest(string path, HttpContext context)
{
    return Results.Ok(new { Message = "PATCH request processed", Path = path });
}

static IResult HandleHeadRequest(string path, HttpContext context)
{
    return Results.Ok();
}

static IResult HandleOptionsRequest(string path, HttpContext context)
{
    return Results.Ok(new
    {
        AllowedMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" },
        Path = path
    });
}

// Interface dla loggera
public interface IRequestLogger
{
    Task LogRequestAsync(HttpContext context, DateTime startTime, string httpMethod, string resourceUrl);
}

// Implementacja loggera do pliku
public class FileRequestLogger : IRequestLogger
{
    private readonly string _logFilePath;
    private readonly SemaphoreSlim _semaphore;

    public FileRequestLogger()
    {
        _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "access.log");
        _semaphore = new SemaphoreSlim(1, 1);

        // Utwórz plik loga jeśli nie istnieje
        if (!File.Exists(_logFilePath))
        {
            File.Create(_logFilePath).Dispose();
        }
    }

    public async Task LogRequestAsync(HttpContext context, DateTime startTime, string httpMethod, string resourceUrl)
    {
        var endTime = DateTime.UtcNow;
        var duration = (endTime - startTime).TotalMilliseconds;

        // Format rozszerzony z dodatkowymi informacjami
        var logEntry = FormatLogEntry(context, startTime, duration, httpMethod, resourceUrl);

        await _semaphore.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine, Encoding.UTF8);

            // Dodatkowe logowanie do konsoli dla debugowania
            Console.WriteLine($"[{startTime:HH:mm:ss}] {httpMethod} {resourceUrl} -> {context.Response.StatusCode} ({duration:F2}ms)");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static string FormatLogEntry(HttpContext context, DateTime timestamp, double durationMs, string httpMethod, string resourceUrl)
    {
        var response = context.Response;

        // Pobierz IP klienta
        var clientIp = GetClientIpAddress(context);

        // Pobierz User-Agent
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "-";

        // Format: IP - - [timestamp] "METHOD URL HTTP/1.1" status "user-agent" duration_ms
        var logEntry = string.Format(CultureInfo.InvariantCulture,
            "{0} - - [{1}] \"{2} {3} {4}\" {5} \"{6}\" {7:F2}ms",
            clientIp,
            timestamp.ToString("dd/MMM/yyyy:HH:mm:ss +0000", CultureInfo.InvariantCulture),
            httpMethod,
            resourceUrl,
            context.Request.Protocol,
            response.StatusCode,
            userAgent,
            durationMs
        );

        return logEntry;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Sprawdź różne źródła IP w kolejności preferencji
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ip))
        {
            return ip.Split(',')[0].Trim();
        }

        ip = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ip))
        {
            return ip;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}*/