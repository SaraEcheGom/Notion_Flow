using NotionFlow.App.ViewModels;
using NotionFlow.App.ViewModels.Course;

namespace NotionFlow.App.Views.Professor;

public partial class PublishContentPage : ContentPage
{
    private readonly CourseViewModel _vm;

    public PublishContentPage(CourseViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        TipoPicker.ItemsSource = new List<string> { "Texto", "Enlace", "Archivo" };
    }

    private async void OnPublicar(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TituloEntry.Text))
        {
            await DisplayAlert("Error", "El título es obligatorio", "OK");
            return;
        }

        try
        {
            await _vm.PublishContentAsync(
                TituloEntry.Text,
                DescripcionEntry.Text ?? string.Empty,
                TipoPicker.SelectedItem?.ToString() ?? "Texto",
                UrlEntry.Text ?? string.Empty);

            await DisplayAlert("Éxito", "Contenido publicado", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
