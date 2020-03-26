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
        const string orMarker = "or";
        public object Convert(object value, object parameter = null, CultureInfo culture = null)
        {
            // TODO: this could be a switch
            if (value is RepeatOr repeatOr) {
                return $"{Convert(repeatOr.left,parameter,culture)} {orMarker} {Convert(repeatOr.right,parameter,culture)}";
            }
            else if (value is RepeatPeriodic repeatPeriodic)
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
        // allows whitespace
        public object ConvertBack(object value, object parameter = null, CultureInfo culture = null)
        {
            //TODO: could also probably switch pattern match this and divide into funcs
            string input = value as string;
            if (input == null) return null;
            input = input.TrimWhitespace();
            if(input.Contains(orMarker)) {
                int andIndex = input.IndexOf(orMarker);
                RepeatScheme left = (RepeatScheme)ConvertBack(input.Substring(0,andIndex),parameter,culture);
                RepeatScheme right = (RepeatScheme)ConvertBack(input.Substring(andIndex+orMarker.Length),parameter,culture);
                return new RepeatOr() { left = left, right = right };
            }
            else if (input.EndsWith(dayOfMonthMarker))
            {
                int dayOfMonth = int.Parse(input.Substring(0, input.Length - dayOfMonthMarker.Length).TrimWhitespace());
                return new RepeatDayOfMonth() { dayOfMonth = dayOfMonth };
            }
            // TODO: fix this, dirty
            else if (input.Length > 0 && char.IsNumber(input[0]))
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
