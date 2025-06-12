using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LearnQuestV1.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. RequestPath: {RequestPath}", context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case KeyNotFoundException ex:
                    response.Message = ex.Message;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Title = "Resource Not Found";
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case InvalidOperationException ex:
                    response.Message = ex.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Title = "Invalid Operation";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case UnauthorizedAccessException ex:
                    response.Message = ex.Message.IsNullOrEmpty() ? "Access denied. You don't have permission to access this resource." : ex.Message;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Title = "Unauthorized Access";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case ArgumentException ex:
                    response.Message = ex.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Title = "Invalid Argument";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case TimeoutException ex:
                    response.Message = "The request timed out. Please try again later.";
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response.Title = "Request Timeout";
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;

                case DbUpdateException ex when ex.InnerException != null:
                    response.Message = "A database error occurred. Please check your data and try again.";
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Title = "Database Conflict";
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;

                    // Log the actual database error for debugging
                    _logger.LogError(ex, "Database update exception: {InnerException}", ex.InnerException.Message);
                    break;

                case FileNotFoundException ex:
                    response.Message = "The requested file was not found.";
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Title = "File Not Found";
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case DirectoryNotFoundException ex:
                    response.Message = "The requested directory was not found.";
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Title = "Directory Not Found";
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case TaskCanceledException ex when ex.InnerException is TimeoutException:
                    response.Message = "The request was cancelled due to timeout.";
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response.Title = "Request Cancelled";
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;

                case NotSupportedException ex:
                    response.Message = ex.Message;
                    response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    response.Title = "Operation Not Supported";
                    context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    break;

                // Application-specific exceptions
                case SecurityException ex:
                    response.Message = "A security violation occurred.";
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.Title = "Security Violation";
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;

                default:
                    response.Message = _environment.IsDevelopment()
                        ? $"An error occurred: {exception.Message}"
                        : "An internal server error occurred. Please try again later.";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Title = "Internal Server Error";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // Add additional details in development environment
            if (_environment.IsDevelopment())
            {
                response.Details = new
                {
                    ExceptionType = exception.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Source = exception.Source,
                    InnerException = exception.InnerException?.Message
                };
            }

            // Add request information
            response.Instance = context.Request.Path;
            response.TraceId = context.TraceIdentifier;
            response.Timestamp = DateTime.UtcNow;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Instance { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public object? Details { get; set; }
    }

    // Custom exception for security-related issues
    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
        public SecurityException(string message, Exception innerException) : base(message, innerException) { }
    }

    // Extension method to register the middleware
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }

    // Extension method for string null/empty check
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}