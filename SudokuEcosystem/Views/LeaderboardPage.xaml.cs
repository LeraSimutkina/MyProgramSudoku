using SudokuEcosystem.Models;
using SudokuEcosystem.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SudokuEcosystem.Views
{
    public partial class LeaderboardPage : Page
    {
        private readonly ApiService _apiService = App.Api;

        public LeaderboardPage()
        {
            InitializeComponent();

            Loaded += LeaderboardPage_Loaded;
        }

        private async void LeaderboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLeaderboard();
        }

        private async Task LoadLeaderboard()
        {
            try
            {
                var data = await _apiService.GetLeaderboardAsync();

                var leaderboard = new ObservableCollection<LeaderboardRow>();

                int rank = 1;

                foreach (var player in data)
                {
                    leaderboard.Add(new LeaderboardRow
                    {
                        Rank = rank++,
                        PlayerName = player.Login,
                        Difficulty = player.Difficulty,
                        Score = player.Score,
                        CompletionTime = "-",
                        CompletedAt = player.CompletedAt.ToString("dd.MM.yyyy")
                    });
                }

                LeaderboardGrid.ItemsSource = leaderboard;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка загрузки лидерборда:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }

    public class LeaderboardRow
    {
        public int Rank { get; set; }

        public string PlayerName { get; set; }

        public string Difficulty { get; set; }

        public int Score { get; set; }

        public string CompletionTime { get; set; }

        public string CompletedAt { get; set; }
    }
}