using System.Net.Http.Headers;
using System.Text;

namespace JobProcessing.Api.Middleware;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BasicAuthMiddleware> _logger;

    public BasicAuthMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<BasicAuthMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth for health endpoint
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        // Skip auth for SignalR negotiate (handled after connection)
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader != null && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var encodedCredentials = authHeader["Basic ".Length..].Trim();
                var decodedBytes = Convert.FromBase64String(encodedCredentials);
                var credentials = Encoding.UTF8.GetString(decodedBytes);
                var separatorIndex = credentials.IndexOf(':');

                if (separatorIndex > 0)
                {
                    var username = credentials[..separatorIndex];
                    var password = credentials[(separatorIndex + 1)..];

                    var validUsername = _configuration["BasicAuth:Username"];
                    var validPassword = _configuration["BasicAuth:Password"];

                    if (username == validUsername && password == validPassword)
                    {
                        _logger.LogInformation("Authenticated user: {Username}", username);
                        await _next(context);
                        return;
                    }
                }
            }
            catch (FormatException)
            {
                // Invalid base64 — fall through to 401
            }
        }

        _logger.LogWarning("Unauthorized access attempt from {IP}", 
            context.Connection.RemoteIpAddress);

        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Job Processing System\"";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }
}