using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Models;
using NotionFlow.App.Services;
using NotionFlow.App.Views;

namespace NotionFlow.App.ViewModels
{
    public class CursoViewModel : BaseViewModel
    {
        private readonly ApiService _api = new();
        private readonly string _cursoId;

        public string CursoNombre { get; }
        public bool EsProfesor { get; }

        public ObservableCollection<Evaluacion> Evaluaciones { get; } = new();
        public ObservableCollection<Contenido> Contenidos { get; } = new();
        public ObservableCollection<EstudianteItem> Estudiantes { get; } = new();

        public ICommand MostrarOpcionesCommand { get; }
        public ICommand VerPerfilEstudianteCommand { get; }

        public CursoViewModel(string cursoId, string cursoNombre, string rol)
        {
            _cursoId = cursoId;
            CursoNombre = cursoNombre;
            EsProfesor = rol == "Profesor";

            MostrarOpcionesCommand = new Command(async () =>
            {
                var opcion = await Shell.Current.DisplayActionSheet(
                    "¿Qué deseas agregar?", "Cancelar", null,
                    "Crear Evaluación", "Publicar Contenido");

                if (opcion == "Crear Evaluación")
                    await Shell.Current.Navigation.PushAsync(
                        new CrearEvaluacionPage(this));
                else if (opcion == "Publicar Contenido")
                    await Shell.Current.Navigation.PushAsync(
                        new PublicarContenidoPage(this));
            });

            VerPerfilEstudianteCommand = new Command<EstudianteItem>(async (e) =>
            {
                var usuario = new AuthResponse
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Email = e.Email,
                    Rol = "Estudiante"
                };
                await Shell.Current.Navigation.PushAsync(
                    new PerfilUsuarioPage(new PerfilUsuarioViewModel(usuario)));
            });

            _ = CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                var evaluaciones = await _api.GetEvaluacionesAsync(int.Parse(_cursoId));
                Evaluaciones.Clear();
                foreach (var e in evaluaciones) Evaluaciones.Add(e);

                var contenidos = await _api.GetContenidosAsync(int.Parse(_cursoId));
                Contenidos.Clear();
                foreach (var c in contenidos) Contenidos.Add(c);

                
                if (EsProfesor)
                {
                    var cursos = await _api.GetCursosProfesorAsync(
                        AuthService.UsuarioActual?.Id ?? string.Empty);
                    var curso = cursos.FirstOrDefault(c => c.Id.ToString() == _cursoId);
                    if (curso != null)
                    {
                        Estudiantes.Clear();
                        foreach (var e in curso.Estudiantes) Estudiantes.Add(e);
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        public async Task CrearEvaluacionAsync(string titulo,
            string descripcion, double porcentaje)
        {
            await _api.CrearEvaluacionAsync(int.Parse(_cursoId),
                titulo, descripcion, porcentaje);
            await CargarDatosAsync();
        }

        public async Task PublicarContenidoAsync(string titulo,
            string descripcion, string tipo, string url)
        {
            await _api.PublicarContenidoAsync(int.Parse(_cursoId),
                titulo, descripcion, tipo, url);
            await CargarDatosAsync();
        }
    }
}