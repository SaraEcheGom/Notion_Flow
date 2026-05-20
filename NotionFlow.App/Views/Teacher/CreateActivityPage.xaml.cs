using NotionFlow.App.ViewModels.Teacher;

namespace NotionFlow.App.Views.Teacher;

public partial class CreateActivityPage : ContentPage
{
    private readonly ActivityViewModel _vm;
    private readonly List<QuestionBuilder> _questionBuilders = new();

    public event Func<Task>? ActivityCreated;

    public CreateActivityPage(ActivityViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
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
            await DisplayAlert("Error", "Ingresa un porcentaje válido.", "OK");
            return;
        }
        if (!_questionBuilders.Any())
        {
            await DisplayAlert("Error", "Agrega al menos una pregunta.", "OK");
            return;
        }
        var questions = new List<QuestionPayload>();
        foreach (var b in _questionBuilders)
        {
            var q = b.Build();
            if (q == null)
            {
                await DisplayAlert("Error", $"La pregunta {b.Number} está incompleta.", "OK");
                return;
            }
            questions.Add(q);
        }
        try
        {
            await _vm.CreateActivityAsync(
                TitleEntry.Text.Trim(),
                DescriptionEditor.Text?.Trim() ?? string.Empty,
                DueDatePicker.Date is DateTime d1 ? d1 : DateTime.Today,
                pct,
                questions
            );
            await DisplayAlert("Éxito", "Actividad creada correctamente.", "OK");
            if (ActivityCreated != null) await ActivityCreated.Invoke();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}

internal class QuestionBuilder
{
    public int Number { get; private set; }
    public Border Container { get; }
    internal readonly Entry _textEntry;
    internal readonly Picker _typePicker;
    internal readonly VerticalStackLayout _optionsContainer;
    internal readonly List<OptionBuilder> _options = new();
    private readonly Label _headerLabel;
    private readonly Action<QuestionBuilder> _onRemove;

    public QuestionBuilder(int number, Action<QuestionBuilder> onRemove)
    {
        Number = number;
        _onRemove = onRemove;
        _headerLabel = new Label
        {
            Text = $"Pregunta {number}",
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
        };
        _textEntry = new Entry { Placeholder = "Texto de la pregunta" };
        _typePicker = new Picker
        {
            Title = "Tipo",
            ItemsSource = new List<string> { "MultipleChoice", "OpenText" },
        };
        _typePicker.SelectedIndex = 0;

        _optionsContainer = new VerticalStackLayout { Spacing = 6 };

        _typePicker.SelectedIndexChanged += (s, e) =>
        _optionsContainer.IsVisible = (string)_typePicker.SelectedItem == "MultipleChoice";

        var addOptBtn = new Button
        {
            Text = "+ Opción",
            FontSize = 12,
            Padding = new Thickness(10, 6),
            CornerRadius = 6,
        };
        addOptBtn.Clicked += (s, e) => AddOption();
        var removeBtn = new Button
        {
            Text = "✕ Quitar pregunta",
            FontSize = 11,
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#E53935"),
            Padding = new Thickness(0),
        };
        removeBtn.Clicked += (s, e) => _onRemove(this);
        Container = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
            StrokeThickness = 1,
            Padding = new Thickness(14),
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    _headerLabel,
                    _textEntry,
                    _typePicker,
                    _optionsContainer,
                    addOptBtn,
                    removeBtn,
                },
            },
        };
        AddOption();
        AddOption();
    }

    public void UpdateNumber(int n)
    {
        Number = n;
        _headerLabel.Text = $"Pregunta {n}";
    }

    private void AddOption()
    {
        var opt = new OptionBuilder(b =>
        {
            _options.Remove(b);
            _optionsContainer.Children.Remove(b.Container);
        });
        _options.Add(opt);
        _optionsContainer.Children.Add(opt.Container);
    }

    public QuestionPayload? Build()
    {
        var text = _textEntry.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return null;
        var type = _typePicker.SelectedItem as string ?? "MultipleChoice";
        var options = new List<OptionPayload>();
        if (type == "MultipleChoice")
        {
            foreach (var o in _options)
            {
                var b = o.Build();
                if (b != null)
                    options.Add(b);
            }
            if (!options.Any())
                return null;
        }
        return new QuestionPayload
        {
            Text = text,
            QuestionType = type,
            Options = options,
        };
    }
}

internal class OptionBuilder
{
    public Grid Container { get; }
    internal readonly Entry _textEntry;
    internal readonly CheckBox _isCorrectCheck;
    private readonly Action<OptionBuilder> _onRemove;

    public OptionBuilder(Action<OptionBuilder> onRemove)
    {
        _onRemove = onRemove;
        _textEntry = new Entry
        {
            Placeholder = "Texto de la opción",
            HorizontalOptions = LayoutOptions.FillAndExpand,
        };
        _isCorrectCheck = new CheckBox { VerticalOptions = LayoutOptions.Center };
        var removeBtn = new Button
        {
            Text = "✕",
            FontSize = 11,
            WidthRequest = 36,
            HeightRequest = 36,
            Padding = new Thickness(0),
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#E53935"),
        };
        removeBtn.Clicked += (s, e) => _onRemove(this);
        var checkLabel = new Label { Text = "✓", VerticalOptions = LayoutOptions.Center };
        Container = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
            ColumnSpacing = 6,
        };
        Container.Add(_textEntry, 0);
        Container.Add(checkLabel, 1);
        Container.Add(_isCorrectCheck, 2);
        Container.Add(removeBtn, 3);
    }

    public OptionPayload? Build()
    {
        var text = _textEntry.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return null;
        return new OptionPayload { Text = text, IsCorrect = _isCorrectCheck.IsChecked };
    }
}
