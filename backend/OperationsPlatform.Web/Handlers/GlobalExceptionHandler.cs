using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NCalc.Exceptions;
using OperationsPlatform.Common.Exceptions;

namespace OperationsPlatform.Web.Handlers;

/// <summary>
/// Turns every exception into an answer. A validation failure, a user mistake or a malformed request becomes 400.
/// Anything else becomes 500 and an Error entry in errors-.log.
/// The caller always gets the same ProblemDetails shape and never a stack trace.
/// </summary>
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
            // The caller disconnected, so there is nobody to answer.
            return true;
        }

        ProblemDetails problem = CreateProblem(exception);
        problem.Instance = context.Request.Path;

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled error on {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), cancellationToken);

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
