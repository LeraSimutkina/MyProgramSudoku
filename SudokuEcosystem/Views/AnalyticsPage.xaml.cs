using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace SudokuEcosystem.Views
{
    public partial class AnalyticsPage : Page
    {
        public AnalyticsPage()
        {
            InitializeComponent();
        }

        // Кнопка возврата в меню
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

        // Событие, которое срабатывает при открытии страницы
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Вызываем метод ApiService, который мы сейчас добавим на Шаге 3
                var statsData = await App.Api.GetPlayerStatsAsync();

                if (statsData != null && statsData.Count > 0)
                {
                    StatsItemsControl.ItemsSource = statsData;
                    LoadingText.Visibility = Visibility.Collapsed; // Скрываем текст загрузки
                }
                else
                {
                    LoadingText.Text = "📝 У вас пока нет сыгранных игр. Статистика появится после первой победы!";
                }
            }
            catch (Exception ex)
            {
                LoadingText.Text = "❌ Не удалось загрузить аналитику прогресса.";
                MessageBox.Show($"Ошибка связи с сервером: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Модель данных для десериализации ответа от API
    public class PlayerDifficultyStats
    {
        public string Difficulty { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
        public double AverageScore { get; set; }
        public int BestScore { get; set; }
        public double AverageTime { get; set; }
    }
}