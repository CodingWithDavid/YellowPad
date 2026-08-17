using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using YellowPad.Data;
using YellowPad.Services;

namespace YellowPad.Tests;

public sealed class TodoServiceTests
{
    [Fact]
    public async Task AddTaskAsync_TrimsTitleAndPersistsUtcTimestamp()
    {
        await using var database = await TestDatabase.CreateAsync();

        var added = await database.Service.AddTaskAsync("  Pick up groceries  ");
        var stored = Assert.Single(await database.Service.GetTasksAsync());

        Assert.Equal("Pick up groceries", added.Title);
        Assert.Equal(added.Id, stored.Id);
        Assert.Equal(DateTimeKind.Utc, stored.CreatedUtc.Kind);
        Assert.False(stored.IsCompleted);
    }

    [Fact]
    public async Task AddTaskAsync_RejectsWhitespaceOnlyTitles()
    {
        await using var database = await TestDatabase.CreateAsync();

        await Assert.ThrowsAsync<ArgumentException>(() => database.Service.AddTaskAsync("   "));
        Assert.Empty(await database.Service.GetTasksAsync());
    }

    [Fact]
    public async Task GetTasksAsync_ListsActiveBeforeCompletedAndKeepsCreationOrder()
    {
        await using var database = await TestDatabase.CreateAsync();
        var first = await database.Service.AddTaskAsync("First");
        var second = await database.Service.AddTaskAsync("Second");
        await database.Service.ToggleCompletedAsync(first.Id);

        var tasks = await database.Service.GetTasksAsync();

        Assert.Equal([second.Id, first.Id], tasks.Select(task => task.Id));
    }

    [Fact]
    public async Task ToggleCompletedAsync_SetsAndClearsCompletedUtc()
    {
        await using var database = await TestDatabase.CreateAsync();
        var item = await database.Service.AddTaskAsync("Write article");

        await database.Service.ToggleCompletedAsync(item.Id);
        var completed = Assert.Single(await database.Service.GetTasksAsync());
        Assert.True(completed.IsCompleted);
        Assert.Equal(DateTimeKind.Utc, completed.CompletedUtc?.Kind);

        await database.Service.ToggleCompletedAsync(item.Id);
        var active = Assert.Single(await database.Service.GetTasksAsync());
        Assert.False(active.IsCompleted);
        Assert.Null(active.CompletedUtc);
    }

    [Fact]
    public async Task UpdateTaskAsync_TrimsTitleAndPersistsAcrossContextFactories()
    {
        var path = TestDatabase.CreatePath();
        await using (var firstSession = await TestDatabase.CreateAsync(path, deleteOnDispose: false))
        {
            var item = await firstSession.Service.AddTaskAsync("Call John");
            item.Title = "  Call Jane  ";
            await firstSession.Service.UpdateTaskAsync(item);
        }

        await using var reopened = await TestDatabase.CreateAsync(path);
        var stored = Assert.Single(await reopened.Service.GetTasksAsync());
        Assert.Equal("Call Jane", stored.Title);
        Assert.Equal(DateTimeKind.Utc, stored.UpdatedUtc?.Kind);
    }

    [Fact]
    public async Task DeleteOperations_RemoveTheRequestedTasks()
    {
        await using var database = await TestDatabase.CreateAsync();
        var active = await database.Service.AddTaskAsync("Active");
        var completed = await database.Service.AddTaskAsync("Completed");
        await database.Service.ToggleCompletedAsync(completed.Id);

        await database.Service.DeleteCompletedAsync();
        Assert.Equal(active.Id, Assert.Single(await database.Service.GetTasksAsync()).Id);

        await database.Service.DeleteAllAsync();
        Assert.Empty(await database.Service.GetTasksAsync());
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly IDbContextFactory<YellowPadDbContext> _factory;
        private readonly bool _deleteOnDispose;

        private TestDatabase(string path, bool deleteOnDispose)
        {
            Path = path;
            _deleteOnDispose = deleteOnDispose;
            var options = new DbContextOptionsBuilder<YellowPadDbContext>()
                .UseSqlite($"Data Source={path};Pooling=False")
                .Options;
            _factory = new TestDbContextFactory(options);
            Service = new TodoService(_factory, NullLogger<TodoService>.Instance);
        }

        public string Path { get; }
        public TodoService Service { get; }

        public static string CreatePath() =>
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"yellowpad-tests-{Guid.NewGuid():N}.db");

        public static async Task<TestDatabase> CreateAsync(string? path = null, bool deleteOnDispose = true)
        {
            var database = new TestDatabase(path ?? CreatePath(), deleteOnDispose);
            var initializer = new DatabaseInitializer(
                database._factory,
                NullLogger<DatabaseInitializer>.Instance);
            await initializer.InitializeAsync();
            return database;
        }

        public ValueTask DisposeAsync()
        {
            if (_deleteOnDispose)
            {
                DeleteIfPresent(Path);
                DeleteIfPresent($"{Path}-shm");
                DeleteIfPresent($"{Path}-wal");
            }

            return ValueTask.CompletedTask;
        }

        private static void DeleteIfPresent(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private sealed class TestDbContextFactory(DbContextOptions<YellowPadDbContext> options)
            : IDbContextFactory<YellowPadDbContext>
        {
            public YellowPadDbContext CreateDbContext() => new(options);

            public Task<YellowPadDbContext> CreateDbContextAsync(
                CancellationToken cancellationToken = default) =>
                Task.FromResult(CreateDbContext());
        }
    }
}
