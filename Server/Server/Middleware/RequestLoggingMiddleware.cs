using System.Diagnostics;

namespace Server.Middleware
{
    /// <summary>
    /// Logs every incoming HTTP request (method, path, status code and elapsed time)
    /// and acts as a safety net for any unhandled exception that escapes a controller,
    /// logging it and returning a generic 500 response.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            HttpRequest request = context.Request;
            string method = request.Method;
            string path = request.Path + request.QueryString;
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            try
            {
                await _next(context);
                stopwatch.Stop();

                int status = context.Response.StatusCode;
                string message = "HTTP {Method} {Path} responded {StatusCode} in {Elapsed} ms (client: {ClientIp})";

                if (status >= 500)
                    _logger.LogError(message, method, path, status, stopwatch.ElapsedMilliseconds, clientIp);
                else if (status >= 400)
                    _logger.LogWarning(message, method, path, status, stopwatch.ElapsedMilliseconds, clientIp);
                else
                    _logger.LogInformation(message, method, path, status, stopwatch.ElapsedMilliseconds, clientIp);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "Unhandled exception during HTTP {Method} {Path} after {Elapsed} ms (client: {ClientIp})",
                    method, path, stopwatch.ElapsedMilliseconds, clientIp);

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"An unexpected error occurred.\"}");
                }
            }
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder) =>
            builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
