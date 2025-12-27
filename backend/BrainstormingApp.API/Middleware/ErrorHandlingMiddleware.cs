using System.Net;
using System.Text.Json;
using BrainstormingApp.Application.Common;
using BrainstormingApp.Application.Common.Exceptions;

namespace BrainstormingApp.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, notFoundEx.Message, new List<string>()),

            Application.Common.Exceptions.ValidationException validationEx =>
                (HttpStatusCode.BadRequest, validationEx.Message, validationEx.Errors),

            FluentValidation.ValidationException fluentValidationEx =>
                (HttpStatusCode.BadRequest, "Validation failed",
                    fluentValidationEx.Errors.Select(e => e.ErrorMessage).ToList()),

            BusinessRuleException businessEx =>
                (HttpStatusCode.UnprocessableEntity, businessEx.Message, new List<string>()),

            UnauthorizedException unauthorizedEx =>
                (HttpStatusCode.Unauthorized, unauthorizedEx.Message, new List<string>()),

            ConflictException conflictEx =>
                (HttpStatusCode.Conflict, conflictEx.Message, new List<string>()),

            UnauthorizedAccessException =>
                (HttpStatusCode.Forbidden, "You do not have permission to perform this action", new List<string>()),

            KeyNotFoundException keyNotFoundEx =>
                (HttpStatusCode.NotFound, keyNotFoundEx.Message, new List<string>()),

            ArgumentException argEx =>
                (HttpStatusCode.BadRequest, argEx.Message, new List<string>()),

            InvalidOperationException invalidOpEx =>
                (HttpStatusCode.BadRequest, invalidOpEx.Message, new List<string>()),

            _ => (HttpStatusCode.InternalServerError,
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                new List<string>())
        };

        // Log based on severity
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }
        else if (statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.UnprocessableEntity)
        {
            _logger.LogWarning("Client error occurred: {Message}", message);
        }
        else
        {
            _logger.LogInformation("Expected exception occurred: {Message}", message);
        }

        response.StatusCode = (int)statusCode;

        var errorResponse = new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
