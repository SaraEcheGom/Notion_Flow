using NotionFlow.App.ViewModels.Auth;

namespace NotionFlow.App.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // Animación de entrada: fade + slide desde abajo
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LoginContainer.Opacity = 0;
        LoginContainer.TranslationY = 30;

        // Pequeño delay para que la imagen de fondo cargue
        await Task.Delay(80);

        await Task.WhenAll(
            LoginContainer.FadeTo(1, 500, Easing.CubicOut),
            LoginContainer.TranslateTo(0, 0, 500, Easing.CubicOut)
        );
    }
}
