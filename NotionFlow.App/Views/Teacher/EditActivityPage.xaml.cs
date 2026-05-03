using NotionFlow.App.Models.Auth;
using NotionFlow.App.ViewModels.Teacher;

namespace NotionFlow.App.Views.Teacher;

public partial class EditActivityPage : ContentPage
{
    private readonly ActivityViewModel _vm;
    private readonly ActivityModel _activity;
    private readonly List<QuestionBuilder> _questionBuilders = new();

    public event Func<Task>? ActivityUpdated;

    public EditActivityPage(ActivityViewModel vm, ActivityModel activity)
    {
        InitializeComponent();
        _vm = vm;
        _activity = activity;
        TitleEntry.Text = activity.Title;
        DescriptionEditor.Text = activity.Description;
        PercentageEntry.Text = activity.PercentageValue.ToString();
        DueDatePicker.Date = activity.DueDate != default ? activity.DueDate.ToLocalTime() : DateTime.Today;
        foreach (var q in activity.Questions)
            AddExistingQuestion(q);
    }

    private void AddExistingQuestion(ActivityQuestionModel existing)
    {
        var builder = new QuestionBuilder(
            _questionBuilders.Count + 1,
            onRemove: (b) =>
            {
                _questionBuilders.Remove(b);
                QuestionsContainer.Children.Remove(b.Container);
                RenumberQuestions();
            }
        );
        // Pre-poblar datos
        builder._textEntry.Text = existing.Text;
        builder._typePicker.SelectedIndex = existing.QuestionType == "OpenText" ? 1 : 0;
        if (existing.QuestionType == "MultipleChoice" && existing.Options.Any())
        {
            builder._options.Clear();
            builder._optionsContainer.Children.Clear();
            foreach (var opt in existing.Options)
            {
                var ob = new OptionBuilder(b =>
                {
                    builder._options.Remove(b);
                    builder._optionsContainer.Children.Remove(b.Container);
                });
                ob._textEntry.Text = opt.Text;
                ob._isCorrectCheck.IsChecked = opt.IsCorrect;
                builder._options.Add(ob);
                builder._optionsContainer.Children.Add(ob.Container);
            }
        }
        _questionBuilders.Add(builder);
        QuestionsContainer.Children.Add(builder.Container);
    }

    private void OnAddQuestion(object sender, EventArgs e)
    {
        var builder = new QuestionBuilder(
            _questionBuilders.Count + 1,
            onRemove: (b) =>
            {
                _questionBuilders.Remove(b);
                QuestionsContainer.Children.Remove(b.Container);
                RenumberQuestions();
            }
        );
        _questionBuilders.Add(builder);
        QuestionsContainer.Children.Add(builder.Container);
    }

    private void RenumberQuestions()
    {
        for (int i = 0; i < _questionBuilders.Count; i++)
            _questionBuilders[i].UpdateNumber(i + 1);
    }

    private async void OnSave(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            await DisplayAlert("Error", "El título es obligatorio.", "OK");
            return;
        }
        if (!double.TryParse(PercentageEntry.Text, out var pct) || pct <= 0)
        {
            await DisplayAlert("Error", "Porcentaje inválido.", "OK");
            return;
        }
        if (!_questionBuilders.Any())
        {
            await DisplayAlert("Error", "Debe haber al menos una pregunta.", "OK");
            return;
        }
        var questions = new List<QuestionPayload>();
        foreach (var b in _questionBuilders)
        {
            var q = b.Build();
            if (q == null)
            {
                await DisplayAlert("Error", $"Pregunta {b.Number} incompleta.", "OK");
                return;
            }
            questions.Add(q);
        }
        try
        {
            await _vm.UpdateActivityAsync(
                _activity.Id,
                TitleEntry.Text.Trim(),
                DescriptionEditor.Text?.Trim() ?? string.Empty,
                DueDatePicker.Date is DateTime d2 ? d2 : DateTime.Today,
                pct,
                questions
            );
            await DisplayAlert("Éxito", "Actividad actualizada.", "OK");
            if (ActivityUpdated != null) await ActivityUpdated.Invoke();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
