using NotionFlow.App.Services;
using System.Diagnostics;

namespace NotionFlow.App;

public partial class App : Application
{
    private LocalDataService _localDataService = new();

    public App(AppShell shell)
    {
        InitializeComponent();
        MainPage = shell;

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            CrashLog.Write("AppDomain.UnhandledException", e.ExceptionObject);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (MainPage?.Window?.Page is Page page)
                    await page.DisplayAlert("Error Fatal",
                        $"La aplicación encontró un error: {e.ExceptionObject}", "OK");
            });
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            CrashLog.Write("TaskScheduler.UnobservedTaskException", e.Exception);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (MainPage?.Window?.Page is Page page)
                    await page.DisplayAlert("Error",
                        $"Error en operación: {e.Exception?.InnerException?.Message ?? e.Exception?.Message}", "OK");
            });
            e.SetObserved();
        };
    }

    protected override async void OnStart()
    {
        base.OnStart();

        try
        {
            Debug.WriteLine("Inicializando datos locales...");
            await _localDataService.InitializeAsync();
            Debug.WriteLine("Datos locales cargados");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }
    }
}