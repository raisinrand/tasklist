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
        const string periodicIntervalMarker = "D";
        const string periodicOffsetMarker = "O";
        public object Convert(object value, object parameter = null, CultureInfo culture = null)
        {
            switch(value) {
                case RepeatOr repeat:
                    return $"{Convert(repeat.left,parameter,culture)} {orMarker} {Convert(repeat.right,parameter,culture)}";
                case RepeatPeriodic repeat:
                    string s = repeat.dayInterval.ToString() + "D";
                    int offset = ((int)(repeat.startDay - DateTime.MinValue).TotalDays) % repeat.dayInterval;
                    if (offset > 0)
                        s += offset + "O";
                    return s;
                case RepeatWeekly repeat:
                    string r = "";
                    foreach (var assignmentPair in repeat.DayAssignments)
                    {
                        if (assignmentPair.Value)
                            r += dayIds[assignmentPair.Key];
                    }
                    return r;
                case RepeatDayOfMonth repeat:
                    return $"{repeat.dayOfMonth} {dayOfMonthMarker}";
                case RepeatScheme repeat:
                    return "";
                default:
                    return null;
            }
        }
        // allows whitespace
        public object ConvertBack(object value, object parameter = null, CultureInfo culture = null)
        {
            string input = value as string;
            if (input == null) return null;
            input = input.Trim();
            switch(input) {
                case "":
                    return new RepeatNever();
                case var s when s.Contains(orMarker):
                    return ParseAsOr(s);
                case var s when s.EndsWith(dayOfMonthMarker):
                    return ParseAsDayOfMonth(s);
                case var s when char.IsNumber(s[0]):
                    return ParseAsPeriodic(input);
                case var s:
                    return ParseAsWeekly(s);
            }
        }
        RepeatOr ParseAsOr(string s) {
            int andIndex = s.IndexOf(orMarker);
            RepeatScheme left = (RepeatScheme)ConvertBack(s.Substring(0,andIndex));
            RepeatScheme right = (RepeatScheme)ConvertBack(s.Substring(andIndex+orMarker.Length));
            return new RepeatOr() { left = left, right = right };
        }
        RepeatDayOfMonth ParseAsDayOfMonth(string s) {
            int dayOfMonth = int.Parse(s.Substring(0, s.Length - dayOfMonthMarker.Length).Trim());
            return new RepeatDayOfMonth() { dayOfMonth = dayOfMonth };
        }
        // expects no whitespace and not null
        RepeatWeekly ParseAsWeekly(string s) {
            Dictionary<DayOfWeek, bool> dayAssignments = new Dictionary<DayOfWeek, bool>();
            foreach (DayOfWeek day in dayIds.Keys)
            {
                string id = dayIds[day];
                int index = s.IndexOf(id);
                bool found = index >= 0;
                dayAssignments.Add(day, found);
                if(found) {
                    s = s.Remove(index,id.Length);
                }
            }
            if (s == "")
            {
                return new RepeatWeekly(dayAssignments);
            }
            return null;
        }
        RepeatPeriodic ParseAsPeriodic(string s) {
            int dayIntervalStartIndex = 0;
            int dayIntervalEndIndex = s.IndexOf(periodicIntervalMarker);
            int dayOffsetStartIndex = dayIntervalEndIndex + periodicIntervalMarker.Length;
            int dayOffsetEndIndex = s.IndexOf(periodicOffsetMarker);
            int fromDayIndex = s.IndexOf(fromDateMarker);
            int interval;
            if (dayIntervalEndIndex >= 0 && int.TryParse(s.SubstringFrom(dayIntervalStartIndex, dayIntervalEndIndex), out interval))
            {
                DateTime startDay = DateTime.MinValue;
                int dayOffset;
                if (dayOffsetEndIndex > dayIntervalEndIndex)
                {
                    int.TryParse(s.SubstringFrom(dayOffsetStartIndex, dayOffsetEndIndex), out dayOffset);
                    startDay = DateTime.MinValue + TimeSpan.FromDays(dayOffset);
                }
                else if (fromDayIndex >= 0)
                {
                    string fromDateText = s.Substring(fromDayIndex+fromDateMarker.Length);
                    startDay = (DateTime)dateToStringConverter.ConvertBack(fromDateText);
                }
                return new RepeatPeriodic() { dayInterval = interval, startDay = startDay};
            }
            return null;
        }
    }
}
