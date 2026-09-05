using System;
using System.Windows;
using System.Windows.Controls;

namespace SudokuEcosystem.Views
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        // 1. При загрузке страницы считываем сохраненные настройки компьютера
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SoundCheckBox.IsChecked = Properties.Settings.Default.IsSoundEnabled;
            VolumeSlider.Value = Properties.Settings.Default.VolumeLevel;
            AutoSaveCheckBox.IsChecked = Properties.Settings.Default.HighlightErrors;
            VolumeValue.Text = $"{(int)VolumeSlider.Value}%";

            // Выставляем нужную тему в ComboBox
            if (Properties.Settings.Default.CurrentTheme == "Классическая темная")
                ThemeComboBox.SelectedIndex = 1;
            else
                ThemeComboBox.SelectedIndex = 0;
        }

        // Обновление текста процентов при движении ползунка
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeValue != null)
            {
                VolumeValue.Text = $"{(int)e.NewValue}%";
            }
        }

        // 2. Кнопка «Сохранить» — записывает данные намертво в память устройства
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsSoundEnabled = SoundCheckBox.IsChecked ?? true;
            Properties.Settings.Default.VolumeLevel = VolumeSlider.Value;
            Properties.Settings.Default.HighlightErrors = AutoSaveCheckBox.IsChecked ?? true;

            // Сохраняем имя выбранной темы текста
            Properties.Settings.Default.CurrentTheme = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Розовая";

            Properties.Settings.Default.Save();

            // Сразу применяем тему ко всему приложению!
            ApplyGlobalTheme();

            MessageBox.Show("Настройки успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // При загрузке страницы выставляем сохраненный ComboBox
       

        // 3. Кнопка сброса настроек
        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите вернуть стандартные настройки?", "Сброс",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();

                // Перезагружаем страницу, чтобы обновить галочки
                Page_Loaded(this, null);
            }
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
        private void ApplyGlobalTheme()
        {
            // Ищем окно, внутри которого сейчас открыта эта страница
            Window currentWindow = Window.GetWindow(this);

            // Если страница еще загружается и окно не найдено, берем главное окно приложения
            if (currentWindow == null)
            {
                currentWindow = Application.Current.MainWindow;
            }

            if (currentWindow == null) return;

            if (Properties.Settings.Default.CurrentTheme == "Классическая темная")
            {
                // Перекрашиваем в темный цвет
                currentWindow.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#212121"));
            }
            else
            {
                // Возвращаем ваш нежно-розовый цвет
                currentWindow.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFF0F5"));
            }
        }
    }
}