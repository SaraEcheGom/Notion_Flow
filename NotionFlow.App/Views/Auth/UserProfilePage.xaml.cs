using NotionFlow.App.ViewModels.Auth;

namespace NotionFlow.App.Views.Auth;

public partial class UserProfilePage : ContentPage
{
    public UserProfilePage(UserProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
