using NotionFlow.App.ViewModels;

namespace NotionFlow.App.Views;

public partial class CrearCursoPage : ContentPage
{
    public CrearCursoPage(AdminViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}