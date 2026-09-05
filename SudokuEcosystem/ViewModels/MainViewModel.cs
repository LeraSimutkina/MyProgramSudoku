using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SudokuEcosystem.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentPage;

        public object CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); }
        }

        public ICommand NavigateToGameCommand { get; }
        public ICommand NavigateToLeaderboardCommand { get; }

        public MainViewModel()
        {
            NavigateToGameCommand = new RelayCommand(_ => NavigateToGame());
            NavigateToLeaderboardCommand = new RelayCommand(_ => NavigateToLeaderboard());

            // По умолчанию показываем игровую страницу
            CurrentPage = null;
        }

        private void NavigateToGame()
        {
        }

        private void NavigateToLeaderboard()
        {
            CurrentPage = new Views.LeaderboardPage();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
