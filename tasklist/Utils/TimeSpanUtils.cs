using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public static class TimeSpanUtils
    {
        public static TimeSpan WrapTimeOfDay(this TimeSpan s) {
            s = TimeSpan.FromHours(s.TotalHours % 24);
            return s<TimeSpan.Zero ? s + TimeSpan.FromHours(24) : s;
        }
    }
}
