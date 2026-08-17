using System.Windows;
using YellowPad.Services;

namespace YellowPad.Tests;

public sealed class WindowStateTransitionTests
{
    [Fact]
    public void ToggleMaximize_MaximizesANormalWindow()
    {
        var nextState = WindowStateTransition.ToggleMaximize(WindowState.Normal);

        Assert.Equal(WindowState.Maximized, nextState);
    }

    [Fact]
    public void ToggleMaximize_RestoresAMaximizedWindow()
    {
        var nextState = WindowStateTransition.ToggleMaximize(WindowState.Maximized);

        Assert.Equal(WindowState.Normal, nextState);
    }
}
