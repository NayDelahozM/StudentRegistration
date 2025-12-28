using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StudentRegistration.Domain.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentRegistration.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
	        private sealed record ErrorResponse(int StatusCode, string Message, IEnumerable<string> Errors);

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

	            var response = exception switch
	            {
	                BusinessException businessEx => new ErrorResponse(
	                    (int)HttpStatusCode.BadRequest,
	                    businessEx.Message,
	                    businessEx.Errors
	                ),
	                KeyNotFoundException => new ErrorResponse(
	                    (int)HttpStatusCode.NotFound,
	                    exception.Message,
	                    new[] { exception.Message }
	                ),
	                UnauthorizedAccessException => new ErrorResponse(
	                    (int)HttpStatusCode.Unauthorized,
	                    "No autorizado",
	                    new[] { exception.Message }
	                ),
	                _ => new ErrorResponse(
	                    (int)HttpStatusCode.InternalServerError,
	                    "Ha ocurrido un error interno",
	                    new[] { "Por favor contacte al administrador" }
	                )
	            };

	            context.Response.StatusCode = response.StatusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

	            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
