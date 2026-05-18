using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Student;

public partial class TakeActivityPage : ContentPage
{
    private readonly ApiService _api;
    private readonly ActivityModel _activity;
    private readonly List<QuestionAnswerWidget> _widgets = new();
    private bool _submitted = false;

    public TakeActivityPage(ApiService api, ActivityModel activity)
    {
        InitializeComponent();
        _api = api;
        _activity = activity;

        ActivityTitleLabel.Text = activity.Title;
        ActivityDescLabel.Text = activity.Description;
        ActivityInfoLabel.Text =
            $"Valor: {activity.PercentageValue}%  |  Entrega: {activity.DueDate:dd/MM/yyyy}  |  Preguntas: {activity.Questions.Count}";

        BuildQuestions();
    }

    private void BuildQuestions()
    {
        QuestionsContainer.Children.Clear();
        _widgets.Clear();

        for (int i = 0; i < _activity.Questions.Count; i++)
        {
            var q = _activity.Questions[i];
            var widget = new QuestionAnswerWidget(i + 1, q);
            _widgets.Add(widget);
            QuestionsContainer.Children.Add(widget.Container);
        }
    }

    private async void OnSubmit(object sender, EventArgs e)
    {
        if (_submitted) return;

        var answers = new List<AnswerPayload>();
        foreach (var w in _widgets)
        {
            var ans = w.GetAnswer();
            if (ans == null)
            {
                await DisplayAlert("Aviso", $"Por favor responde la pregunta {w.Number}.", "OK");
                return;
            }
            answers.Add(ans);
        }

        var confirm = await DisplayAlert("Confirmar",
            "¿Deseas enviar tus respuestas? Esta acción no se puede deshacer.",
            "Enviar", "Cancelar");
        if (!confirm) return;

        try
        {
            SubmitButton.IsEnabled = false;
            SubmitButton.Text = "Enviando...";

            var feedback = await _api.SubmitActivityAsync(_activity.CourseId, _activity.Id, answers);
            _submitted = true;

            ShowFeedback(feedback);
        }
        catch (Exception ex)
        {
            SubmitButton.IsEnabled = true;
            SubmitButton.Text = "✅ Enviar respuestas";
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void ShowFeedback(SubmitFeedbackResponse feedback)
    {
        SubmitButton.Text = "← Volver al curso";
        SubmitButton.BackgroundColor = Color.FromArgb("#388E3C");
        SubmitButton.IsEnabled = true;
        SubmitButton.Clicked -= OnSubmit;
        SubmitButton.Clicked += async (s, e) => await Navigation.PopAsync();

        ScoreBanner.IsVisible = true;
        if (feedback.Total > 0)
        {
            ScoreLabel.Text = $"Resultado: {feedback.Correct}/{feedback.Total} correctas — {feedback.Score}%";
            ScoreLabel.TextColor = feedback.Score >= 60
                ? Color.FromArgb("#388E3C")
                : Color.FromArgb("#C62828");
        }
        else
        {
            ScoreLabel.Text = "Actividad enviada correctamente";
            ScoreLabel.TextColor = Color.FromArgb("#388E3C");
        }

        QuestionsContainer.Children.Clear();
        foreach (var w in _widgets)
        {
            var fb = feedback.Answers.FirstOrDefault(a => a.QuestionId == w.QuestionModel.Id);
            w.ShowFeedback(fb);
            QuestionsContainer.Children.Add(w.Container);
        }
    }
}

// ── Widget por pregunta ──────────────────────────────────────────────────────

internal class QuestionAnswerWidget
{
    public int Number { get; }
    public Border Container { get; private set; }
    public ActivityQuestionModel QuestionModel => _question;

    private readonly ActivityQuestionModel _question;
    private readonly List<(CheckBox cb, Label lbl, int optionId)> _optionRows = new();
    private readonly Editor? _openTextEditor;
    private readonly bool _isMultipleChoice;
    private readonly VerticalStackLayout _layout;

    public QuestionAnswerWidget(int number, ActivityQuestionModel question)
    {
        Number = number;
        _question = question;
        _isMultipleChoice = question.QuestionType != "OpenText";
        _layout = new VerticalStackLayout { Spacing = 8 };

        _layout.Children.Add(new Label
        {
            Text = $"Pregunta {number}: {question.Text}",
            FontAttributes = FontAttributes.Bold,
            FontSize = 15,
        });

        if (_isMultipleChoice)
        {
            foreach (var opt in question.Options)
            {
                var row = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star),
                    },
                    ColumnSpacing = 8,
                };
                var cb = new CheckBox { VerticalOptions = LayoutOptions.Center };
                var lbl = new Label
                {
                    Text = opt.Text,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 14,
                };
                row.Add(cb, 0);
                row.Add(lbl, 1);
                _optionRows.Add((cb, lbl, opt.Id));
                _layout.Children.Add(row);
            }
            _layout.Children.Add(new Label
            {
                Text = "Selecciona la opción correcta",
                FontSize = 11,
                TextColor = Colors.Gray,
                FontAttributes = FontAttributes.Italic,
            });
        }
        else
        {
            _openTextEditor = new Editor
            {
                Placeholder = "Escribe tu respuesta aquí...",
                HeightRequest = 100,
                AutoSize = EditorAutoSizeOption.TextChanges,
            };
            _layout.Children.Add(_openTextEditor);
        }

