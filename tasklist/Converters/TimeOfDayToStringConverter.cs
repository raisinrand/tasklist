using System;
using System.Diagnostics;
using System.Globalization;

namespace tasklist
{
    public class TimeOfDayToStringConverter : IConverter
    {
        const string format = "h:mm tt";
        public object Convert(object value, object parameter = null, CultureInfo culture = null)
        {
            if(!(value as TimeSpan?).HasValue) return null;
            TimeSpan time = (TimeSpan)value;
            Debug.Assert(time >= TimeSpan.Zero);
            return (new DateTime() + time).ToString(format);
        }
        public object ConvertBack(object value, object parameter = null, CultureInfo culture = null)
        {
            string input = value as string;
            if(input == null) return null;
            DateTime time;
            if (DateTime.TryParseExact(input, format, null, DateTimeStyles.None, out time))
            {
                return time.TimeOfDay;
            }
            return null;
        }
        // returns [min,max) possible lengths of times of day accepted by this converter
        public int[] PossibleLengthRange() {
            return new int[] { "h:mm tt".Length, "hh:mm tt".Length+1 };
        }
    }
}
