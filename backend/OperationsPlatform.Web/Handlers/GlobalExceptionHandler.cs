using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NCalc.Exceptions;
using OperationsPlatform.Common.Exceptions;

namespace OperationsPlatform.Web.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        ProblemDetails problem = CreateProblem(exception);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Server-side failure on {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        await ProblemResponse.WriteAsync(context, problem, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblem(Exception exception)
    {
        if (exception is ValidationException validation)
        {
            return new ValidationProblemDetails(new ValidationResult(validation.Errors).ToDictionary())
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ProblemTitles.InvalidRequest
            };
        }

        if (exception is UserInputException)
        {
            return new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = exception.Message };
        }

        if (exception is NotFoundException)
        {
            return new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = exception.Message };
        }

        if (exception is ConflictException)
        {
            return new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = exception.Message };
        }

        if (exception is ExternalServiceException)
        {
            return new ProblemDetails { Status = StatusCodes.Status502BadGateway, Title = exception.Message };
        }

        if (exception is HttpRequestException or OperationCanceledException)
        {
            return new ProblemDetails { Status = StatusCodes.Status502BadGateway, Title = ProblemTitles.ExternalServiceUnavailable };
        }

        if (exception is NCalcException or JsonException)
        {
            return new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = ProblemTitles.InvalidDefinitionOrInput + exception.Message };
        }

        if (exception is BadHttpRequestException badRequest)
        {
            return new ProblemDetails { Status = badRequest.StatusCode, Title = ProblemTitles.InvalidRequest };
        }

        return new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = ProblemTitles.InternalError };
    }
}
