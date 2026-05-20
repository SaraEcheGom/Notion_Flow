using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NotionFlow.App.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // Hook crash logging as early as possible so that we capture failures during
            // InitializeComponent / CreateMauiApp / first XAML load (root cause of
            // 0xc000027b crashes that don't surface a managed stack trace by themselves).
            try
            {
                CrashLog.Write("WinUI App ctor starting", $"Log file: {CrashLog.LogFilePath}");

                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                    CrashLog.Write("WinUI AppDomain.UnhandledException", e.ExceptionObject);

                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
                {
                    CrashLog.Write("WinUI TaskScheduler.UnobservedTaskException", e.Exception);
                    e.SetObserved();
                };

                this.UnhandledException += (s, e) =>
                {
                    CrashLog.Write("Microsoft.UI.Xaml.Application.UnhandledException", e.Exception);
                    // Do NOT mark e.Handled = true: we want the original behaviour preserved,
                    // but the log captures the stack so the user can read it afterwards.
                };
            }
            catch (Exception bootstrapEx)
            {
                CrashLog.Write("WinUI App ctor crash-logger setup failed", bootstrapEx);
            }

            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp()
        {
            try
            {
                return MauiProgram.CreateMauiApp();
            }
            catch (Exception ex)
            {
                CrashLog.Write("MauiProgram.CreateMauiApp threw", ex);
                throw;
            }
        }
    }

}
