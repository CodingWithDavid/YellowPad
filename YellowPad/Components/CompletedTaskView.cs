using YellowPad.Models;

namespace YellowPad.Components;

public static class CompletedTaskView
{
    public static IReadOnlyList<TodoItem> Select(IReadOnlyList<TodoItem> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        return tasks.Where(task => task.IsCompleted).ToList();
    }

    public static string FormatCompletedDate(DateTime? completedUtc, TimeZoneInfo? timeZone = null)
    {
        if (completedUtc is null)
        {
            return "Date unavailable";
        }

        var utcValue = DateTime.SpecifyKind(completedUtc.Value, DateTimeKind.Utc);
        var localValue = TimeZoneInfo.ConvertTimeFromUtc(utcValue, timeZone ?? TimeZoneInfo.Local);
        return localValue.ToString("MMM d, yyyy");
    }
}
