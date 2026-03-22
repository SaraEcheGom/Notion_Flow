using NotionFlow.App.Models;
using NotionFlow.App.Models.Auth;

namespace NotionFlow.App.Services
{
    public class AuthService
    {
        private readonly ApiService _api = new();
        public static User? CurrentUser { get; private set; }

        public async Task<User> LoginAsync(string email, string password)
        {
            var response = await _api.LoginAsync(email, password);

            CurrentUser = new User
            {
                Id = response.Id,
                Name = response.Name,
                Email = response.Email,
                Role = response.Role
            };

            return CurrentUser;
        }

        public async Task RegisterAsync(string name, string email,
            string password, string role, string token)
        {
            await _api.RegisterAsync(name, email, password, role, token);
        }

        public Task LogoutAsync()
        {
            CurrentUser = null;
            Preferences.Remove("jwt_token");
            return Task.CompletedTask;
        }
    }
}