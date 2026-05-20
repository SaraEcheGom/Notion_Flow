using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Teacher;

public partial class ActivityResultsPage : ContentPage
{
    static readonly Color TextDark     = Color.FromArgb("#1A4A32");
    static readonly Color TextMuted    = Color.FromArgb("#5A9A72");
    static readonly Color PrimaryColor = Color.FromArgb("#5BBF8A");
    static readonly Color PrimaryDark  = Color.FromArgb("#2E8A5E");
    static readonly Color PrimaryLight = Color.FromArgb("#C8F0DC");
    static readonly Color BorderLight  = Color.FromArgb("#90D4B0");
    static readonly Color AccentOrange = Color.FromArgb("#F4A940");
    static readonly Color AccentYellow = Color.FromArgb("#F9D06A");
    static readonly Color DangerColor  = Color.FromArgb("#F4A0C0");
    static readonly Color CardBg       = Colors.White;

    private readonly ApiService _api;
    private readonly int _courseId;
    private readonly int _activityId;

    public ActivityResultsPage(ApiService api, int courseId, int activityId, string activityTitle)
    {
        InitializeComponent();
        _api = api;
        _courseId = courseId;
        _activityId = activityId;
        ActivityTitleLabel.Text = activityTitle;
        SummaryLabel.Text = "Cargando resultados...";
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var data = await _api.GetActivityResultsAsync(_courseId, _activityId);

            SummaryLabel.Text = $"{data.Submitted} de {data.TotalStudents} estudiante(s) han respondido";
            SubmittedCountLabel.Text = $"{data.Submitted}/{data.TotalStudents}";

            if (data.Results.Count > 0)
            {
                var scores = data.Results.Where(r => r.Score.HasValue).Select(r => (double)r.Score!.Value).ToList();
                var avg = scores.Count > 0 ? scores.Average() : 0;
                var passed = scores.Count(s => s >= 60);
                GroupAverageLabel.Text = $"{avg:F0}%";
                PassRateLabel.Text = $"{passed}/{data.Results.Count}";
            }
            else
            {
                GroupAverageLabel.Text = "—";
                PassRateLabel.Text = "—";
            }

            ResultsContainer.Children.Clear();

            if (!data.Results.Any())
            {
                ResultsContainer.Children.Add(new Label
                {
                    Text = "Ningún estudiante ha enviado esta actividad aún.",
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = TextMuted, FontSize = 14,
                    Margin = new Thickness(0, 32)
                });
                return;
            }

            foreach (var result in data.Results.OrderByDescending(r => r.Score ?? -1))
                ResultsContainer.Children.Add(BuildStudentCard(result));
        }
        catch (Exception ex)
        {
            SummaryLabel.Text = "Error al cargar";
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            Loader.IsRunning = false;
            Loader.IsVisible = false;
        }
    }

    private View BuildStudentCard(StudentResult result)
    {
        var score = result.Score ?? 0;
        var passed = score >= 60;
        var hasMC = result.Questions.Any(q => q.QuestionType != "OpenText");

        var layout = new VerticalStackLayout { Spacing = 10 };

        // Encabezado
        var headerGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        var nameStack = new VerticalStackLayout { Spacing = 2 };
        nameStack.Add(new Label { Text = $"👤 {result.StudentName}", FontAttributes = FontAttributes.Bold, FontSize = 15, TextColor = TextDark });
        nameStack.Add(new Label { Text = result.StudentEmail, FontSize = 12, TextColor = TextMuted });
        if (result.SubmittedAt.HasValue)
            nameStack.Add(new Label { Text = $"Enviado: {result.SubmittedAt.Value.ToLocalTime():dd/MM/yyyy HH:mm}", FontSize = 11, TextColor = TextMuted });
        headerGrid.Add(nameStack, 0);

        if (hasMC)
        {
            var scoreColor = passed ? PrimaryDark : Color.FromArgb("#C0392B");
            headerGrid.Add(new Border
            {
                BackgroundColor = passed ? PrimaryLight : DangerColor,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(10, 6),
                VerticalOptions = LayoutOptions.Center,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = $"{score}%", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = scoreColor, HorizontalTextAlignment = TextAlignment.Center },
                        new Label { Text = $"{result.Correct}/{result.Total} correctas", FontSize = 10, TextColor = scoreColor, HorizontalTextAlignment = TextAlignment.Center }
                    }
                }
            }, 1);
        }
        layout.Add(headerGrid);

        // Separador
        layout.Add(new BoxView { HeightRequest = 1, BackgroundColor = BorderLight, Margin = new Thickness(0, 2) });

        // Detalle por pregunta
        int qNum = 0;
        foreach (var q in result.Questions)
        {
            qNum++;
            if (q.QuestionType == "OpenText")
            {
                var openStack = new VerticalStackLayout { Spacing = 4, Margin = new Thickness(0, 4) };
                openStack.Add(new Label { Text = $"P{qNum}. {q.QuestionText}", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = TextDark });
                openStack.Add(new Border
                {
                    BackgroundColor = Color.FromArgb("#F0FFF4"),
                    StrokeThickness = 1, Stroke = BorderLight,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                    Padding = new Thickness(10, 6),
                    Content = new Label
                    {
                        Text = string.IsNullOrWhiteSpace(q.TextAnswer) ? "(sin respuesta)" : q.TextAnswer,
                        FontSize = 13,
                        TextColor = string.IsNullOrWhiteSpace(q.TextAnswer) ? TextMuted : TextDark,
                        FontAttributes = FontAttributes.Italic
                    }
                });
                layout.Add(openStack);
                continue;
            }

            var qStack = new VerticalStackLayout { Spacing = 5, Margin = new Thickness(0, 4) };
            var qIcon = q.IsCorrect ? "✅" : "❌";
            var qColor = q.IsCorrect ? PrimaryDark : Color.FromArgb("#C0392B");
            qStack.Add(new Label { Text = $"{qIcon} P{qNum}. {q.QuestionText}", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = qColor });

            foreach (var opt in q.Options)
            {
                bool wasSelected = q.SelectedOptionIds.Contains(opt.Id);
                bool isCorrectOpt = q.CorrectOptionIds.Contains(opt.Id);
                if (!wasSelected && !isCorrectOpt) continue;

                Color chipBg; string chipIcon; string chipSuffix;
                if (wasSelected && isCorrectOpt)       { chipBg = PrimaryLight; chipIcon = "✅"; chipSuffix = " (seleccionó — correcta)"; }
                else if (wasSelected && !isCorrectOpt) { chipBg = DangerColor;  chipIcon = "❌"; chipSuffix = " (seleccionó — incorrecta)"; }
                else                                   { chipBg = AccentYellow; chipIcon = "💡"; chipSuffix = " (respuesta correcta)"; }

                qStack.Add(new Border
                {
                    BackgroundColor = chipBg, StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                    Padding = new Thickness(10, 5), Margin = new Thickness(8, 1),
                    Content = new Label { Text = $"{chipIcon} {opt.Text}{chipSuffix}", TextColor = TextDark, FontSize = 12 }
                });
            }
            layout.Add(qStack);
        }

        return new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
            StrokeThickness = 1.5,
            Stroke = hasMC ? (passed ? PrimaryColor : DangerColor) : BorderLight,
            BackgroundColor = CardBg,
            Padding = new Thickness(16),
            Content = layout
        };
    }
}
