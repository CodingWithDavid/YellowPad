using System.Windows;

namespace YellowPad.Services;

public sealed class WindowService : IWindowService
{
    private Window? _window;

    public void Attach(Window window) => _window = window;

    public void Minimize() => RunOnUiThread(window => window.WindowState = WindowState.Minimized);

    public void ToggleMaximize() => RunOnUiThread(window =>
        window.WindowState = WindowStateTransition.ToggleMaximize(window.WindowState));

    public void Close() => RunOnUiThread(window => window.Close());

    private void RunOnUiThread(Action<Window> action)
    {
        var window = _window;
        if (window is null)
        {
            return;
        }

        if (window.Dispatcher.CheckAccess())
        {
            action(window);
            return;
        }

        window.Dispatcher.Invoke(() => action(window));
    }
}
