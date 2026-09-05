using System.Windows;
using SudokuEcosystem.Services;
using WpfApp1;

namespace SudokuEcosystem.Views
{
    public partial class LoginWindow : Window
    {
        private ApiService _apiService;

        public LoginWindow()
        {
            InitializeComponent();
            _apiService = App.Api;
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            // Используем наш единый App.Api
            var (success, role) = await App.Api.LoginAsync(LoginBox.Text, PasswordBox.Password);

            if (success)
            {

                App.CurrentUserRole = role;
                var mainWindow = new MainWindow();
                mainWindow.Show();

               

                this.Close();
            }
        }
        private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            var success = await _apiService.RegisterAsync(LoginBox.Text, PasswordBox.Password);
        }
    }
}