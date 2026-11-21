using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YellowPad.Data;
using YellowPad.Models;

namespace YellowPad.Services;

public sealed class TabService(
    IDbContextFactory<YellowPadDbContext> contextFactory,
    ILogger<TabService> logger) : ITabService
{
    public Task<IReadOnlyList<TodoTab>> GetTabsAsync() => ExecuteAsync(
        async () =>
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return (IReadOnlyList<TodoTab>)await context.TodoTabs
                .AsNoTracking()
                .OrderBy(tab => tab.SortOrder)
                .ThenBy(tab => tab.CreatedUtc)
                .ToListAsync();
        },
        "load tabs");

    public Task<TodoTab> AddTabAsync(string name)
    {
        var trimmedName = NormalizeName(name);
        return ExecuteAsync(async () =>
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var highestSortOrder = await context.TodoTabs
                .Select(tab => (int?)tab.SortOrder)
                .MaxAsync() ?? -1;
            var tab = new TodoTab
            {
                Name = trimmedName,
                CreatedUtc = DateTime.UtcNow,
                SortOrder = highestSortOrder + 1
            };

            context.TodoTabs.Add(tab);
            await context.SaveChangesAsync();
            return tab;
        }, "add a tab");
    }

    private static string NormalizeName(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("A tab name is required.", nameof(name));
        }

        return trimmed;
    }

    private async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string operation)
    {
        try
        {
            return await action();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to {Operation}.", operation);
            throw;
        }
    }
}
