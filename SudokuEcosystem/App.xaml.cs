using System.Windows;
using SudokuEcosystem.Services; // Укажите правильный пак, где лежит ApiService

namespace SudokuEcosystem // Тут ваше имя нового проекта
{
    public partial class App : Application
    {
        // Возвращаем глобальный сервис авторизации
        public static ApiService Api { get; } = new ApiService();

        // Возвращаем глобальное свойство роли
        public static string CurrentUserRole { get; set; } = "User";
    }
}