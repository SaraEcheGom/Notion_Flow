namespace NotionFlow.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Manejar excepciones no capturadas
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            CrashLog.Write("AppDomain.UnhandledException", e.ExceptionObject);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (MainPage?.Window?.Page is Page page)
                {
                    await page.DisplayAlert("Error Fatal", $"La aplicación encontró un error: {e.ExceptionObject}", "OK");
                }
            });
        };

        // Manejar excepciones de tareas no esperadas
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            CrashLog.Write("TaskScheduler.UnobservedTaskException", e.Exception);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (MainPage?.Window?.Page is Page page)
                {
                    await page.DisplayAlert("Error", $"Error en operación: {e.Exception?.InnerException?.Message ?? e.Exception?.Message}", "OK");
                }
            });
            e.SetObserved();
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}