using System;
using System.IO; // ОБЯЗАТЕЛЬНО: нужно для работы с файлом user_visit.txt
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SudokuEcosystem.ViewModels;

namespace SudokuEcosystem.Views
{
    public partial class MainMenuPage : Page
    {
        private MainMenuViewModel _viewModel;

        // Переменная для отслеживания 24 часов неактивности (просрока)
        private readonly string _visitLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_visit.txt");

        // 1. Конструктор по умолчанию (чтобы проект не падал, если WPF вызывает его сам)
        public MainMenuPage()
        {
            InitializeComponent();
            if (Properties.Settings.Default.CurrentTheme == "Классическая темная")
            {
                this.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#212121"));
            }
            else
            {
                this.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFF0F5"));
            }

            if (_viewModel == null)
            {
                _viewModel = new MainMenuViewModel();
            }

            DataContext = _viewModel;
            this.Loaded += MainMenuPage_Loaded; // Подписываемся на событие загрузки страницы
        }

        // 2. Конструктор с параметром (который вызывается из MainWindow)
        public MainMenuPage(MainMenuViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            this.Loaded += MainMenuPage_Loaded; // Подписываемся на событие загрузки страницы
        }

        // Срабатывает при появлении страницы на экране
        private void MainMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.NavigationService = this.NavigationService;
            }
            ShowAdminButtonIfNeeded(App.CurrentUserRole);

            // Запускаем проверку отсутствия пользователя более 24 часов
            CheckUserInactivity();
        }

        // Логика проверки 24 часов отсутствия
        private void CheckUserInactivity()
        {
            try
            {
                if (File.Exists(_visitLogPath))
                {
                    string fileContent = File.ReadAllText(_visitLogPath);

                    if (DateTime.TryParse(fileContent, out DateTime lastVisit))
                    {
                        TimeSpan inactiveTime = DateTime.Now - lastVisit;

                        if (inactiveTime.TotalHours >= 24)
                        {
                            MessageBox.Show(
                                "Внимание! Вы не заходили в приложение более 24 часов.\n\n" +
                                "🧠 Регулярные тренировки улучшают память и концентрацию. " +
                                "Пора размять мозги и решить новое Судоку!",
                                "🔔 Системное уведомление",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
                        }
                    }
                }

                // Перезаписываем текущую дату
                File.WriteAllText(_visitLogPath, DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка уведомлений: {ex.Message}");
            }
        }

        // Клик по кнопке НАЧАТЬ ИГРУ (исправленный, через GameViewModel)
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Получаем выбранную сложность из ViewModel главного меню (или "Easy" по умолчанию)
                string difficulty = _viewModel?.CurrentDifficulty ?? "Easy";

                // 2. Создаем экземпляр ViewModel для игрового экрана
                var gameViewModel = new GameViewModel();

                // 3. Вызываем ваш родной метод для установки сложности и генерации новой игры
                gameViewModel.SetDifficulty(difficulty);

                // 4. Передаем полностью готовую и инициализированную ViewModel в конструктор страницы игры
                var gamePage = new GamePage(gameViewModel);

                // 5. Переключаем экран
                this.NavigationService?.Navigate(gamePage);
            }
            catch (Exception ex)
            {
                // Если что-то упало внутри инициализации игры, покажем понятную ошибку
                MessageBox.Show($"Не удалось запустить игру.\nОшибка: {ex.Message}",
                                "Ошибка навигации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Клик по кнопкам сложности
        private void Difficulty_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string difficulty = button.Tag.ToString();

                if (_viewModel != null)
                {
                    _viewModel.CurrentDifficulty = difficulty;
                }

                // Вызываем ваш метод безопасного обновления стилей кнопок
                UpdateDifficultyStyles(difficulty);
            }
        }


        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            Window.GetWindow(this)?.Close();
        }
        // Метод динамического поиска WrapPanel и перекраски кнопок
        private void UpdateDifficultyStyles(string selectedDifficulty)
        {
            try
            {
                if (this.Content is Grid mainGrid)
                {
                    foreach (var child in mainGrid.Children)
                    {
                        if (child is Grid centerGrid)
                        {
                            foreach (var centerChild in centerGrid.Children)
                            {
                                if (centerChild is Border border && border.Child is StackPanel stack)
                                {
                                    foreach (var stackChild in stack.Children)
                                    {
                                        if (stackChild is WrapPanel wrapPanel)
                                        {
                                            UpdateButtonsInWrapPanel(wrapPanel, selectedDifficulty);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // Вспомогательный метод перекраски кнопок внутри WrapPanel
        private void UpdateButtonsInWrapPanel(WrapPanel wrapPanel, string selectedDifficulty)
        {
            foreach (var btn in wrapPanel.Children)
            {
                if (btn is Button button && button.Tag != null)
                {
                    if (button.Tag.ToString() == selectedDifficulty)
                    {
                        button.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 105, 180)); // Розовый акцент
                        button.Foreground = System.Windows.Media.Brushes.White;
                    }
                    else
                    {
                        if (button.Tag.ToString() == "Easy") button.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 192, 203));
                        if (button.Tag.ToString() == "Medium") button.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 250));
                        if (button.Tag.ToString() == "Hard") button.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(216, 191, 216));

                        button.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 0, 74));
                    }
                }
            }
        }

        private void AnalyticsButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new AnalyticsPage());
        }

        private void LeaderboardButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new LeaderboardPage());
        }

        private void RulesButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new RulesPage());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new SettingsPage());
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void ShowAdminButtonIfNeeded(string role)
        {
            if (role == "Admin" && AdminPanelBtn != null)
            {
                AdminPanelBtn.Visibility = Visibility.Visible;
            }
        }

        private void AdminPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            var adminPanel = new AdminPanelPage();
            NavigationService?.Navigate(adminPanel);
        }
    }
}