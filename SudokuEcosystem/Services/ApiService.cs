using Newtonsoft.Json;
using SudokuEcosystem.Models;
using SudokuEcosystem.Views;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SudokuEcosystem.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string _authToken;
        private int _currentUserId;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7067/");
        }

        // РЕГИСТРАЦИЯ
        public async Task<bool> RegisterAsync(string login, string password)
        {
            try
            {
                var data = new { login = login, password = password };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Регистрация успешна! Теперь войдите.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }

                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Ошибка: {error}", "Регистрация не удалась",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public async Task<List<SudokuEcosystem.Views.PlayerDifficultyStats>> GetPlayerStatsAsync()
        {
            // _httpClient у вас уже настроен и содержит токен авторизации
            HttpResponseMessage response = await _httpClient.GetAsync("api/progress/stats");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                // Распаковываем массив JSON, прилетевший от сервера, в наш список объектов
                return JsonConvert.DeserializeObject<List<SudokuEcosystem.Views.PlayerDifficultyStats>>(json);
            }

            return new List<SudokuEcosystem.Views.PlayerDifficultyStats>();
        }
        // ВХОД
        public async Task<(bool success, string role)> LoginAsync(string login, string password)
        {
            try
            {
                var data = new { login, password };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<LoginResponse>(resultJson);

                    _authToken = result.token;
                    _currentUserId = result.userId;

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

                    return (true, result.role);
                }

                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Ошибка: {error}", "Вход не удался",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return (false, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return (false, null);
            }
        }


        // 1. Модель данных, которая в точности соответствует вашему [HttpGet("users")]
        public class AdminUserResponse
        {
            public int Id { get; set; }
            public string Login { get; set; } = string.Empty;
            public bool IsBanned { get; set; }
            public DateTime? BanExpiry { get; set; }

            // Удобные свойства для красивого вывода в DataGrid (вычисляются на клиенте)
            public string Status => IsBanned ? "Забанен" : "Активен";
            public string BanExpiryDisplay => BanExpiry.HasValue ? BanExpiry.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm") : "—";
        }

        // 2. Методы внутри класса ApiService:
        public async Task<List<AdminUserResponse>> GetAdminUsersAsync()
        {
            // Запрос к вашему [HttpGet("users")]
            var response = await _httpClient.GetAsync("api/admin/users");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdminUserResponse>>(json);
            }
            return new List<AdminUserResponse>();
        }



        public async Task<bool> CreateDatabaseBackupAsync()
        {
            // Отправляем POST-запрос на ваш эндпоинт api/admin/backup
            var response = await _httpClient.PostAsync("api/admin/backup", null);
            return response.IsSuccessStatusCode;
        }
        // СОХРАНИТЬ ПРОГРЕСС
        public async Task<bool> SaveProgressAsync(string difficulty, int score, int timeSeconds)
        {
            try
            {
                var data = new { difficulty = difficulty, score = score, timeSeconds = timeSeconds };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Progress/save", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> BanUserAsync(int userId, int days = 7)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/Admin/ban/{userId}?days={days}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnbanUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/Admin/unban/{userId}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserInfo>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Admin/users");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<UserInfo>>(json);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public class UserInfo
        {
            public int Id { get; set; }
            public string Login { get; set; }
            public bool IsBanned { get; set; }
            public DateTime? BanExpiry { get; set; }
        }
        // ПОЛУЧИТЬ СТАТИСТИКУ
        public async Task<object> GetStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Progress/stats");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<object>(json);
                }
                return null;
            }
            catch
            {
                return null;
            }

        }

        // ПОЛУЧИТЬ УВЕДОМЛЕНИЯ
        public async Task<object> GetNotificationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Notification/my");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<object>(json);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);
        public int CurrentUserId => _currentUserId;

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Progress/leaderboard");

                if (!response.IsSuccessStatusCode)
                    return new List<LeaderboardEntry>();

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json)
                       ?? new List<LeaderboardEntry>();
            }
            catch
            {
                return new List<LeaderboardEntry>();
            }
        }
    }

    // Классы для ответов от API
    public class LoginResponse
    {
        public string token { get; set; }
        public int userId { get; set; }
        public string role { get; set; }
    }



} 