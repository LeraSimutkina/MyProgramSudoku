using SudokuEcosystem.ViewModels;
using SudokuEcosystem.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SudokuEcosystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var mainMenuViewModel = new MainMenuViewModel();

            // 2. Создаем страницу и передаем ей ViewModel
            var mainMenuPage = new MainMenuPage(mainMenuViewModel);

            // 3. Заставляем Frame (например, с именем MainFrame) отобразить эту страницу
            MainFrame.Navigate(mainMenuPage);
        }

        // Добавляем этот метод, чтобы LoginWindow не ругался:
        public void SetUserRole(string role)
        {
            // 1. Создаем "мозг" для страницы меню
            MainMenuViewModel menuViewModel = new MainMenuViewModel();

            // 2. Создаем саму страницу и передаем ей этот мозг в скобках
            MainMenuPage mainMenu = new MainMenuPage(menuViewModel);

            // 3. Отправляем страницу во Frame
            MainFrame.Navigate(mainMenu);
        }
        public string UserRole { get; private set; }
        

    }
}
