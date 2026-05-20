using System.Windows.Input;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Auth
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

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
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(AuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            LoginCommand = new Command(async () => await LoginAsync());
            NavigateToRegisterCommand = new Command(async () =>
                await Shell.Current.GoToAsync(Constants.Routes.Register));
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Error", "Ingresa tu correo y contraseña", "OK");
                return;
            }

            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var user = await _authService.LoginAsync(Email, Password);
                if (Application.Current?.MainPage is AppShell shell)
                    await shell.ShowRoleTabsAsync(user.Role);
                else
                    await Shell.Current.DisplayAlert("Error", "No se pudo determinar la pantalla de inicio.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
