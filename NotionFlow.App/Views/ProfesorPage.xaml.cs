using NotionFlow.App.ViewModels;

namespace NotionFlow.App.Views;

[QueryProperty(nameof(ProfesorId), "id")]
public partial class ProfesorPage : ContentPage
{
    private string _profesorId = string.Empty;

    public string ProfesorId
    {
        get => _profesorId;
        set
        {
            _profesorId = value;
            if (!string.IsNullOrEmpty(_profesorId))
                BindingContext = new ProfesorViewModel(_profesorId);
        }
    }

    public ProfesorPage()
    {
        InitializeComponent();
    }
}