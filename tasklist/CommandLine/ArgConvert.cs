using System;

namespace tasklist.CommandLine
{
    // TODO: generalize this?
    public class ArgConvert
    {
        public static TimeSpan? ParseTimeOfDay(string s) {
            if(s == null) return null;
            var timeOfDayToStringConverter = new TimeOfDayToStringConverter();
            var res = timeOfDayToStringConverter.ConvertBack(s);
            if(res == null) {
                throw new ArgumentException("Failed to parse time of day.");
            }
            return (TimeSpan)res;
        }
        public static TimeSpan? ParseTimeSpan(string s) {
            if(s == null) return null;
            var timeSpanToStringConverter = new TimeSpanToStringConverter();
            var res = timeSpanToStringConverter.ConvertBack(s);
            if(res == null) {
                throw new ArgumentException("Failed to parse time of day.");
            }
            return (TimeSpan)res;
        }
    }
}