using NotionFlow.App.ViewModels;
using NotionFlow.App.ViewModels.Professor;

namespace NotionFlow.App.Views.Professor;

[QueryProperty(nameof(ProfesorId), "id")]
public partial class ProfessorPage : ContentPage
{
    private string _profesorId = string.Empty;

    public string ProfesorId
    {
        get => _profesorId;
        set
        {
            _profesorId = value;
            if (!string.IsNullOrEmpty(_profesorId))
                BindingContext = new ProfessorViewModel(_profesorId);
        }
    }

    public ProfessorPage()
    {
        InitializeComponent();
    }
}
