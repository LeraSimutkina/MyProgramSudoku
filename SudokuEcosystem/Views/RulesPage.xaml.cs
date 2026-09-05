using System;
using System.Windows;
using System.Windows.Controls;

namespace SudokuEcosystem.Views
{
    public partial class RulesPage : Page
    {
        public RulesPage()
        {
            InitializeComponent();
        }

        // Возвращаемся на предыдущую страницу (в Главное меню)
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
            else
            {
                // Резервный вариант, если история переходов пуста
                this.NavigationService?.Navigate(new MainMenuPage());
            }
        }
    }
}