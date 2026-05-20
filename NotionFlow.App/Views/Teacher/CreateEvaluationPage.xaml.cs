using NotionFlow.App.ViewModels.Course;

namespace NotionFlow.App.Views.Teacher;

public partial class CreateEvaluationPage : ContentPage
{
    private readonly CourseViewModel _vm;

    public CreateEvaluationPage(CourseViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DatePicker.Date = DateTime.Today;
    }

    private async void OnSave(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            await DisplayAlert("Error", "El título es obligatorio.", "OK");
            return;
        }

        if (!double.TryParse(PercentageEntry.Text,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var pct) || pct <= 0 || pct > 100)
        {
            await DisplayAlert("Error", "Ingresa un porcentaje válido entre 1 y 100.", "OK");
            return;
        }

        SaveButton.IsEnabled = false;
        SaveButton.Text = "Guardando...";

        try
        {
            await _vm.CreateEvaluationAsync(
                TitleEntry.Text.Trim(),
                DescriptionEditor.Text?.Trim() ?? string.Empty,
                pct,
                DatePicker.Date);

            await DisplayAlert("Éxito", "Evaluación creada correctamente.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Text = "Crear Evaluación";
        }
    }
}
