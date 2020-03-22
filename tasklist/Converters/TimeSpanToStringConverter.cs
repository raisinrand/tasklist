using System;
using System.Globalization;

namespace tasklist
{
    public class TimeSpanToStringConverter
    {
        const string hoursLabel = "h";
        const string minutesLabel = "m";
        const string secondsLabel = "s";
        public object Convert(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
        {
            throw new NotImplementedException();
        }
        public object ConvertBack(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
        {
            string input = value as string;
            if(input == null) return null;
            bool negative = input.StartsWith('-');
            if(negative) input = input.Substring(1);
            string[] labels = {hoursLabel,minutesLabel,secondsLabel};
            double hours = ExtractTimeUnit(ref input, hoursLabel, labels) ?? 0;
            double minutes = ExtractTimeUnit(ref input, minutesLabel, labels) ?? 0;
            double seconds = ExtractTimeUnit(ref input, secondsLabel, labels) ?? 0;
            if(!string.IsNullOrWhiteSpace(input)) {
                return null;
            }
            TimeSpan total = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
            return negative ? -total : total;
        }
        double? ExtractTimeUnit(ref string input, string label, string[] labels) {
            int labelIndex = input.IndexOf(label);
            if(labelIndex > 0) { 
                int startIndex = input.LastIndexOfAny(labels,labelIndex-1)+1;
                string val = input.Substring(startIndex,labelIndex-startIndex);
                input = input.Remove(startIndex,1+labelIndex-startIndex);
                double res;
                if(double.TryParse(val, out res)) return res;
                else return null;
            }
            return null;
        }
    }
}
