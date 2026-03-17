using NotionFlow.App.Models;

namespace NotionFlow.App.Services
{
    public class AuthService
    {
        private readonly ApiService _api = new();
        public static Usuario? UsuarioActual { get; private set; }

        public async Task<Usuario> LoginAsync(string email, string password)
        {
            var response = await _api.LoginAsync(email, password);

            UsuarioActual = new Usuario
            {
                Id = response.Id,
                Nombre = response.Nombre,
                Email = response.Email,
                Rol = response.Rol
            };

            return UsuarioActual;
        }


        public async Task RegistrarAsync(string nombre, string email,
            string password, string rol, string token)
        {
            await _api.RegisterAsync(nombre, email, password, rol, token);
        }

        public Task CerrarSesionAsync()
        {
            UsuarioActual = null;
            Preferences.Remove("jwt_token");
            return Task.CompletedTask;
        }
    }
}