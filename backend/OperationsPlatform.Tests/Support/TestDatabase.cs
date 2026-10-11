using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OperationsPlatform.Core.Data;

namespace OperationsPlatform.Tests.Support;

/// <summary>
/// A throwaway database on a real SQL Server. The server comes from the TEST_SQLSERVER environment variable
/// (a connection string without a database name), or SQL Server Express on this machine. The database is deleted on dispose.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private const string ServerVariable = "TEST_SQLSERVER";
    private const string LocalExpress = "Server=.\\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True";

    public TestDatabase()
    {
        SqlConnectionStringBuilder connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable(ServerVariable) ?? LocalExpress)
        {
            InitialCatalog = $"OperationsPlatformTests_{Guid.NewGuid():N}"
        };
        ConnectionString = connection.ConnectionString;
    }

    public string ConnectionString { get; }

    public OperationsDbContext CreateContext() =>
        new OperationsDbContext(new DbContextOptionsBuilder<OperationsDbContext>().UseSqlServer(ConnectionString).Options);

    public void Dispose()
    {
        using OperationsDbContext db = CreateContext();
        db.Database.EnsureDeleted();
    }
}
