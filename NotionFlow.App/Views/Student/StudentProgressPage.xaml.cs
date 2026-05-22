using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Student;

public partial class StudentProgressPage : ContentPage
{
    static readonly Color TextDark       = Color.FromArgb("#1A4A32");
    static readonly Color TextMuted      = Color.FromArgb("#5A9A72");
    static readonly Color PrimaryColor   = Color.FromArgb("#5BBF8A");
    static readonly Color PrimaryLight   = Color.FromArgb("#C8F0DC");
    static readonly Color PrimaryDark    = Color.FromArgb("#2E8A5E");
    static readonly Color AccentYellow   = Color.FromArgb("#F9D06A");
    static readonly Color AccentOrange   = Color.FromArgb("#F4A940");
    static readonly Color BorderLight    = Color.FromArgb("#90D4B0");
    static readonly Color CardBg         = Colors.White;
    static readonly Color PageBg         = Color.FromArgb("#FBF8EF");
    static readonly Color DangerColor    = Color.FromArgb("#F4A0C0");

    private readonly ApiService _api;

    public StudentProgressPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
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
            var progress = await _api.GetMyProgressAsync();

            TotalStudentsLabel.Text = $"Total Estudiantes: {progress.TotalStudents}";
            TotalActivitiesLabel.Text = $"Total Actividades: {progress.TotalActivities}";
            AverageLabel.Text = $"Promedio: {progress.AverageCourseScore:F1}";

            StudentsList.Children.Clear();
            if (progress.StudentSummaries.Count == 0)
            {
                StudentsList.Add(new Label
                {
                    Text = "No hay estudiantes en este curso.",
                    TextColor = TextMuted,
                    FontSize = 13
                });
            }
            else
            {
                foreach (var student in progress.StudentSummaries)
                {
                    var row = new HorizontalStackLayout { Spacing = 12 };
                    
                    var info = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.FillAndExpand };
                    info.Add(new Label 
                    { 
                        Text = student.StudentName, 
                        FontSize = 14, 
                        FontAttributes = FontAttributes.Bold, 
                        TextColor = TextDark 
                    });
                    info.Add(new Label
                    {
                        Text = $"Promedio: {student.Score:F1} · ID: {student.StudentId}",
                        FontSize = 12,
                        TextColor = TextMuted
                    });
                    row.Add(info);

                    StudentsList.Add(new Border
                    {
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                        BackgroundColor = CardBg,
                        Padding = new Thickness(14, 10),
                        StrokeThickness = 1.5,
                        Stroke = BorderLight,
                        Content = row
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el progreso: {ex.Message}", "OK");
        }
    }
}