using Microsoft.AspNetCore.Mvc;

namespace OperationsPlatform.Web.Handlers;

public static class ProblemResponse
{
    public static async Task WriteAsync(HttpContext context, ProblemDetails problem, CancellationToken cancellationToken = default)
    {
        problem.Instance = context.Request.Path;

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), cancellationToken);
    }

    public static Task WriteAsync(HttpContext context, int status, string title) =>
        WriteAsync(context, new ProblemDetails { Status = status, Title = title });
}
