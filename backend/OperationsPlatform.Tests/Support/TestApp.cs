using OperationsPlatform.Core.Executors;
using OperationsPlatform.Core.Services.Calculation;
using OperationsPlatform.Core.Services.Operations;
using OperationsPlatform.Core.Services.Resolving;
using OperationsPlatform.Core.Data;
using OperationsPlatform.Core.Validators;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace OperationsPlatform.Tests.Support;

/// <summary>
/// The real services on a real EF Core database (SQLite in memory, built from the model and the seed).
/// Each test creates its own, so tests never affect each other.
/// </summary>
public sealed class TestApp : IDisposable
{
    private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");

    public TestApp()
    {
        _connection.Open();

        DbContextOptions<OperationsDbContext> options = new DbContextOptionsBuilder<OperationsDbContext>().UseSqlite(_connection).Options;
        Db = new OperationsDbContext(options);
        Db.Database.EnsureCreated();

        Clock = new FakeClock(new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero));

        // The HTTP executor is only used for validation in tests, so no real client factory is needed.
        HttpExecutorOptions httpOptions = new HttpExecutorOptions(new[] { "api.open-meteo.com" });
        HttpExecutor httpExecutor = new HttpExecutor(null!, httpOptions);
        ExecutorResolver executors = new ExecutorResolver(new IOperationExecutor[] { new ExpressionExecutor(), new TextExecutor(), httpExecutor });

        OperationRepository operationRepository = new OperationRepository(Db);
        OperationLogRepository logRepository = new OperationLogRepository(Db);

        Calculation = new CalculationService(operationRepository, logRepository, executors, new CalculateRequestValidator(), Clock);
        Operations = new OperationService(operationRepository, executors, new OperationDtoValidator());
    }

    public OperationsDbContext Db { get; }

    public FakeClock Clock { get; }

    public ICalculationService Calculation { get; }

    public IOperationService Operations { get; }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
