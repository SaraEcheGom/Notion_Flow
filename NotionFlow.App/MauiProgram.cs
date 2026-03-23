using Microsoft.Extensions.Logging;
using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Auth;
using NotionFlow.App.ViewModels.Admin;
using NotionFlow.App.ViewModels.Professor;
using NotionFlow.App.ViewModels.Student;

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
                });

            // Register Services
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<AuthService>();

            // Register Auth ViewModels
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<RegisterViewModel>();

            // Register Role ViewModels
            builder.Services.AddSingleton<AdminViewModel>();
            builder.Services.AddSingleton<ProfessorViewModel>();
            builder.Services.AddSingleton<StudentViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
