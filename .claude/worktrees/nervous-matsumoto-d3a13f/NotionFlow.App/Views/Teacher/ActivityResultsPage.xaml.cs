using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Teacher;

public partial class ActivityResultsPage : ContentPage
{
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

            SummaryLabel.Text =
                $"{data.Submitted} de {data.TotalStudents} estudiante(s) han respondido";

            ResultsContainer.Children.Clear();

            if (!data.Results.Any())
            {
                ResultsContainer.Children.Add(new Label
                {
                    Text = "Ningún estudiante ha enviado esta actividad aún.",
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(0, 24),
                });
                return;
            }

            foreach (var result in data.Results.OrderByDescending(r => r.Score))
            {
                ResultsContainer.Children.Add(BuildStudentCard(result));
            }
        }
        catch (Exception ex)
        {
            SummaryLabel.Text = "Error al cargar";
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private View BuildStudentCard(StudentResult result)
    {
        var scoreColor = (result.Score ?? 0) >= 60
            ? Color.FromArgb("#388E3C")
            : Color.FromArgb("#C62828");

        var layout = new VerticalStackLayout { Spacing = 10 };

        // Student header row
        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        };

        var nameStack = new VerticalStackLayout { Spacing = 2 };
        nameStack.Children.Add(new Label
        {
            Text = result.StudentName,
            FontAttributes = FontAttributes.Bold,
            FontSize = 15,
        });
        nameStack.Children.Add(new Label
        {
            Text = result.StudentEmail,
            FontSize = 12,
            TextColor = Colors.Gray,
        });
        if (result.SubmittedAt.HasValue)
        {
            nameStack.Children.Add(new Label
            {
                Text = $"Enviado: {result.SubmittedAt.Value.ToLocalTime():dd/MM/yyyy HH:mm}",
                FontSize = 11,
                TextColor = Colors.Gray,
            });
        }

        headerGrid.Add(nameStack, 0);

        // Score badge
        var mcQuestions = result.Questions.Where(q => q.QuestionType != "OpenText").ToList();
        if (mcQuestions.Any())
        {
            var scoreLabel = new Label
            {
                Text = $"{result.Correct}/{result.Total}\n{result.Score}%",
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = scoreColor,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            headerGrid.Add(scoreLabel, 1);
        }

        layout.Children.Add(headerGrid);

        // Separator
        layout.Children.Add(new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = Color.FromArgb("#DDDDDD"),
        });

        // Per-question results (only multiple choice)
        foreach (var q in result.Questions)
        {
            if (q.QuestionType == "OpenText")
            {
                // Show open text answer
                var openStack = new VerticalStackLayout { Spacing = 3, Margin = new Thickness(0, 2) };
                openStack.Children.Add(new Label
                {
                    Text = $"📝 {q.QuestionText}",
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                });
                openStack.Children.Add(new Label
                {
                    Text = $"Respuesta: {q.TextAnswer ?? "(sin respuesta)"}",
                    FontSize = 12,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(8, 0, 0, 0),
                });
                layout.Children.Add(openStack);
                continue;
            }

            // Multiple choice question
            var qStack = new VerticalStackLayout { Spacing = 4, Margin = new Thickness(0, 2) };
            var qCorrectColor = q.IsCorrect ? Color.FromArgb("#388E3C") : Color.FromArgb("#C62828");
            var qIcon = q.IsCorrect ? "✅" : "❌";

            qStack.Children.Add(new Label
            {
                Text = $"{qIcon} {q.QuestionText}",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = qCorrectColor,
            });

            // Show selected & correct options
            foreach (var opt in q.Options)
            {
                bool wasSelected = q.SelectedOptionIds.Contains(opt.Id);
                bool isCorrectOpt = q.CorrectOptionIds.Contains(opt.Id);

                if (!wasSelected && !isCorrectOpt) continue;

                Color chipBg;
                string chipIcon;
                string chipSuffix;

                if (wasSelected && isCorrectOpt)
                {
                    chipBg = Color.FromArgb("#388E3C");
                    chipIcon = "✅";
                    chipSuffix = " (seleccionada — correcta)";
                }
                else if (wasSelected && !isCorrectOpt)
                {
                    chipBg = Color.FromArgb("#C62828");
                    chipIcon = "❌";
                    chipSuffix = " (seleccionada — incorrecta)";
                }
                else
                {
                    chipBg = Color.FromArgb("#1565C0");
                    chipIcon = "💡";
                    chipSuffix = " (respuesta correcta)";
                }

                qStack.Children.Add(new Border
                {
                    BackgroundColor = chipBg,
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 5 },
                    Padding = new Thickness(8, 4),
                    Margin = new Thickness(8, 1),
                    Content = new Label
                    {
                        Text = $"{chipIcon} {opt.Text}{chipSuffix}",
                        TextColor = Colors.White,
                        FontSize = 12,
                    },
                });
            }

            layout.Children.Add(qStack);
        }

        return new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            StrokeThickness = 1.5,
            Stroke = Color.FromArgb("#CCCCCC"),
            Padding = new Thickness(16),
            Content = layout,
        };
    }
}
