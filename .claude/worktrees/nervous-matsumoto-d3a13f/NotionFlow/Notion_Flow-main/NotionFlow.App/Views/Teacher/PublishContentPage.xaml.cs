using NotionFlow.App.ViewModels.Course;

namespace NotionFlow.App.Views.Teacher;

public partial class PublishContentPage : ContentPage
{
    private readonly CourseViewModel _vm;

    public PublishContentPage(CourseViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    private async void OnPublish(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text) ||
            string.IsNullOrWhiteSpace(UrlEntry.Text) ||
            TypePicker.SelectedIndex < 0)
        {
            await DisplayAlert("Error", "Complete all required fields", "OK");
            return;
        }

        try
        {
            var type = (string)TypePicker.SelectedItem;
            await _vm.PublishContentAsync(
                TitleEntry.Text,
                DescriptionEntry.Text ?? string.Empty,
                type,
                UrlEntry.Text);

            await DisplayAlert("Success", "Content published", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }
}
