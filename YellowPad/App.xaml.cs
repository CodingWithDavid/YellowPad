using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YellowPad.Data;
using YellowPad.Services;

namespace YellowPad;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());
        services.AddWpfBlazorWebView();
        var databasePath = DatabasePathProvider.GetDatabasePath();
        services.AddDbContextFactory<YellowPadDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<ITodoService, TodoService>();
        services.AddSingleton<IVoiceCommandService, WindowsVoiceCommandService>();
        services.AddSingleton<WindowService>();
        services.AddSingleton<IWindowService>(provider => provider.GetRequiredService<WindowService>());

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        _services = services.BuildServiceProvider();
        try
        {
            await _services.GetRequiredService<DatabaseInitializer>().InitializeAsync();
        }
        catch (Exception exception)
        {
            _services.GetRequiredService<ILogger<App>>()
                .LogError(exception, "YellowPad started without an available database.");
        }

        var window = new MainWindow(_services);
        _services.GetRequiredService<WindowService>().Attach(window);
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
