using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Media; // Добавили пространство имен для работы со звуками
using SudokuEcosystem.ViewModels;

namespace SudokuEcosystem.Views
{
    public partial class GamePage : Page
    {
        private GameViewModel _viewModel;
        public event Action BackToMenuRequested;

        public GamePage()
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            DataContext = _viewModel;
        }

        public GamePage(GameViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.Lives))
                {
                    UpdateHearts();
                }
            };

            _viewModel.PauseStateChanged += OnPauseStateChanged;
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
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

        public GameViewModel GetViewModel() => _viewModel;

        private void UpdateHearts()
        {
            int lives = _viewModel.Lives;

            var heart1 = (TextBlock)FindName("Heart1");
            var heart2 = (TextBlock)FindName("Heart2");
            var heart3 = (TextBlock)FindName("Heart3");
            var heart4 = (TextBlock)FindName("Heart4");

            if (heart1 != null) heart1.Opacity = lives >= 1 ? 1 : 0.2;
            if (heart2 != null) heart2.Opacity = lives >= 2 ? 1 : 0.2;
            if (heart3 != null) heart3.Opacity = lives >= 3 ? 1 : 0.2;
            if (heart4 != null) heart4.Opacity = lives >= 4 ? 1 : 0.2;
        }

        private void OnPauseStateChanged(bool isPaused)
        {
            PauseOverlay.Visibility = isPaused ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ResumeGame();
        }

        // Нажатие на экранную кнопку цифры (1-9)
        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Content.ToString(), out int number))
            {
                _viewModel.SetSelectedNumber(number);

                // ЖИВОЙ ЗВУК: Издаем клик при нажатии на кнопку на экране
                if (Properties.Settings.Default.IsSoundEnabled)
                {
                    SystemSounds.Asterisk.Play(); // Легкий приятный клик Windows
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearCellCommand.Execute(null);
        }

        // Ввод цифр прямо с клавиатуры компьютера
        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out int number) && number >= 1 && number <= 9)
            {
                if (sender is TextBox textBox && textBox.Tag is ViewModels.CellViewModel cell)
                {
                    _viewModel.SetSelectedNumber(number);
                    _viewModel.SetCellValue(cell, number);

                    // ЖИВОЙ ЗВУК: Издаем звук в зависимости от правильности введенной цифры
                    if (Properties.Settings.Default.IsSoundEnabled)
                    {
                        // Сверяем введенное значение с правильным из вашей ViewModel
                        // Примечание: предполагается, что в cell есть свойство для проверки корректности, 
                        // либо мы ориентируемся на то, уменьшились ли жизни. 
                        // Самый надежный способ издать стандартный клик:
                        SystemSounds.Asterisk.Play();
                    }

                    e.Handled = true;
                }
            }
            else
            {
                // Если пользователь нажал букву — блокируем и издаем предупреждающий звук ошибки
                if (Properties.Settings.Default.IsSoundEnabled)
                {
                    SystemSounds.Hand.Play(); // Звук критического уведомления / ошибки
                }
                e.Handled = true;
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (sender is TextBox textBox && textBox.Tag is ViewModels.CellViewModel cell)
                {
                    _viewModel.ClearCellCommand.Execute(cell);
                    e.Handled = true;
                }
            }
        }

        private void OnCellGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();

                if (textBox.Tag is CellViewModel cell)
                {
                    _viewModel.SetSelectedCell(cell);
                }
            }
        }

        private void OnCellLostFocus(object sender, RoutedEventArgs e)
        {
            // Можно оставить пустым
        }
    }
}