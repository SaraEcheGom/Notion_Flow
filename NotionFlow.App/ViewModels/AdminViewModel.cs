using System.Collections.ObjectModel;
using System.Windows.Input;
using NotionFlow.App.Models;
using NotionFlow.App.Services;
using NotionFlow.App.Views;

namespace NotionFlow.App.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly ApiService _api = new();

        public ObservableCollection<AuthResponse> Profesores { get; } = new();
        public ObservableCollection<AuthResponse> Estudiantes { get; } = new();
        public ObservableCollection<CursoResponse> Cursos { get; } = new();

        private AuthResponse? _profesorSeleccionado;
        private AuthResponse? _estudianteSeleccionado;
        private CursoResponse? _cursoSeleccionado;
        private string _nombreCurso = string.Empty;
        private string _materiaCurso = string.Empty;
        private string _nombreProfesor = string.Empty;
        private string _emailProfesor = string.Empty;
        private string _passwordProfesor = string.Empty;

        public AuthResponse? ProfesorSeleccionado
        {
            get => _profesorSeleccionado;
            set { _profesorSeleccionado = value; OnPropertyChanged(); }
        }

        public AuthResponse? EstudianteSeleccionado
        {
            get => _estudianteSeleccionado;
            set { _estudianteSeleccionado = value; OnPropertyChanged(); }
        }

        public CursoResponse? CursoSeleccionado
        {
            get => _cursoSeleccionado;
            set { _cursoSeleccionado = value; OnPropertyChanged(); }
        }

        public string NombreCurso
        {
            get => _nombreCurso;
            set { _nombreCurso = value; OnPropertyChanged(); }
        }

        public string MateriaCurso
        {
            get => _materiaCurso;
            set { _materiaCurso = value; OnPropertyChanged(); }
        }

        public string NombreProfesor
        {
            get => _nombreProfesor;
            set { _nombreProfesor = value; OnPropertyChanged(); }
        }

        public string EmailProfesor
        {
            get => _emailProfesor;
            set { _emailProfesor = value; OnPropertyChanged(); }
        }

        public string PasswordProfesor
        {
            get => _passwordProfesor;
            set { _passwordProfesor = value; OnPropertyChanged(); }
        }

        public ICommand CargarDatosCommand { get; }
        public ICommand CrearCursoCommand { get; }
        public ICommand AsignarEstudianteCommand { get; }
        public ICommand CerrarSesionCommand { get; }
        public ICommand IrACrearCursoCommand { get; }
        public ICommand IrACrearProfesorCommand { get; }
        public ICommand CrearProfesorCommand { get; }
        public ICommand VerPerfilCommand { get; }

        public AdminViewModel()
        {
            CargarDatosCommand = new Command(async () => await CargarDatosAsync());
            CrearCursoCommand = new Command(async () => await CrearCursoAsync());
            AsignarEstudianteCommand = new Command(async () => await AsignarEstudianteAsync());
            CerrarSesionCommand = new Command(async () => await CerrarSesionAsync());
            CrearProfesorCommand = new Command(async () => await CrearProfesorAsync());

            IrACrearCursoCommand = new Command(async () =>
                await Shell.Current.Navigation.PushAsync(new CrearCursoPage(this)));

            IrACrearProfesorCommand = new Command(async () =>
                await Shell.Current.Navigation.PushAsync(new CrearProfesorPage(this)));

            VerPerfilCommand = new Command<AuthResponse>(async (u) =>
                await Shell.Current.Navigation.PushAsync(
                    new PerfilUsuarioPage(new PerfilUsuarioViewModel(u))));

            _ = CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            try
            {
                var profesores = await _api.GetUsuariosPorRolAsync("Profesor");
                var estudiantes = await _api.GetUsuariosPorRolAsync("Estudiante");
                var cursos = await _api.GetTodosLosCursosAsync();

                Profesores.Clear();
                foreach (var p in profesores) Profesores.Add(p);

                Estudiantes.Clear();
                foreach (var e in estudiantes) Estudiantes.Add(e);

                Cursos.Clear();
                foreach (var c in cursos) Cursos.Add(c);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        private async Task CrearCursoAsync()
        {
            if (ProfesorSeleccionado == null ||
                string.IsNullOrWhiteSpace(NombreCurso) ||
                string.IsNullOrWhiteSpace(MateriaCurso))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Completa todos los campos", "OK");
                return;
            }

            try
            {
                await _api.CrearCursoAsync(NombreCurso, MateriaCurso, ProfesorSeleccionado.Id);
                await Shell.Current.DisplayAlertAsync("Éxito", "Curso creado", "OK");
                NombreCurso = string.Empty;
                MateriaCurso = string.Empty;
                await CargarDatosAsync();
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        private async Task CrearProfesorAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreProfesor) ||
                string.IsNullOrWhiteSpace(EmailProfesor) ||
                string.IsNullOrWhiteSpace(PasswordProfesor))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Completa todos los campos", "OK");
                return;
            }

            try
            {
                await _api.RegisterAsync(
                    NombreProfesor, EmailProfesor, PasswordProfesor,
                    "Profesor", "NOTIONFLOW_ADMIN_2024");

                await Shell.Current.DisplayAlertAsync("Éxito", "Profesor creado correctamente", "OK");
                NombreProfesor = string.Empty;
                EmailProfesor = string.Empty;
                PasswordProfesor = string.Empty;
                await CargarDatosAsync();
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        private async Task AsignarEstudianteAsync()
        {
            if (CursoSeleccionado == null || EstudianteSeleccionado == null)
            {
                await Shell.Current.DisplayAlertAsync("Error", "Selecciona un curso y un estudiante", "OK");
                return;
            }

            try
            {
                await _api.AsignarEstudianteAsync(CursoSeleccionado.Id, EstudianteSeleccionado.Id);
                await Shell.Current.DisplayAlertAsync("Éxito",
                    $"{EstudianteSeleccionado.Nombre} asignado a {CursoSeleccionado.Nombre}", "OK");
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        private async Task CerrarSesionAsync()
        {
            await new AuthService().CerrarSesionAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}