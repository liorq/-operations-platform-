using OperationsPlatform.Web.Extensions;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Serilog is set up in the "Serilog" section of appsettings.json: errors-.log gets every Error and above,
// http-.log gets every request with its response and every request our middleware rejected.
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddPlatform(builder.Configuration);

WebApplication app = builder.Build();

app.UsePlatform();
app.Run();
