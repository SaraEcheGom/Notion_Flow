using NotionFlow.App.ViewModels;

namespace NotionFlow.App.Views;

[QueryProperty(nameof(CursoId), "cursoId")]
[QueryProperty(nameof(CursoNombre), "cursoNombre")]
[QueryProperty(nameof(Rol), "rol")]
public partial class CursoPage : ContentPage
{
    private string _cursoId = string.Empty;
    private string _cursoNombre = string.Empty;
    private string _rol = string.Empty;

    public string CursoId
    {
        get => _cursoId;
        set { _cursoId = value; TryLoadViewModel(); }
    }

    public string CursoNombre
    {
        get => _cursoNombre;
        set { _cursoNombre = value; TryLoadViewModel(); }
    }

    public string Rol
    {
        get => _rol;
        set { _rol = value; TryLoadViewModel(); }
    }

    private void TryLoadViewModel()
    {
        if (!string.IsNullOrEmpty(_cursoId) &&
            !string.IsNullOrEmpty(_cursoNombre) &&
            !string.IsNullOrEmpty(_rol))
        {
            BindingContext = new CursoViewModel(_cursoId, _cursoNombre, _rol);
        }
    }

    public CursoPage()
    {
        InitializeComponent();
    }
}