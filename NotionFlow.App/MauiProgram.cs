using Microsoft.Extensions.Logging;
using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Auth;
using NotionFlow.App.ViewModels.Admin;
using NotionFlow.App.ViewModels.Teacher;
using NotionFlow.App.ViewModels.Student;
using NotionFlow.App.Views.Auth;
using NotionFlow.App.Views.Admin;

namespace NotionFlow.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Fraunces_72pt-Regular.ttf", "FrauncesRegular");
                    fonts.AddFont("Fraunces_72pt-Italic.ttf", "FrauncesItalic");
                    fonts.AddFont("Fraunces_72pt-SemiBold.ttf", "FrauncesSemiBold");
                });

            // ── Servicios (Singleton: una sola instancia compartida) ───────────
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<AuthService>();

            // ── Shell ─────────────────────────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();

            // ── ViewModels ────────────────────────────────────────────────────
            // Singleton: mantienen estado de sesión entre navegaciones
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<RegisterViewModel>();
            builder.Services.AddSingleton<AdminViewModel>();
            builder.Services.AddSingleton<TeacherViewModel>();
            builder.Services.AddSingleton<StudentViewModel>();

            // ── Páginas con dependencias ──────────────────────────────────────
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<CreateCoursePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
