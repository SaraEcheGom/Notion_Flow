using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Student;

public partial class StudentProgressPage : ContentPage
{
    // Paleta del tema crema/verde de la app
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
    private readonly int _courseId;
    private readonly string _studentId;

    public StudentProgressPage(ApiService api, int courseId, string studentId)
    {
        InitializeComponent();
        _api = api;
        _courseId = courseId;
        _studentId = studentId;
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
            var progress = await _api.GetMyProgressAsync(_courseId, _studentId);

            StudentNameLabel.Text = progress.StudentName;
            CompletedLabel.Text   = $"{progress.CompletedActivities}/{progress.TotalActivities}";
            AverageLabel.Text     = $"{progress.AverageScore:F0}";
            PointsLabel.Text      = $"{progress.TotalPoints}";

            double pct = progress.TotalActivities > 0
                ? (double)progress.CompletedActivities / progress.TotalActivities : 0;
            OverallProgress.Progress = pct;
            ProgressPercentLabel.Text = $"{pct * 100:F0}% completado";

            // ── Nivel ────────────────────────────────────────────────────────
            LevelLabel.Text = $"{progress.LevelEmoji} Nivel: {progress.LevelName}";
            StreakLabel.Text = $"🔁 Racha: {progress.Streak} actividades seguidas";

            if (progress.NextLevelPoints < int.MaxValue && progress.NextLevelPoints > 0)
            {
                double levelPct = Math.Min(1.0, (double)progress.TotalPoints / progress.NextLevelPoints);
                LevelProgressBar.Progress = levelPct;
                LevelProgressLabel.Text = $"{progress.TotalPoints} / {progress.NextLevelPoints} pts para el siguiente nivel";
            }
            else
            {
                LevelProgressBar.Progress = 1.0;
                LevelProgressLabel.Text = "🌟 ¡Nivel máximo — Leyenda!";
            }

            // ── Insignias ────────────────────────────────────────────────────
            BadgesList.Children.Clear();
            if (progress.Badges.Count == 0)
            {
                BadgesList.Add(new Label
                {
                    Text = "Aún no tienes insignias. ¡Completa actividades para ganarlas!",
                    TextColor = TextMuted, FontSize = 13
                });
            }
            else
            {
                // Título con conteo
                BadgesHeader.Text = $"🏆 Mis Insignias ({progress.Badges.Count})";

                foreach (var badge in progress.Badges)
                {
                    var row = new HorizontalStackLayout { Spacing = 12 };
                    row.Add(new Label { Text = badge.Emoji, FontSize = 28, VerticalOptions = LayoutOptions.Center });
                    var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
                    texts.Add(new Label { Text = badge.Name, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = PrimaryDark });
                    texts.Add(new Label { Text = badge.Description, FontSize = 12, TextColor = TextMuted });
                    if (badge.EarnedAt.HasValue)
                        texts.Add(new Label { Text = $"🗓 Obtenida el {badge.EarnedAt:dd/MM/yyyy}", FontSize = 11, TextColor = TextMuted });
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

            // ── Actividades ───────────────────────────────────────────────────
            ActivitiesList.Children.Clear();
            if (progress.ActivityDetails.Count == 0)
            {
                ActivitiesList.Add(new Label { Text = "No tienes actividades asignadas todavía.", TextColor = TextMuted, FontSize = 13 });
            }
            else
            {
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
                        var ptColor = act.Score == 100 ? AccentOrange :
                                      act.Score >= 80 ? PrimaryDark : TextMuted;
                        row.Add(new Label
                        {
                            Text = $"+{act.Score}pts",
                            FontSize = 13, FontAttributes = FontAttributes.Bold,
                            TextColor = ptColor, VerticalOptions = LayoutOptions.Center
                        });
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
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el progreso: {ex.Message}", "OK");
        }
    }
}
