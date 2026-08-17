using System.Windows;

namespace YellowPad.Services;

public static class WindowStateTransition
{
    public static WindowState ToggleMaximize(WindowState currentState) =>
        currentState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
}
