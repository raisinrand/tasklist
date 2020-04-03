using System;
using System.Diagnostics;
using System.Globalization;

namespace tasklist
{
    public class DateToStringConverter : IConverter
    {
        public object Convert(object value, object parameter = null, CultureInfo culture = null)
        {
            if(!(value as DateTime?).HasValue) return null;
            DateTime date = (DateTime)value;
            return date.Date.ToShortDateString();
        }
        const string TomorrowMarker = "tomorrow";
        public object ConvertBack(object value, object parameter = null, CultureInfo culture = null)
        {
            string input = value as string;
            if(input == null) return null;

            if(input.Trim() == TomorrowMarker) {
                return DateTime.Now.Date + TimeSpan.FromDays(1);
            }


            DateTime date;
            if (DateTime.TryParse(input, out date))
            {
                return date.Date;
            }
            return null;
        }
    }
}
