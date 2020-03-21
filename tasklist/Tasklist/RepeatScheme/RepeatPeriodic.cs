using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public class RepeatPeriodic : RepeatScheme
    {
        public DateTime startDay;
        public int dayInterval;
        public override bool RepeatsOn(DateTime day)
        {
            int daysFromStart = (int)Math.Floor((day - startDay).TotalDays);
            return daysFromStart % dayInterval == 0;
        }
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                RepeatPeriodic p = (RepeatPeriodic) obj;
                return p.startDay == startDay && p.dayInterval == dayInterval;
            }   
        }
        public override int GetHashCode()
        {
            return startDay.GetHashCode() + dayInterval.GetHashCode();
        }
    }
}
