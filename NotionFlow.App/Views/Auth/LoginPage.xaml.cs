using NotionFlow.App.ViewModels.Auth;

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
        await Task.Delay(80);
    }
}
