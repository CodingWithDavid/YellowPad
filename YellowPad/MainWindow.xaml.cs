using System.Windows;
using Microsoft.AspNetCore.Components.WebView;

namespace YellowPad;

public partial class MainWindow : Window
{
    public MainWindow(IServiceProvider services)
    {
        Resources.Add("services", services);
        InitializeComponent();
    }

    private void OnBlazorWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs args)
    {
        var webView = args.WebView.CoreWebView2;
        webView.Settings.AreDefaultContextMenusEnabled = true;
        webView.ContextMenuRequested += (_, eventArgs) =>
        {
            if (!eventArgs.ContextMenuTarget.IsEditable)
            {
                eventArgs.Handled = true;
            }
        };
    }
}
