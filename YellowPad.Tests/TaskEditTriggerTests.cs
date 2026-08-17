using YellowPad.Components;

namespace YellowPad.Tests;

public sealed class TaskEditTriggerTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void ShouldStartEditing_RequiresAtLeastTwoConsecutiveClicks(long clickCount, bool expected)
    {
        Assert.Equal(expected, TaskEditTrigger.ShouldStartEditing(clickCount));
    }
}
