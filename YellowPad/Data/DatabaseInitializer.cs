using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace YellowPad.Data;

public sealed class DatabaseInitializer(
    IDbContextFactory<YellowPadDbContext> contextFactory,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
            await context.Database.OpenConnectionAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.Transaction = transaction.GetDbTransaction();
            command.CommandText = "PRAGMA table_info(\"TodoItems\")";
            var hasColor = false;
            await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (string.Equals(reader.GetString(1), "Color", StringComparison.OrdinalIgnoreCase))
                    {
                        hasColor = true;
                        break;
                    }
                }
            }

            if (!hasColor)
            {
                command.CommandText = "ALTER TABLE \"TodoItems\" ADD COLUMN \"Color\" INTEGER NOT NULL DEFAULT 0";
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "YellowPad database initialization failed.");
            throw;
        }
    }
}
