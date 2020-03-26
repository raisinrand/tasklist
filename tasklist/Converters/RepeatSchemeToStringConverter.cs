using System;
using System.Collections.Generic;
using System.Globalization;

namespace tasklist
{
    // TODO: clean all of this
    public class RepeatSchemeToStringConverter : IConverter
    {
        Dictionary<DayOfWeek, string> dayIds = new Dictionary<DayOfWeek, string>();
        DateToStringConverter dateToStringConverter = new DateToStringConverter();
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
        const string dayOfMonthMarker = "of month";
        const string fromDateMarker = "from";
        public object Convert(object value, object parameter = null, CultureInfo culture = null)
        {
            if (value is RepeatPeriodic repeatPeriodic)
            {
                string s = repeatPeriodic.dayInterval.ToString() + "D";
                int offset = ((int)(repeatPeriodic.startDay - DateTime.MinValue).TotalDays) % repeatPeriodic.dayInterval;
                if (offset > 0)
                    s += offset + "O";
                return s;
            }
            else if (value is RepeatWeekly repeatWeekly)
            {
                string r = "";
                foreach (var assignmentPair in repeatWeekly.DayAssignments)
                {
                    if (assignmentPair.Value)
                        r += dayIds[assignmentPair.Key];
                }
                return r;
            }
            else if (value is RepeatDayOfMonth repeatDayOfMonth)
            {
                return $"{repeatDayOfMonth.dayOfMonth} {dayOfMonthMarker}";
            }
            // general placeholder
            else if (value is RepeatScheme)
            {
                return "";
            }
            return null;
        }
        public object ConvertBack(object value, object parameter = null, CultureInfo culture = null)
        {
            string input = value as string;
            if (input == null) return null;
            input = input.TrimWhitespace();
            if (input.EndsWith(dayOfMonthMarker))
            {
                int dayOfMonth = int.Parse(input.Substring(0, input.Length - dayOfMonthMarker.Length).TrimWhitespace());
                return new RepeatDayOfMonth() { dayOfMonth = dayOfMonth };
            }
            // TODO: fix this, dirty
            if (input.Length > 0 && char.IsNumber(input[0]))
            {
                int dayIntervalStartIndex = 0;
                int dayIntervalEndIndex = input.IndexOf('D');
                int dayOffsetStartIndex = dayIntervalEndIndex + 1;
                int dayOffsetEndIndex = input.IndexOf('O');
                int fromDayIndex = input.IndexOf(fromDateMarker);
                int interval;
                if (dayIntervalEndIndex >= 0 && int.TryParse(input.Substring(dayIntervalStartIndex, dayIntervalEndIndex - dayIntervalStartIndex), out interval))
                {
                    DateTime startDay = DateTime.MinValue;
                    int dayOffset;
                    if (dayOffsetEndIndex > dayIntervalEndIndex)
                    {
                        int.TryParse(input.Substring(dayOffsetStartIndex, dayOffsetEndIndex - dayOffsetStartIndex), out dayOffset);
                        startDay = DateTime.MinValue + TimeSpan.FromDays(dayOffset);
                    }
                    else if (fromDayIndex >= 0)
                    {
                        string fromDateText = input.Substring(fromDayIndex+fromDateMarker.Length);
                        startDay = (DateTime)dateToStringConverter.ConvertBack(fromDateText);
                    }
                    return new RepeatPeriodic() { dayInterval = interval, startDay = startDay};
                }
            }
            else
            {
                bool foundAny = false;
                Dictionary<DayOfWeek, bool> dayAssignments = new Dictionary<DayOfWeek, bool>();
                foreach (DayOfWeek day in dayIds.Keys)
                {
                    bool found = input.Contains(dayIds[day]);
                    foundAny |= found;
                    dayAssignments.Add(day, found);
                }
                if (foundAny)
                {
                    return new RepeatWeekly(dayAssignments);
                }
            }
            return new RepeatNever();
        }

    }
}
