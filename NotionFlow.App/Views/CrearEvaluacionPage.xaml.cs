using NotionFlow.App.ViewModels;

namespace NotionFlow.App.Views;

public partial class CrearEvaluacionPage : ContentPage
{
    private readonly CursoViewModel _vm;

    public CrearEvaluacionPage(CursoViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
    }

    private async void OnGuardar(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TituloEntry.Text) ||
            string.IsNullOrWhiteSpace(PorcentajeEntry.Text))
        {
            await DisplayAlert("Error", "Completa los campos obligatorios", "OK");
            return;
        }

        try
        {
            await _vm.CrearEvaluacionAsync(
                TituloEntry.Text,
                DescripcionEntry.Text ?? string.Empty,
                double.Parse(PorcentajeEntry.Text));

            await DisplayAlert("Éxito", "Evaluación creada", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}