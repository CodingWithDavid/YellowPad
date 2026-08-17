using Microsoft.EntityFrameworkCore;
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
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "YellowPad database initialization failed.");
            throw;
        }
    }
}
