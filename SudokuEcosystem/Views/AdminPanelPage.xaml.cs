using System;
using System.Windows;
using System.Windows.Controls;
using static SudokuEcosystem.Services.ApiService;

namespace SudokuEcosystem.Views
{
    public partial class AdminPanelPage : Page
    {
        public AdminPanelPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                // Вызываем обновленный метод
                var users = await App.Api.GetAdminUsersAsync();
                UsersGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
            else
            {
                this.NavigationService?.Navigate(new MainMenuPage());
            }
        }
        private async void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Отключаем кнопку на время выполнения, чтобы юзер не кликал сто раз
                if (sender is Button btn) btn.IsEnabled = false;

                bool success = await App.Api.CreateDatabaseBackupAsync();

                if (success)
                {
                    MessageBox.Show("Резервная копия базы данных успешно создана на сервере!",
                        "Резервное копирование", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Сервер не смог выполнить резервное копирование. Проверьте права доступа SQL-сервера к папке.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении запроса бэкапа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Включаем кнопку обратно
                if (sender is Button btn) btn.IsEnabled = true;
            }
        }
        private async void BanButton_Click(object sender, RoutedEventArgs e)
        {
            // Используем наш AdminUserResponse
            if (sender is Button btn && btn.Tag is AdminUserResponse selectedUser)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите заблокировать пользователя {selectedUser.Login} на 7 дней?",
                    "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Передает ID в метод ban/{userId}
                    bool success = await App.Api.BanUserAsync(selectedUser.Id, 7);
                    if (success)
                    {
                        MessageBox.Show($"Пользователь {selectedUser.Login} успешно забанен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers(); // Перезагружаем таблицу
                    }
                }
            }
        }

        private async void UnbanButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is AdminUserResponse selectedUser)
            {
                // Передает ID в метод unban/{userId}
                bool success = await App.Api.UnbanUserAsync(selectedUser.Id);
                if (success)
                {
                    MessageBox.Show($"Пользователь {selectedUser.Login} успешно разблокирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers(); // Перезагружаем таблицу
                }
            }
        }
    }
}