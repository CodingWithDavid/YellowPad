using YellowPad.Components;
using YellowPad.Models;

namespace YellowPad.Tests;

public sealed class CompletedTaskViewTests
{
    [Fact]
    public void Select_OnlyReturnsCompletedTasksInTheirCurrentOrder()
    {
        var tasks = new TodoItem[]
        {
            new() { Id = 1, Title = "Still active" },
            new() { Id = 2, Title = "Finished first", IsCompleted = true },
            new() { Id = 3, Title = "Finished second", IsCompleted = true }
        };

        var completed = CompletedTaskView.Select(tasks);

        Assert.Equal([2, 3], completed.Select(task => task.Id));
    }

    [Fact]
    public void FormatCompletedDate_UsesTheCompletionDateInTheRequestedTimeZone()
    {
        var completedUtc = new DateTime(2026, 8, 17, 1, 30, 0, DateTimeKind.Utc);
        var arizona = TimeZoneInfo.CreateCustomTimeZone(
            "Arizona test time",
            TimeSpan.FromHours(-7),
            "Arizona test time",
            "Arizona test time");

        var formatted = CompletedTaskView.FormatCompletedDate(completedUtc, arizona);

        Assert.Equal(new DateTime(2026, 8, 16).ToString("MMM d, yyyy"), formatted);
    }

    [Fact]
    public void FormatCompletedDate_HandlesOlderTasksWithoutACompletionDate()
    {
        var formatted = CompletedTaskView.FormatCompletedDate(null, TimeZoneInfo.Utc);

        Assert.Equal("Date unavailable", formatted);
    }
}
