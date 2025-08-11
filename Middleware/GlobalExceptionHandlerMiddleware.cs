using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace K8Intel.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Create a standardized error response, ALWAYS including full details for this diagnostic session.
                var response = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "An internal server error has occurred.",
                    // This new detailed error will give us the real reason for the failure.
                    ErrorDetails = new
                    {
                        ExceptionType = ex.GetType().Name,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.ToString(),
                        InnerException = GetInnerExceptionDetails(ex) // Recursive helper
                    }
                };

                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
                await context.Response.WriteAsync(jsonResponse);
            }
        }

        // Helper function to recursively get all inner exceptions.
        private object? GetInnerExceptionDetails(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return null;
            }

            return new
            {
                ExceptionType = ex.InnerException.GetType().Name,
                ErrorMessage = ex.InnerException.Message,
                StackTrace = ex.InnerException.ToString(),
                InnerException = GetInnerExceptionDetails(ex.InnerException)
            };
        }
    }
}