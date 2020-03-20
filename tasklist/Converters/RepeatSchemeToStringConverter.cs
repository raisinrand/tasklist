using System;
using System.Collections.Generic;
using System.Globalization;

namespace tasklist
{
    public class RepeatSchemeToStringConverter
    {
        Dictionary<DayOfWeek,string> dayIds = new Dictionary<DayOfWeek, string>();
        public RepeatSchemeToStringConverter()
        {
            dayIds.Add(DayOfWeek.Monday, "M");
            dayIds.Add(DayOfWeek.Tuesday, "T");
            dayIds.Add(DayOfWeek.Wednesday, "W");
            dayIds.Add(DayOfWeek.Thursday, "R");
            dayIds.Add(DayOfWeek.Friday, "F");
            dayIds.Add(DayOfWeek.Saturday, "Sa");
            dayIds.Add(DayOfWeek.Sunday, "Su");
        }
        public object Convert(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
        {
            if(value is RepeatPeriodic repeatPeriodic)
            {
                string s = repeatPeriodic.dayInterval.ToString() + "D";
                int offset = ((int)(repeatPeriodic.startDay - DateTime.MinValue).TotalDays) % repeatPeriodic.dayInterval;
                if(offset > 0)
                    s += offset + "O";
                return s; 
            }
            else if(value is RepeatWeekly repeatWeekly)
            {
                string r = "";
                foreach (var assignmentPair in repeatWeekly.DayAssignments)
                {
                    if(assignmentPair.Value)
                        r+=dayIds[assignmentPair.Key];
                }
                return r;
            }
            return "";
        }
        public object ConvertBack(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
        {
            string input = (string)value;
            input = input.Trim(' ');
            if(input.Length > 0 && char.IsNumber(input[0]))
            {
                int dayIntervalStartIndex = 0;
                int dayIntervalEndIndex = input.IndexOf('D');
                int dayOffsetStartIndex = dayIntervalEndIndex+1;
                int dayOffsetEndIndex = input.IndexOf('O');
                int interval;
                if (dayIntervalEndIndex >= 0 && int.TryParse(input.Substring(dayIntervalStartIndex, dayIntervalEndIndex - dayIntervalStartIndex), out interval))
                {
                    //int dayOffset = ((int)(DateTime.Today - DateTime.MinValue).TotalDays) % interval;
                    int dayOffset = 0;
                    if (dayOffsetEndIndex > dayIntervalEndIndex)
                    {
                        int.TryParse(input.Substring(dayOffsetStartIndex, dayOffsetEndIndex - dayOffsetStartIndex), out dayOffset);
                    }
                    RepeatPeriodic repeatPeriodic = new RepeatPeriodic();
                    repeatPeriodic.startDay = DateTime.MinValue + TimeSpan.FromDays(dayOffset);
                    repeatPeriodic.dayInterval = interval;
                    return repeatPeriodic;
                }
            }
            else
            {
                bool foundAny = false;
                Dictionary<DayOfWeek, bool> dayAssignments = new Dictionary<DayOfWeek, bool>();
                foreach(DayOfWeek day in dayIds.Keys)
                {
                    bool found = input.Contains(dayIds[day]);
                    foundAny |= found;
                    dayAssignments.Add(day, found);
                }
                if(foundAny)
                {
                    return new RepeatWeekly(dayAssignments);
                }
            }
            return new RepeatNever();
        }
    }
}
