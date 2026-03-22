using System.Collections.ObjectModel;
using NotionFlow.App.Models;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels
{
    public class PerfilUsuarioViewModel : BaseViewModel
    {
        private readonly ApiService _api = new();

        public string Nombre { get; }
        public string Email { get; }
        public string Rol { get; }
        public string Inicial => string.IsNullOrEmpty(Nombre) ? "?" :
            Nombre[0].ToString().ToUpper();

        public ObservableCollection<CursoResponse> Cursos { get; } = new();

        public PerfilUsuarioViewModel(AuthResponse usuario)
        {
            Nombre = usuario.Nombre;
            Email = usuario.Email;
            Rol = usuario.Rol;
            _ = CargarCursosAsync(usuario);
        }

        private async Task CargarCursosAsync(AuthResponse usuario)
        {
            try
            {
                List<CursoResponse> cursos;

                if (usuario.Rol == "Profesor")
                    cursos = await _api.GetCursosProfesorAsync(usuario.Id);
                else
                    cursos = await _api.GetCursosEstudianteAsync(usuario.Id);

                Cursos.Clear();
                foreach (var c in cursos) Cursos.Add(c);
            }
            catch { }
        }
    }
}