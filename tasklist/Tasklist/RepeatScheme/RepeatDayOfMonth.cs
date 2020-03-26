using System;

namespace tasklist
{
    public class RepeatDayOfMonth : RepeatScheme
    {
        public int dayOfMonth;
        public override bool RepeatsOn(DateTime day)
        {
            int dayActual = dayOfMonth > 0 ? dayOfMonth : DateTime.DaysInMonth(day.Year,day.Month) + dayOfMonth;
            return day.Day == dayActual;
        }
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                RepeatDayOfMonth p = (RepeatDayOfMonth) obj;
                return p.dayOfMonth == dayOfMonth;
            }   
        }
        public override int GetHashCode()
        {
            return dayOfMonth.GetHashCode();
        }
    }
}
