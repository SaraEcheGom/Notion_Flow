using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _email = string.Empty;
        private string _password = string.Empty;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand GoRegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            GoRegisterCommand = new Command(async () => await GoRegisterAsync());
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Error", "Ingresa correo y contraseña", "OK");
                return;
            }

            try
            {
                var user = await _authService.LoginAsync(Email, Password);

                if (user.Rol == "Admin")
                    await Shell.Current.GoToAsync("//admin");
                else if (user.Rol == "Profesor")
                    await Shell.Current.GoToAsync($"//profesor?id={user.Id}&nombre={user.Nombre}");
                else if (user.Rol == "Estudiante")
                    await Shell.Current.GoToAsync($"//estudiante?id={user.Id}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task GoRegisterAsync()
        {
            await Shell.Current.GoToAsync("register");
        }
    }
}