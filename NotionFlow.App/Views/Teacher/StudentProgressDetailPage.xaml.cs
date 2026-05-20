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
    private readonly int _courseId;
    private readonly string _studentId;

    public StudentProgressDetailPage(ApiService api, int courseId, string studentId, string studentName)
    {
        InitializeComponent();
        _api = api;
        _courseId = courseId;
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
            var progress = await _api.GetStudentProgressAsync(_courseId, _studentId);

            CompletedLabel.Text = $"{progress.CompletedActivities}/{progress.TotalActivities}";
            AverageLabel.Text   = $"{progress.AverageScore:F0}";
            PointsLabel.Text    = $"{progress.TotalPoints}";

            // Nivel
            LevelLabel.Text = $"{progress.LevelEmoji} {progress.LevelName}";
            StreakLabel.Text = $"🔁 Racha: {progress.Streak} actividades";

            // Barra de nivel
            if (progress.NextLevelPoints < int.MaxValue && progress.NextLevelPoints > 0)
            {
                double levelPct = Math.Min(1.0, (double)progress.TotalPoints / progress.NextLevelPoints);
                LevelProgress.Progress = levelPct;
                LevelProgressLabel.Text = $"{progress.TotalPoints} / {progress.NextLevelPoints} pts para subir de nivel";
            }
            else
            {
                LevelProgress.Progress = 1.0;
                LevelProgressLabel.Text = "¡Nivel máximo alcanzado! 🌟";
            }

            // Insignias
            BadgesList.Children.Clear();
            if (progress.Badges.Count == 0)
            {
                BadgesList.Add(new Label { Text = "Este estudiante aún no ha ganado insignias.", TextColor = TextMuted, FontSize = 13 });
            }
            else
            {
                foreach (var badge in progress.Badges)
                {
                    var row = new HorizontalStackLayout { Spacing = 12 };
                    row.Add(new Label { Text = badge.Emoji, FontSize = 26, VerticalOptions = LayoutOptions.Center });
                    var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
                    texts.Add(new Label { Text = badge.Name, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = PrimaryDark });
                    texts.Add(new Label { Text = badge.Description, FontSize = 12, TextColor = TextMuted });
                    if (badge.EarnedAt.HasValue)
                        texts.Add(new Label { Text = $"Obtenida el {badge.EarnedAt:dd/MM/yyyy}", FontSize = 11, TextColor = TextMuted });
                    row.Add(texts);
                    BadgesList.Add(new Border
                    {
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                        BackgroundColor = PrimaryLight,
                        Stroke = BorderLight, StrokeThickness = 1,
                        Padding = new Thickness(12, 8),
                        Content = row
                    });
                }
            }

            // Actividades
            ActivitiesList.Children.Clear();
            if (progress.ActivityDetails.Count == 0)
            {
                ActivitiesList.Add(new Label { Text = "No hay actividades asignadas a este estudiante.", TextColor = TextMuted, FontSize = 13 });
                return;
            }

            foreach (var act in progress.ActivityDetails)
            {
                var row = new HorizontalStackLayout { Spacing = 12 };
                row.Add(new Label { Text = act.Completed ? "✅" : "⏳", FontSize = 20, VerticalOptions = LayoutOptions.Center });

                var info = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.FillAndExpand };
                info.Add(new Label { Text = act.ActivityTitle, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = TextDark });
                info.Add(new Label
                {
                    Text = act.Completed && act.Score.HasValue
                        ? $"Puntuación: {act.Score}/100 · {act.SubmittedAt:dd/MM/yyyy}"
                        : "Pendiente",
                    FontSize = 12,
                    TextColor = act.Completed ? PrimaryDark : TextMuted
                });
                row.Add(info);

                if (act.Completed && act.Score.HasValue)
                {
                    var scoreColor = act.Score == 100 ? AccentOrange :
                                    act.Score >= 80 ? PrimaryDark : TextMuted;
                    row.Add(new Label { Text = $"+{act.Score}pts", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = scoreColor, VerticalOptions = LayoutOptions.Center });
                }

                ActivitiesList.Add(new Border
                {
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                    BackgroundColor = CardBg,
                    Padding = new Thickness(14, 10), StrokeThickness = 1.5,
                    Stroke = act.Completed ? PrimaryColor : BorderLight,
                    Content = row
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el progreso: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
