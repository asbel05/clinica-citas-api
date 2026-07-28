using Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Errors;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            return false;
        }

        if (httpContext.Response.HasStarted)
        {
            _logger.LogError(
                exception,
                "No se pudo generar una respuesta de error porque la respuesta ya había comenzado.");

            return false;
        }

        return exception switch
        {
            ValidationException validationException => await HandleValidationAsync(
                httpContext, 
                validationException, 
                cancellationToken),

            NotFoundException notFoundException => await HandleExpectedExceptionAsync(
                httpContext,
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                "RESOURCE_NOT_FOUND",
                notFoundException.Message,
                cancellationToken),

            BusinessRuleException businessRuleException => await HandleExpectedExceptionAsync(
                httpContext,
                StatusCodes.Status409Conflict,
                "Conflicto de negocio",
                "BUSINESS_RULE_VIOLATION",
                businessRuleException.Message,
                cancellationToken),

            _ => await HandleUnexpectedExceptionAsync(httpContext, exception, cancellationToken)
        };
    }

    private async ValueTask<bool> HandleValidationAsync(HttpContext httpContext, 
        ValidationException exception, CancellationToken cancellationToken)
    {
        var errors = exception.Errors
            .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName)
                ? "request"
                : error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        return await WriteProblemDetailsAsync(
            httpContext,
            StatusCodes.Status400BadRequest,
            "La solicitud contiene errores de validación.",
            "VALIDATION_ERROR",
            cancellationToken,
            errors);
    }

    private async ValueTask<bool> HandleExpectedExceptionAsync(HttpContext httpContext, int statusCode,
        string title, string code, string detail, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Error controlado en {Path}: {Code} - {Detail}",
            httpContext.Request.Path,
            code,
            detail);

        return await WriteProblemDetailsAsync(
            httpContext,
            statusCode,
            detail,
            code,
            cancellationToken,
            title: title);
    }

    private async ValueTask<bool> HandleUnexpectedExceptionAsync(HttpContext httpContext,
        Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Error no controlado en {Path}. TraceId: {TraceId}",
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        return await WriteProblemDetailsAsync(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Ocurrió un error interno al procesar la solicitud.",
            "INTERNAL_SERVER_ERROR",
            cancellationToken,
            title: "Error interno del servidor");
    }

    private async ValueTask<bool> WriteProblemDetailsAsync(HttpContext httpContext, int statusCode,
        string detail, string code, CancellationToken cancellationToken, object? errors = null, string? title = null)
    {
        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title ?? "Error en la solicitud",
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["code"] = code;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        await _problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });

        return true;
    }
}
