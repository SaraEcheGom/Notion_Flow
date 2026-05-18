using NotionFlow.App.ViewModels.Auth;
using NotionFlow.App.Platforms.Helpers;

namespace NotionFlow.App.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Ajustar padding superior del hero al inset real de la status bar / notch.
        // El padding base lateral e inferior se conserva (26 / 78).
        double topInset = SafeAreaHelper.GetTopInset();
        HeroContent.Padding = new Thickness(26, 16 + topInset, 26, 78);

        // Animación de entrada: fade + slide desde abajo
        LoginContainer.Opacity = 0;
        LoginContainer.TranslationY = 30;

        await Task.Delay(80);

        try
        {
            await Task.WhenAll(
                LoginContainer.FadeTo(1, 500, Easing.CubicOut),
                LoginContainer.TranslateTo(0, 0, 500, Easing.CubicOut)
            );
        }
        catch (ObjectDisposedException)
        {
            // La página fue descartada antes de que terminara la animación.
        }
    }
}
