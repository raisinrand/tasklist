using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public static class TimeUtils
    {
        public static TimeSpan RoundedHrsToTimeSpan(float duration)
        {
            int hours = (int)Math.Floor(duration);
            int minutes = (int)Math.Floor((duration - hours) * 60);
            int seconds = (int)Math.Floor((duration - hours - (float)minutes / 60) * 3600);
            return new TimeSpan(hours, minutes, seconds);
        }
        /*public static DateTime DayOfMonthToDateTime(int day)
        {
            //TEMP use current month automatically rn, this is bad
            DateTime today = DateTime.Today;
            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            if (day > daysInMonth)
                day = daysInMonth;
            return new DateTime(today.Year, today.Month, day);
        }*/
        public static string DayOfWeekShortname(DayOfWeek day)
        {
            return day.ToString().Substring(0, 3);
        }
    }
}
