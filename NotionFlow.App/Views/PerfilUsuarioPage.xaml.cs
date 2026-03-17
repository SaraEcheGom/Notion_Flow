using NotionFlow.App.ViewModels;

namespace NotionFlow.App.Views;

public partial class PerfilUsuarioPage : ContentPage
{
    public PerfilUsuarioPage(PerfilUsuarioViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}