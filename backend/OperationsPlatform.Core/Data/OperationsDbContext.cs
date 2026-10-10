using OperationsPlatform.Common.Constants;
using OperationsPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OperationsPlatform.Core.Data;

/// <summary>
/// The database, defined Code First: the tables come from these entities and the layout in OnModelCreating,
/// and EF migrations turn them into SQL.
/// </summary>
public class OperationsDbContext : DbContext
{
    public OperationsDbContext(DbContextOptions<OperationsDbContext> options) : base(options)
    {
    }

    public DbSet<Operation> Operations { get; set; } = null!;

    public DbSet<OperationLog> OperationLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Operations, including the initial rows.
        modelBuilder.Entity<Operation>(builder =>
        {
            builder.HasIndex(nameof(Operation.Key)).IsUnique();

            builder.Property(nameof(Operation.Key)).HasMaxLength(InputLimits.MaxKeyLength).IsRequired();
            builder.Property(nameof(Operation.Name)).HasMaxLength(InputLimits.MaxNameLength).IsRequired();
            builder.Property(nameof(Operation.Description)).HasMaxLength(InputLimits.MaxDescriptionLength);

            // Enums are stored as readable text instead of numbers.
            builder.Property(nameof(Operation.Kind)).HasConversion<string>().HasMaxLength(InputLimits.MaxEnumTextLength);
            builder.Property(nameof(Operation.InputType)).HasConversion<string>().HasMaxLength(InputLimits.MaxEnumTextLength);

            builder.HasData(OperationSeed.Operations);
        });

        // OperationLogs.
        modelBuilder.Entity<OperationLog>(builder =>
        {
            builder.Property(nameof(OperationLog.A)).HasMaxLength(InputLimits.MaxInputLength);
            builder.Property(nameof(OperationLog.B)).HasMaxLength(InputLimits.MaxInputLength);
            builder.Property(nameof(OperationLog.Result)).HasMaxLength(InputLimits.MaxResultLength);

            // One index serves both history queries: the latest runs of an operation, and the count since the start of the month.
            builder.HasIndex(nameof(OperationLog.OperationId), nameof(OperationLog.CreatedAtUtc));

            builder.HasOne(nameof(OperationLog.Operation))
                .WithMany()
                .HasForeignKey(nameof(OperationLog.OperationId))
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
