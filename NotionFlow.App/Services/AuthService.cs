using System.Diagnostics;
using NotionFlow.App.Models;
using NotionFlow.App.Models.Auth;

namespace NotionFlow.App.Services
{
    /// <summary>
    /// Capa de autenticación de la aplicación.
    /// Mantiene el estado de sesión del usuario actual (CurrentUser) y delega
    /// operaciones HTTP a ApiService. Es el único punto que muta CurrentUser.
    /// Registrado como Singleton en MauiProgram para que CurrentUser sea accesible
    /// desde cualquier ViewModel sin estado estático.
    /// </summary>
    public class AuthService
    {
        private readonly ApiService _apiService;

        /// <summary>Usuario en sesión. Null cuando no hay sesión activa.</summary>
        public User? CurrentUser { get; private set; }

        public AuthService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var response = await _apiService.LoginAsync(email, password);

            CurrentUser = new User
            {
                Id = response.Id,
                Name = response.Name,
                Email = response.Email,
                Role = response.Role,
                InstitutionId = response.InstitutionId
            };

            return CurrentUser;
        }

        public async Task RegisterAsync(string name, string email,
            string password, string role, string token)
        {
            await _apiService.RegisterAsync(name, email, password, role, token);
        }

        public async Task LogoutAsync()
        {
            CurrentUser = null;
            try
            {
                SecureStorage.Remove("jwt_token");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] Error limpiando token: {ex.GetType().Name}");
            }
            await Task.CompletedTask;
        }
    }
}
