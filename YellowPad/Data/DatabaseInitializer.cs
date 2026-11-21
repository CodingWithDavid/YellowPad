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
            if (!await TableExistsAsync(command, "TodoTabs", cancellationToken))
            {
                command.CommandText = """
                    CREATE TABLE "TodoTabs" (
                        "Id" INTEGER NOT NULL CONSTRAINT "PK_TodoTabs" PRIMARY KEY AUTOINCREMENT,
                        "Name" TEXT NOT NULL,
                        "CreatedUtc" TEXT NOT NULL,
                        "SortOrder" INTEGER NOT NULL
                    );
                    """;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            command.CommandText = """
                INSERT INTO "TodoTabs" ("Name", "CreatedUtc", "SortOrder")
                SELECT 'Today', @createdUtc, 0
                WHERE NOT EXISTS (SELECT 1 FROM "TodoTabs");
                """;
            var createdParameter = command.CreateParameter();
            createdParameter.ParameterName = "@createdUtc";
            createdParameter.Value = DateTime.UtcNow;
            command.Parameters.Add(createdParameter);
            await command.ExecuteNonQueryAsync(cancellationToken);
            command.Parameters.Clear();

            if (!await ColumnExistsAsync(command, "TodoItems", "TabId", cancellationToken))
            {
                command.CommandText = "ALTER TABLE \"TodoItems\" ADD COLUMN \"TabId\" INTEGER NOT NULL DEFAULT 1";
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            if (!await ColumnExistsAsync(command, "TodoItems", "Color", cancellationToken))
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

    private static async Task<bool> TableExistsAsync(
        System.Data.Common.DbCommand command,
        string tableName,
        CancellationToken cancellationToken)
    {
        command.CommandText = "SELECT 1 FROM \"sqlite_master\" WHERE \"type\" = 'table' AND \"name\" = @name LIMIT 1";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        command.Parameters.Clear();
        return result is not null;
    }

    private static async Task<bool> ColumnExistsAsync(
        System.Data.Common.DbCommand command,
        string tableName,
        string columnName,
        CancellationToken cancellationToken)
    {
        command.CommandText = $"PRAGMA table_info(\"{tableName}\")";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
