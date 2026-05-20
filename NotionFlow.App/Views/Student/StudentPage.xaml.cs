using NotionFlow.App.ViewModels;
using NotionFlow.App.ViewModels.Student;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Student;

[QueryProperty(nameof(StudentId), "id")]
public partial class StudentPage : ContentPage
{
    private string _studentId = string.Empty;

    public string StudentId
    {
        get => _studentId;
        set
        {
            _studentId = value;
            if (!string.IsNullOrEmpty(_studentId))
            {
                var apiService = new ApiService();
                BindingContext = new StudentViewModel(apiService, _studentId);
            }
        }
    }

    public StudentPage()
    {
        InitializeComponent();
    }

    // Animación de entrada suave
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Parte desde invisible y ligeramente abajo
        MainContainer.Opacity = 0;
        MainContainer.TranslationY = 20;

        // Fade in + slide up simultáneos
        await Task.WhenAll(
            MainContainer.FadeTo(1, 400, Easing.CubicOut),
            MainContainer.TranslateTo(0, 0, 400, Easing.CubicOut)
        );
    }
}
