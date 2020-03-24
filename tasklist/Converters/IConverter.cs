using System.Globalization;

namespace tasklist
{
    public interface IConverter
    {
        object Convert(object value, object parameter = null, CultureInfo culture = null);
        object ConvertBack(object value, object parameter = null, CultureInfo culture = null);
    }
}