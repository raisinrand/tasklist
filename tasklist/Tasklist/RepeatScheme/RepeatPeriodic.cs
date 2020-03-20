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
    }
}
