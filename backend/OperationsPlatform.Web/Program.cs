using OperationsPlatform.Web.Extensions;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddPlatform(builder.Configuration);

WebApplication app = builder.Build();

app.UsePlatform();
app.Run();
