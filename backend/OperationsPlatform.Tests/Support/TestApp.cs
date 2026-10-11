using OperationsPlatform.Core.Executors;
using OperationsPlatform.Core.Services.Calculation;
using OperationsPlatform.Core.Services.Operations;
using OperationsPlatform.Core.Services.Resolving;
using OperationsPlatform.Core.Data;
using OperationsPlatform.Core.Validators;
using Microsoft.EntityFrameworkCore;

namespace OperationsPlatform.Tests.Support;

/// <summary>
/// The real services on a real SQL Server database, built by the real migration.
/// Each test creates its own database, so tests never affect each other.
/// </summary>
public sealed class TestApp : IDisposable
{
    private readonly TestDatabase _database = new TestDatabase();

    public TestApp()
    {
        Db = _database.CreateContext();
        Db.Database.Migrate();

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
        _database.Dispose();
    }
}
