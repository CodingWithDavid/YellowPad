using YellowPad.Components;

namespace YellowPad.Tests;

public sealed class TodoSummaryTests
{
    [Theory]
    [InlineData(0, 0, "Nothing on the pad")]
    [InlineData(1, 0, "1 task remaining")]
    [InlineData(2, 0, "2 tasks remaining")]
    [InlineData(3, 1, "3 tasks • 1 completed")]
    [InlineData(2, 2, "2 tasks • 2 completed")]
    public void Format_DescribesTaskCounts(int total, int completed, string expected)
    {
        Assert.Equal(expected, TodoSummary.Format(total, completed));
    }
}
