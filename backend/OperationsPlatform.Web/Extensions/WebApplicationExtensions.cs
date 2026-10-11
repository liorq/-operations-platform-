using Microsoft.EntityFrameworkCore;
using OperationsPlatform.Common.Constants;
using OperationsPlatform.Core.Data;
using OperationsPlatform.Web.Middleware;

namespace OperationsPlatform.Web.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Builds the request pipeline. The order matters: a middleware sees what the ones inside it produce.
    /// Static files -> HttpLogging -> ExceptionHandler -> AdminKey -> InputValidation -> controllers.
    /// </summary>
    public static WebApplication UsePlatform(this WebApplication app)
    {
        MigrateDatabase(app);

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseHttpLogging();
        app.UseExceptionHandler();
        app.UseMiddleware<AdminKeyMiddleware>();
        app.UseMiddleware<InputValidationMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        return app;
    }

    /// <summary>Code First: creates the database if it is missing and applies any migration it has not seen yet.</summary>
    private static void MigrateDatabase(WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        OperationsDbContext db = scope.ServiceProvider.GetRequiredService<OperationsDbContext>();

        db.Database.Migrate();
    }
}
