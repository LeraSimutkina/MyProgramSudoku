    using System;
    using System.Globalization;
    using System.Windows.Data;

    namespace SudokuEcosystem.Helpers
    {
        public class HeartVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                int lives = (int)value;
                int heartNumber = int.Parse((string)parameter);

                return heartNumber <= lives ? 1.0 : 0.2;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }