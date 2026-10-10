using Microsoft.AspNetCore.Mvc;

namespace OperationsPlatform.Web.Handlers;

/// <summary>Writes the short error answer (a ProblemDetails with a status and a title) that the middlewares give.</summary>
public static class ProblemResponse
{
    public static async Task WriteAsync(HttpContext context, int status, string title)
    {
        ProblemDetails problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem);
    }
}
