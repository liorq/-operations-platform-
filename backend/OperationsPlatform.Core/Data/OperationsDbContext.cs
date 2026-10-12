using OperationsPlatform.Common.Constants;
using OperationsPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OperationsPlatform.Core.Data;

public class OperationsDbContext : DbContext
{
    public OperationsDbContext(DbContextOptions<OperationsDbContext> options) : base(options)
    {
    }

    public DbSet<Operation> Operations { get; set; } = null!;

    public DbSet<OperationLog> OperationLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Operation>(builder =>
        {
            builder.HasIndex(nameof(Operation.Key)).IsUnique();

            builder.Property(nameof(Operation.Key)).HasMaxLength(InputLimits.MaxKeyLength).IsRequired();
            builder.Property(nameof(Operation.Name)).HasMaxLength(InputLimits.MaxNameLength).IsRequired();
            builder.Property(nameof(Operation.Description)).HasMaxLength(InputLimits.MaxDescriptionLength);
            builder.Property(nameof(Operation.Definition)).HasMaxLength(InputLimits.MaxDefinitionLength).IsRequired();

            builder.Property(nameof(Operation.Kind)).HasConversion<string>().HasMaxLength(InputLimits.MaxEnumTextLength);
            builder.Property(nameof(Operation.InputType)).HasConversion<string>().HasMaxLength(InputLimits.MaxEnumTextLength);

            builder.HasData(OperationSeed.Operations);
        });

        modelBuilder.Entity<OperationLog>(builder =>
        {
            builder.Property(nameof(OperationLog.A)).HasMaxLength(InputLimits.MaxInputLength);
            builder.Property(nameof(OperationLog.B)).HasMaxLength(InputLimits.MaxInputLength);
            builder.Property(nameof(OperationLog.Result)).HasMaxLength(InputLimits.MaxResultLength);

            builder.Property<DateTime>(nameof(OperationLog.CreatedAtUtc)).HasConversion(stored => stored, loaded => DateTime.SpecifyKind(loaded, DateTimeKind.Utc));

            builder.HasIndex(nameof(OperationLog.OperationId), nameof(OperationLog.CreatedAtUtc));

            builder.HasOne(nameof(OperationLog.Operation))
                .WithMany()
                .HasForeignKey(nameof(OperationLog.OperationId))
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
