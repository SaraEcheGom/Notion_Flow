using NotionFlow.App.ViewModels.Auth;

namespace NotionFlow.App.Views.Auth;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
        BindingContext = new RegisterViewModel();
    }
}
