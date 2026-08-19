using YellowPad.Models;

namespace YellowPad.Components;

public static class ActiveTaskView
{
    public static IReadOnlyList<TodoItem> Select(
        IReadOnlyList<TodoItem> tasks,
        IReadOnlySet<int> hiddenCompletedTaskIds)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        ArgumentNullException.ThrowIfNull(hiddenCompletedTaskIds);
        return tasks
            .Where(task => !task.IsCompleted || !hiddenCompletedTaskIds.Contains(task.Id))
            .ToList();
    }
}
