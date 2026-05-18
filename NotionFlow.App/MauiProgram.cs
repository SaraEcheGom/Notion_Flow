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

            // Register Services
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<AuthService>();

            // Register Auth ViewModels
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<RegisterViewModel>();

            // Register Role ViewModels
            builder.Services.AddSingleton<AdminViewModel>();
            builder.Services.AddSingleton<TeacherViewModel>();
            builder.Services.AddSingleton<StudentViewModel>();

            // Register Pages whose constructors require DI (needed for Shell on Windows/WinUI;
            // without this, MAUI on Windows tries to instantiate them via parameterless ctor
            // and crashes at startup with a XAML/handler init failure (0xc000027b)).
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
