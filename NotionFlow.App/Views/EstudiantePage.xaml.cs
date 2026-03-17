using NotionFlow.App.ViewModels;

namespace NotionFlow.App.Views;

[QueryProperty(nameof(EstudianteId), "id")]
public partial class EstudiantePage : ContentPage
{
    private string _estudianteId = string.Empty;

    public string EstudianteId
    {
        get => _estudianteId;
        set
        {
            _estudianteId = value;
            if (!string.IsNullOrEmpty(_estudianteId))
                BindingContext = new EstudianteViewModel(_estudianteId);
        }
    }

    public EstudiantePage()
    {
        InitializeComponent();
    }
}