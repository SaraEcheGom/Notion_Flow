using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Models;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels
{
    public class EstudianteViewModel : BaseViewModel
    {
        private readonly ApiService _api = new();
        private readonly string _estudianteId;

        public ObservableCollection<CursoResponse> Cursos { get; } = new();
        public ICommand IrACursoCommand { get; }
        public ICommand CerrarSesionCommand { get; }

        public EstudianteViewModel(string estudianteId)
        {
            _estudianteId = estudianteId;
            IrACursoCommand = new Command<CursoResponse>(async (c) => await IrACursoAsync(c));
            CerrarSesionCommand = new Command(async () => await CerrarSesionAsync());
            _ = CargarCursosAsync();
        }

        private async Task CargarCursosAsync()
        {
            try
            {
                var lista = await _api.GetCursosEstudianteAsync(_estudianteId);
                Cursos.Clear();
                foreach (var c in lista) Cursos.Add(c);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task IrACursoAsync(CursoResponse curso)
        {
            await Shell.Current.GoToAsync(
                $"curso?cursoId={curso.Id}&cursoNombre={curso.Nombre}&rol=Estudiante");
        }

        private async Task CerrarSesionAsync()
        {
            await new AuthService().CerrarSesionAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}