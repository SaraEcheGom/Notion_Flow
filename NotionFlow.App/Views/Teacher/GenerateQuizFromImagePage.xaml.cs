using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Teacher;

namespace NotionFlow.App.Views.Teacher;

public partial class GenerateQuizFromImagePage : ContentPage
{
    private readonly ApiService _api;
    private readonly ActivityViewModel _vm;
    private FileResult? _selectedImage;
    private GeneratedQuizResponse? _generatedQuiz;

    public GenerateQuizFromImagePage(ApiService api, ActivityViewModel vm)
    {
        InitializeComponent();
        _api = api;
        _vm = vm;
    }

    // ── Image picker ─────────────────────────────────────────────────────────

    private async void OnPickImage(object sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Seleccionar imagen del examen",
                FileTypes = FilePickerFileType.Images,
            };

            var result = await FilePicker.PickAsync(options);
            if (result == null) return;

            _selectedImage = result;
            ImagePreview.Source = ImageSource.FromFile(result.FullPath);
            ImagePreview.IsVisible = true;
            PickImageButton.Text = "Cambiar imagen";
            GenerateButton.IsEnabled = true;

            HideError();
            HideQuizPreview();
        }
        catch (Exception ex)
        {
            ShowError($"No se pudo cargar la imagen: {ex.Message}");
        }
    }

    // ── Generate ─────────────────────────────────────────────────────────────

    private async void OnGenerate(object sender, EventArgs e)
    {
        if (_selectedImage == null) return;

        SetLoading(true);
        HideError();
        HideQuizPreview();

        try
        {
            byte[] imageData;
            using (var stream = await _selectedImage.OpenReadAsync())
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                imageData = ms.ToArray();
            }

            _generatedQuiz = await _api.GenerateQuizFromImageAsync(
                imageData,
                _selectedImage.FileName,
                _selectedImage.ContentType ?? "image/jpeg");

            if (_generatedQuiz.Questions.Count == 0)
            {
                ShowError("No se encontraron preguntas en la imagen. Intenta con una foto más clara.");
                return;
            }

            RenderQuizPreview(_generatedQuiz);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ── Render quiz preview ───────────────────────────────────────────────────

    private void RenderQuizPreview(GeneratedQuizResponse quiz)
    {
        QuizTitleLabel.Text = quiz.Title;
        QuestionsContainer.Children.Clear();

        foreach (var question in quiz.Questions)
            QuestionsContainer.Add(BuildQuestionView(question));

        QuizPreviewSection.IsVisible = true;
        SaveButton.IsVisible = true;
    }

    private static View BuildQuestionView(GeneratedQuestionItem question)
    {
        var container = new Border
        {
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#E0E0E0"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
            Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 0, 4),
            Content = BuildQuestionContent(question)
        };
        return container;
    }

    private static VerticalStackLayout BuildQuestionContent(GeneratedQuestionItem question)
    {
        var layout = new VerticalStackLayout { Spacing = 10 };

        var typeTag = question.Type switch
        {
            "single_choice"   => "Selección única",
            "multiple_choice" => "Selección múltiple",
            _                 => "Respuesta abierta"
        };

        var header = new HorizontalStackLayout { Spacing = 8 };
        header.Add(new Label
        {
            Text = $"{question.Id}.",
            FontAttributes = FontAttributes.Bold,
            FontSize = 14,
            TextColor = Color.FromArgb("#1A4A32"),
            VerticalOptions = LayoutOptions.Start
        });
        header.Add(new Label
        {
            Text = question.Text,
            FontAttributes = FontAttributes.Bold,
            FontSize = 14,
            TextColor = Color.FromArgb("#1C1C1E"),
            HorizontalOptions = LayoutOptions.FillAndExpand,
            LineBreakMode = LineBreakMode.WordWrap
        });
        layout.Add(header);

        layout.Add(new Label
        {
            Text = typeTag,
            FontSize = 11,
            TextColor = Color.FromArgb("#757575"),
            Margin = new Thickness(22, 0, 0, 0)
        });

        switch (question.Type)
        {
            case "open":
                layout.Add(new Editor
                {
                    Placeholder = "Espacio para respuesta...",
                    HeightRequest = 72,
                    Margin = new Thickness(22, 4, 0, 0),
                    BackgroundColor = Color.FromArgb("#F5F5F5"),
                    FontSize = 13
                });
                break;

            case "single_choice" when question.Options?.Count > 0:
                var groupName = $"q{question.Id}";
                foreach (var option in question.Options)
                {
                    var row = new HorizontalStackLayout
                    {
                        Spacing = 8,
                        Margin = new Thickness(22, 2, 0, 0)
                    };
                    row.Add(new RadioButton
                    {
                        GroupName = groupName,
                        VerticalOptions = LayoutOptions.Center
                    });
                    row.Add(new Label
                    {
                        Text = option,
                        FontSize = 13,
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.WordWrap
                    });
                    layout.Add(row);
                }
                break;

            case "multiple_choice" when question.Options?.Count > 0:
                foreach (var option in question.Options)
                {
                    var row = new HorizontalStackLayout
                    {
                        Spacing = 8,
                        Margin = new Thickness(22, 2, 0, 0)
                    };
                    row.Add(new CheckBox { VerticalOptions = LayoutOptions.Center });
                    row.Add(new Label
                    {
                        Text = option,
                        FontSize = 13,
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.WordWrap
                    });
                    layout.Add(row);
                }
                break;
        }

        return layout;
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    private async void OnSave(object sender, EventArgs e)
    {
        if (_generatedQuiz == null) return;

        var percentageStr = await DisplayPromptAsync(
            "Guardar cuestionario",
            "Ingresa el porcentaje sobre la nota (ej: 20):",
            placeholder: "20",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(percentageStr)) return;
        if (!double.TryParse(percentageStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var pct) || pct <= 0)
        {
            await DisplayAlert("Error", "Ingresa un porcentaje válido mayor a 0.", "OK");
            return;
        }

        var questions = MapToQuestionPayloads(_generatedQuiz);

        SaveButton.IsEnabled = false;
        try
        {
            await _vm.CreateActivityAsync(
                title:       _generatedQuiz.Title,
                description: "Generado automáticamente desde imagen.",
                dueDate:     DateTime.Today.AddDays(7),
                pct:         pct,
                questions:   questions);

            await DisplayAlert("Éxito", "Cuestionario guardado como actividad.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private static List<QuestionPayload> MapToQuestionPayloads(GeneratedQuizResponse quiz)
    {
        var result = new List<QuestionPayload>();
        foreach (var q in quiz.Questions)
        {
            if (q.Type == "open")
            {
                result.Add(new QuestionPayload
                {
                    Text = q.Text,
                    QuestionType = "OpenText",
                    Options = new List<OptionPayload>()
                });
            }
            else
            {
                var options = q.Options?.Select(o => new OptionPayload
                {
                    Text = o,
                    IsCorrect = false
                }).ToList() ?? new List<OptionPayload>();

                result.Add(new QuestionPayload
                {
                    Text = q.Text,
                    QuestionType = "MultipleChoice",
                    Options = options
                });
            }
        }
        return result;
    }

    // ── UI state helpers ──────────────────────────────────────────────────────

    private void SetLoading(bool loading)
    {
        LoadingSection.IsVisible = loading;
        GenerateButton.IsEnabled = !loading && _selectedImage != null;
        PickImageButton.IsEnabled = !loading;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void HideError() => ErrorLabel.IsVisible = false;

    private void HideQuizPreview()
    {
        QuizPreviewSection.IsVisible = false;
        SaveButton.IsVisible = false;
        _generatedQuiz = null;
    }
}
