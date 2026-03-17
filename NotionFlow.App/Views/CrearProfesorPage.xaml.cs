using NotionFlow.App.ViewModels;

namespace NotionFlow.App.Views;

public partial class CrearProfesorPage : ContentPage
{
    public CrearProfesorPage(AdminViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}