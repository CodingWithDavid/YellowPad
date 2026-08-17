namespace YellowPad.Components;

public static class TodoSummary
{
    public static string Format(int total, int completed)
    {
        if (total == 0)
        {
            return "Nothing on the pad";
        }

        if (completed == 0)
        {
            var taskLabel = total == 1 ? "task" : "tasks";
            return $"{total} {taskLabel} remaining";
        }

        var totalLabel = total == 1 ? "task" : "tasks";
        return $"{total} {totalLabel} • {completed} completed";
    }
}
