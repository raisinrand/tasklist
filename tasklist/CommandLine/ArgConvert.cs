using System;

namespace tasklist.CommandLine
{
    // conversion for arguments
    // returns null if string is null but throws if there is a malformed string.
    public class ArgConvert
    {
        public static TimeSpan? ParseTimeOfDay(string s) {
            var timeOfDayToStringConverter = new TimeOfDayToStringConverter();
            try {
                return (TimeSpan?)ConvertWith(s, timeOfDayToStringConverter);
            }
            catch(ArgumentException){
                throw new ArgumentException("Failed to parse time of day.");
            }
        }
        public static TimeSpan? ParseTimeSpan(string s) {
            var timeSpanToStringConverter = new TimeSpanToStringConverter();
            try {
                return (TimeSpan?)ConvertWith(s, timeSpanToStringConverter);
            }
            catch(ArgumentException){
                throw new ArgumentException("Failed to parse time of day.");
            }
        }
        public static DateTime? ParseDateTime(string s) {
            var dateTimeToStringConverter = new DateToStringConverter();
            try {
                return (DateTime?)ConvertWith(s, dateTimeToStringConverter);
            }
            catch(ArgumentException){
                throw new ArgumentException("Failed to parse time of day.");
            }
        }

        public static object ConvertWith(string s, IConverter converter) {
            if(s == null) return null;
            var res = converter.ConvertBack(s);
            if(res == null) {
                throw new ArgumentException("Failed to parse.");
            }
            return res;
        } 

        public static bool TryParseTaskIndexFromPrefix(DayTasks day, string prefix, out int res) {
            res = -1;
            for(int i = 0; i < day.tasks.Count; i++) {
                ITodoTask task = day.tasks[i];
                if(task.Name.StartsWith(prefix,true,null)) {
                    res = i;
                    return true;
                }
            }
            return false;
        }
    }
}