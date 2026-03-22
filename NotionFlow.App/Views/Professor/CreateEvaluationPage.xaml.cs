using NotionFlow.App.ViewModels.Course;

namespace NotionFlow.App.Views.Professor;

public partial class CreateEvaluationPage : ContentPage
{
    private readonly CourseViewModel _vm;

    public CreateEvaluationPage(CourseViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
    }

    private async void OnSave(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text) ||
            string.IsNullOrWhiteSpace(PercentageEntry.Text))
        {
            await DisplayAlert("Error", "Complete the required fields", "OK");
            return;
        }

        try
        {
            await _vm.CreateEvaluationAsync(
                TitleEntry.Text,
                DescriptionEntry.Text ?? string.Empty,
                double.Parse(PercentageEntry.Text));

            await DisplayAlert("Success", "Evaluation created", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }
}
