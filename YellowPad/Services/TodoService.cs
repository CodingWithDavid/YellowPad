using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YellowPad.Data;
using YellowPad.Models;

namespace YellowPad.Services;

public sealed class TodoService(
    IDbContextFactory<YellowPadDbContext> contextFactory,
    ILogger<TodoService> logger) : ITodoService
{
    public Task<IReadOnlyList<TodoItem>> GetTasksAsync() => ExecuteAsync(
        async () =>
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            return (IReadOnlyList<TodoItem>)await context.TodoItems
                .AsNoTracking()
                .OrderBy(item => item.IsCompleted)
                .ThenBy(item => item.SortOrder)
                .ThenBy(item => item.CreatedUtc)
                .ToListAsync();
        },
        "load tasks");

    public Task<TodoItem> AddTaskAsync(string title)
    {
        var trimmedTitle = NormalizeTitle(title);
        return ExecuteAsync(async () =>
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var highestSortOrder = await context.TodoItems
                .Select(item => (int?)item.SortOrder)
                .MaxAsync() ?? -1;
            var item = new TodoItem
            {
                Title = trimmedTitle,
                CreatedUtc = DateTime.UtcNow,
                SortOrder = highestSortOrder + 1
            };

            context.TodoItems.Add(item);
            await context.SaveChangesAsync();
            return item;
        }, "add a task");
    }

    public Task UpdateTaskAsync(TodoItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var trimmedTitle = NormalizeTitle(item.Title);
        return ExecuteAsync(async () =>
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var stored = await FindTaskAsync(context, item.Id);
            stored.Title = trimmedTitle;
            stored.UpdatedUtc = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }, "update a task");
    }

    public Task ToggleCompletedAsync(int id) => ExecuteAsync(async () =>
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var item = await FindTaskAsync(context, id);
        item.IsCompleted = !item.IsCompleted;
        item.CompletedUtc = item.IsCompleted ? DateTime.UtcNow : null;
        item.UpdatedUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }, "toggle a task");

    public Task DeleteTaskAsync(int id) => ExecuteAsync(async () =>
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var item = await context.TodoItems.FindAsync(id);
        if (item is not null)
        {
            context.TodoItems.Remove(item);
            await context.SaveChangesAsync();
        }
    }, "delete a task");

    public Task DeleteCompletedAsync() => ExecuteAsync(async () =>
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.TodoItems.Where(item => item.IsCompleted).ExecuteDeleteAsync();
    }, "delete completed tasks");

    public Task DeleteAllAsync() => ExecuteAsync(async () =>
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.TodoItems.ExecuteDeleteAsync();
    }, "delete all tasks");

    private static string NormalizeTitle(string title)
    {
        var trimmed = title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("A task title is required.", nameof(title));
        }

        return trimmed;
    }

    private static async Task<TodoItem> FindTaskAsync(YellowPadDbContext context, int id) =>
        await context.TodoItems.FindAsync(id)
        ?? throw new KeyNotFoundException($"Task {id} was not found.");

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

    private async Task ExecuteAsync(Func<Task> action, string operation)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to {Operation}.", operation);
            throw;
        }
    }
}
