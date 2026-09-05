using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;

namespace SudokuEcosystem.ViewModels
{
    public class MainMenuViewModel : INotifyPropertyChanged
    {
        private string _currentDifficulty = "Medium";
        private string _progressPercent = "0%";
        private string _currentTime = "00:00:00";

        // Наш мост к навигации WPF
        public System.Windows.Navigation.NavigationService NavigationService { get; set; }

        public string CurrentDifficulty
        {
            get => _currentDifficulty;
            set { _currentDifficulty = value; OnPropertyChanged(); }
        }

        public string ProgressPercent
        {
            get => _progressPercent;
            set { _progressPercent = value; OnPropertyChanged(); }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(); }
        }

        public ICommand SetDifficultyCommand { get; }
        public ICommand NewGameCommand { get; }
        public ICommand ContinueGameCommand { get; }
        public ICommand ShowLeaderboardCommand { get; }
        public ICommand ShowRulesCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ExitCommand { get; }

        // События оставляем для обратной совместимости, если они где-то используются
        public event Action<string> NavigateToPage;
        public event Action<GameViewModel> StartNewGame;
        public event Action ContinueGame;

        public MainMenuViewModel()
        {
            SetDifficultyCommand = new RelayCommand(SetDifficulty);

            // Перенаправляем команды напрямую на методы с NavigationService
            NewGameCommand = new RelayCommand(_ => StartNewGameWithDifficulty());
            ContinueGameCommand = new RelayCommand(_ => ContinueExistingGame());
            ShowLeaderboardCommand = new RelayCommand(_ => NavigateToLeaderboard());
            ShowRulesCommand = new RelayCommand(_ => ShowRules());
            ShowSettingsCommand = new RelayCommand(_ => NavigateToSettings());
            ExitCommand = new RelayCommand(_ => ExitApplication());
        }

        private void SetDifficulty(object parameter)
        {
            if (parameter is string difficulty)
            {
                CurrentDifficulty = difficulty;
            }
        }

        private void StartNewGameWithDifficulty()
        {
            var gameViewModel = new GameViewModel();
            gameViewModel.SetDifficulty(CurrentDifficulty);

            StartNewGame?.Invoke(gameViewModel);

            // Передаем gameViewModel прямо в конструктор страницы
            var gamePage = new SudokuEcosystem.Views.GamePage(gameViewModel);

            NavigationService?.Navigate(gamePage);
        }

        private void ContinueExistingGame()
        {
            ContinueGame?.Invoke();
            // Сюда можно будет добавить логику загрузки сохраненного GamePage, когда сделаете сохранения
        }

        private void NavigateToLeaderboard()
        {
            NavigateToPage?.Invoke("Leaderboard");
            // Переходим на страницу таблицы лидеров
            NavigationService?.Navigate(new SudokuEcosystem.Views.LeaderboardPage());
        }

        private void NavigateToSettings()
        {
            NavigateToPage?.Invoke("Settings");
            // Переходим на страницу настроек
            NavigationService?.Navigate(new SudokuEcosystem.Views.SettingsPage());
        }

        private void ShowRules()
        {
            string rules = @"ПРАВИЛА ИГРЫ СУДОКУ:

1. Цель игры: заполнить поле цифрами от 1 до 9 так,
   чтобы в каждой строке, каждом столбце и каждом 
   квадрате 3×3 все цифры были различны.

2. В начале игры некоторые клетки уже заполнены.
   Их нельзя изменять.

3. Вы можете вводить цифры в пустые клетки.

4. Для победы нужно правильно заполнить все клетки.

5. Чем сложнее уровень, тем меньше начальных цифр.

Удачи!";

            MessageBox.Show(rules, "Правила игры", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitApplication()
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}