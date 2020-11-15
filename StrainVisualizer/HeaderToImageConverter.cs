using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace StrainVisualizer
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    class HeaderToImageConverter : IValueConverter
    {

        public static HeaderToImageConverter Instance = new HeaderToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = (string)value;

            if (path == null)
            {
                return null;
            }
            var name = MainWindow.GetFileFolderName(path);
            var icon = "file.ico";

            if (string.IsNullOrEmpty(name))
            {
                icon = "drive.ico";
            }
            else if (new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory))
            {
                icon = "folder.ico";
            }


            return new BitmapImage(new Uri($"pack://application:,,,/images/{icon}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