        Container = MakeBorder(_layout, Colors.Transparent);
    }

    public AnswerPayload? GetAnswer()
    {
        if (_isMultipleChoice)
        {
            var selected = _optionRows
                .Where(r => r.cb.IsChecked)
                .Select(r => r.optionId)
                .ToList();
            if (!selected.Any()) return null;
            return new AnswerPayload
            {
                QuestionId = _question.Id,
                SelectedOptionIds = selected,
                TextAnswer = null,
            };
        }
        else
        {
            var text = _openTextEditor?.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return null;
            return new AnswerPayload
            {
                QuestionId = _question.Id,
                SelectedOptionIds = new List<int>(),
                TextAnswer = text,
            };
        }
    }

    public void ShowFeedback(QuestionFeedback? fb)
    {
        if (!_isMultipleChoice || fb == null)
        {
            if (_openTextEditor != null) _openTextEditor.IsEnabled = false;
            Container = MakeBorder(_layout, Colors.Transparent);
            return;
        }

        foreach (var (cb, _, _) in _optionRows) cb.IsEnabled = false;

        // Remove hint label at end
        if (_layout.Children.LastOrDefault() is Label last && last.FontSize == 11)
            _layout.Children.Remove(last);

        // Result summary
        var summaryColor = fb.IsCorrect ? Color.FromArgb("#388E3C") : Color.FromArgb("#C62828");
        _layout.Children.Add(new Label
        {
            Text = fb.IsCorrect ? "✅ Correcta" : "❌ Incorrecta",
            TextColor = summaryColor,
            FontAttributes = FontAttributes.Bold,
            FontSize = 13,
            Margin = new Thickness(0, 6, 0, 2),
        });

        // Chips per relevant option
        foreach (var (cb, lbl, optId) in _optionRows)
        {
            bool wasSelected = fb.SelectedOptionIds.Contains(optId);
            bool isCorrectOpt = fb.CorrectOptionIds.Contains(optId);

            if (!wasSelected && !isCorrectOpt) continue;

            Color chipBg;
            string icon;
            string suffix = "";

            if (wasSelected && isCorrectOpt)
            {
                chipBg = Color.FromArgb("#388E3C");
                icon = "✅";
                suffix = " (tu respuesta — correcta)";
            }
            else if (wasSelected && !isCorrectOpt)
            {
                chipBg = Color.FromArgb("#C62828");
                icon = "❌";
                suffix = " (tu respuesta — incorrecta)";
            }
            else
            {
                chipBg = Color.FromArgb("#1565C0");
                icon = "💡";
                suffix = " (respuesta correcta)";
            }

            _layout.Children.Add(new Border
            {
                BackgroundColor = chipBg,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                Padding = new Thickness(10, 5),
                Margin = new Thickness(0, 2),
                Content = new Label
                {
                    Text = $"{icon} {lbl.Text}{suffix}",
                    TextColor = Colors.White,
                    FontSize = 13,
                },
            });
        }

        Container = MakeBorder(_layout, summaryColor);
    }

    private static Border MakeBorder(View content, Color stroke)
    {
        bool transparent = stroke == Colors.Transparent;
        return new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
            StrokeThickness = transparent ? 1.5 : 2.5,
            Stroke = transparent ? Color.FromArgb("#AAAAAA") : stroke,
            Padding = new Thickness(16),
            Content = content,
        };
    }
}

public class AnswerPayload
{
    public int QuestionId { get; set; }
    public List<int> SelectedOptionIds { get; set; } = new();
    public string? TextAnswer { get; set; }
}
