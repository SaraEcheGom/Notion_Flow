using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Teacher;

public partial class StudentProgressDetailPage : ContentPage
{
    static readonly Color TextDark     = Color.FromArgb("#1A4A32");
    static readonly Color TextMuted    = Color.FromArgb("#5A9A72");
    static readonly Color PrimaryColor = Color.FromArgb("#5BBF8A");
    static readonly Color PrimaryDark  = Color.FromArgb("#2E8A5E");
    static readonly Color PrimaryLight = Color.FromArgb("#C8F0DC");
    static readonly Color BorderLight  = Color.FromArgb("#90D4B0");
    static readonly Color AccentOrange = Color.FromArgb("#F4A940");
    static readonly Color AccentYellow = Color.FromArgb("#F9D06A");
    static readonly Color CardBg       = Colors.White;

    private readonly ApiService _api;
    private readonly string _studentId;

    public StudentProgressDetailPage(ApiService api, string studentId, string studentName)
    {
        InitializeComponent();
        _api = api;
        _studentId = studentId;
        StudentTitleLabel.Text = studentName;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProgressAsync();
    }

    private async Task LoadProgressAsync()
    {
        try
        {
            var progress = await _api.GetStudentProgressAsync(_studentId);

            CompletedLabel.Text = $"{progress.TotalActivities}";
            AverageLabel.Text   = $"{progress.AverageCourseScore:F0}";
            PointsLabel.Text    = $"{progress.TotalStudents}";

            LevelLabel.Text = "Progreso del Estudiante";
            StreakLabel.Text = $"Total Actividades: {progress.TotalActivities}";

            if (progress.TotalStudents > 0)
            {
                double levelPct = Math.Min(1.0, (double)progress.AverageCourseScore / 100);
                LevelProgress.Progress = levelPct;
                LevelProgressLabel.Text = $"Promedio: {progress.AverageCourseScore:F1}%";
            }

            BadgesList.Children.Clear();
            BadgesList.Add(new Label { Text = "Sin insignias asignadas", TextColor = TextMuted, FontSize = 13 });

            ActivitiesList.Children.Clear();
            ActivitiesList.Add(new Label { Text = "Cargando actividades...", TextColor = TextMuted, FontSize = 13 });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el progreso: {ex.Message}", "OK");
        }
    }
}