using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Teacher;

public partial class CourseReportPage : ContentPage
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
    static readonly Color Gold         = Color.FromArgb("#FFD700");
    static readonly Color Silver       = Color.FromArgb("#C0C0C0");
    static readonly Color Bronze       = Color.FromArgb("#CD7F32");

    private readonly ApiService _api;
    private readonly int _courseId;

    public CourseReportPage(ApiService api, int courseId, string courseName)
    {
        InitializeComponent();
        _api = api;
        _courseId = courseId;
        CourseTitleLabel.Text = courseName;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadReportAsync();
    }

    private async Task LoadReportAsync()
    {
        try
        {
            var report = await _api.GetCourseReportAsync(_courseId);

            TotalStudentsLabel.Text   = report.TotalStudents.ToString();
            TotalActivitiesLabel.Text = report.TotalActivities.ToString();
            AverageCourseLabel.Text   = $"{report.AverageCourseScore:F0}";

            StudentsList.Children.Clear();

            if (report.StudentSummaries.Count == 0)
            {
                StudentsList.Add(new Label
                {
                    Text = "No hay estudiantes inscritos en este curso.",
                    TextColor = TextMuted, FontSize = 13
                });
                return;
            }

            // Encabezado ranking
            StudentsList.Add(new Label
            {
                Text = "🏅 Ranking del Grupo",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = TextDark,
                Margin = new Thickness(0, 4, 0, 4)
            });

            foreach (var student in report.StudentSummaries)
            {
                double pct = student.TotalActivities > 0
                    ? (double)student.CompletedActivities / student.TotalActivities : 0;

                // Color de medalla para top 3
                Color rankBg = student.Rank switch
                {
                    1 => Gold,
                    2 => Silver,
                    3 => Bronze,
                    _ => PrimaryLight
                };
                Color rankText = student.Rank <= 3 ? Colors.White : TextDark;
                string rankIcon = student.Rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{student.Rank}" };

                var main = new VerticalStackLayout { Spacing = 10 };

                // Fila: posición + nombre + nivel + puntos
                var topRow = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(new GridLength(44)),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    ColumnSpacing = 8
                };

                // Medalla
                topRow.Add(new Border
                {
                    BackgroundColor = rankBg,
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse(),
                    WidthRequest = 40, HeightRequest = 40,
                    VerticalOptions = LayoutOptions.Center,
                    Content = new Label
                    {
                        Text = rankIcon, FontSize = student.Rank <= 3 ? 18 : 13,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = rankText,
                        FontAttributes = FontAttributes.Bold
                    }
                }, 0);

                // Nombre + nivel
                var nameStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
                nameStack.Add(new Label
                {
                    Text = $"{student.LevelEmoji} {student.StudentName}",
                    FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = TextDark
                });
                nameStack.Add(new Label
                {
                    Text = $"{student.LevelName} · {student.CompletedActivities}/{student.TotalActivities} act. · Prom: {student.AverageScore:F0}",
                    FontSize = 11, TextColor = TextMuted
                });
                // Insignias en mini
                if (student.Badges.Count > 0)
                {
                    var badgeRow = new HorizontalStackLayout { Spacing = 4 };
                    foreach (var b in student.Badges.Take(5))
                        badgeRow.Add(new Label { Text = b.Emoji, FontSize = 14 });
                    if (student.BadgeCount > 5)
                        badgeRow.Add(new Label { Text = $"+{student.BadgeCount - 5}", FontSize = 11, TextColor = TextMuted, VerticalOptions = LayoutOptions.Center });
                    nameStack.Add(badgeRow);
                }
                topRow.Add(nameStack, 1);

                // Puntos
                topRow.Add(new Border
                {
                    BackgroundColor = AccentYellow,
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                    Padding = new Thickness(10, 4),
                    VerticalOptions = LayoutOptions.Center,
                    Content = new Label
                    {
                        Text = $"🏅 {student.TotalPoints}pts",
                        FontSize = 12, FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#7A5A20")
                    }
                }, 2);

                main.Add(topRow);

                // Barra de progreso
                var progressBar = new ProgressBar
                {
                    Progress = pct,
                    ProgressColor = student.Rank == 1 ? Gold : PrimaryColor,
                    HeightRequest = 6
                };
                main.Add(progressBar);

                // Botón ver detalle
                var btn = new Button
                {
                    Text = "Ver progreso individual →",
                    FontSize = 12,
                    BackgroundColor = PrimaryColor,
                    TextColor = Colors.White,
                    CornerRadius = 20,
                    Padding = new Thickness(16, 8),
                    HorizontalOptions = LayoutOptions.Start
                };
                var sid = student.StudentId;
                var sname = student.StudentName;
                btn.Clicked += async (s, e) =>
                    await Navigation.PushAsync(new StudentProgressDetailPage(_api, _courseId, sid, sname));
                main.Add(btn);

                // Borde especial para top 3
                Color cardBorder = student.Rank switch { 1 => Gold, 2 => Silver, 3 => Bronze, _ => BorderLight };
                StudentsList.Add(new Border
                {
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                    BackgroundColor = CardBg,
                    Padding = new Thickness(16, 14),
                    StrokeThickness = student.Rank <= 3 ? 2.5 : 1.5,
                    Stroke = cardBorder,
                    Content = main
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el reporte: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
