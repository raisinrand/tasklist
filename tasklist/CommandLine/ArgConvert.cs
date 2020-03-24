using System;

namespace tasklist.CommandLine
{
    // conversion for arguments
    // returns null if string is null but throws if there is a malformed string.
    public class ArgConvert
    {
        // generalize this fn if i add more parse fns
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
                throw new ArgumentException("Failed to parse time span.");
            }
            return (TimeSpan)res;
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