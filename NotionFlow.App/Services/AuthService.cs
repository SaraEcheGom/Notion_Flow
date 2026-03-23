using System.Diagnostics;
using NotionFlow.App.Models;
using NotionFlow.App.Models.Auth;

namespace NotionFlow.App.Services
{
    public class AuthService
    {
        private readonly ApiService _apiService;
        public static User? CurrentUser { get; private set; }

        public AuthService(ApiService apiService)
        {
            _apiService = apiService;
            Debug.WriteLine("✓ AuthService initialized");
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            Debug.WriteLine($"🔐 [AuthService] Starting LoginAsync for {email}");
            try
            {
                Debug.WriteLine($"📡 [AuthService] Calling ApiService.LoginAsync");
                var response = await _apiService.LoginAsync(email, password);

                Debug.WriteLine($"✓ [AuthService] Response received. ID: {response.Id}, Role: {response.Role}");

                CurrentUser = new User
                {
                    Id = response.Id,
                    Name = response.Name,
                    Email = response.Email,
                    Role = response.Role
                };

                Debug.WriteLine($"✓ [AuthService] CurrentUser set. Role: {CurrentUser.Role}");
                return CurrentUser;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ [AuthService] Error: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
        }

        public async Task RegisterAsync(string name, string email,
            string password, string role, string token)
        {
            Debug.WriteLine($"📝 [AuthService] Registering user: {email}");
            await _apiService.RegisterAsync(name, email, password, role, token);
        }

        public static Task LogoutAsync()
        {
            Debug.WriteLine("🔓 [AuthService] Logout");
            CurrentUser = null;
            Preferences.Remove("jwt_token");
            return Task.CompletedTask;
        }
    }
}