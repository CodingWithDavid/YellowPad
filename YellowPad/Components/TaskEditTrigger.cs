namespace YellowPad.Components;

public static class TaskEditTrigger
{
    public static bool ShouldStartEditing(long consecutiveClickCount) => consecutiveClickCount >= 2;
}
