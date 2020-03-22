using System;
using System.Diagnostics;
using System.Globalization;

namespace tasklist
{
    public class TimeOfDayToStringConverter
    {
        public object Convert(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
        {
            if(!(value as TimeSpan?).HasValue) return null;
            TimeSpan time = (TimeSpan)value;
            Debug.Assert(time > TimeSpan.Zero);
            return (new DateTime() + time).ToString("h:mm tt");
        }
        public object ConvertBack(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
        {
            string input = value as string;
            if(input == null) return null;
            DateTime time;
            if (DateTime.TryParseExact(input, "h:mm tt", null, DateTimeStyles.None, out time))
            {
                return time.TimeOfDay;
            }
            return null;
        }
    }
}
