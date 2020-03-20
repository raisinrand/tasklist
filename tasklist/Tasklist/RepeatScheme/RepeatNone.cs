using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public class RepeatNone : RepeatScheme
    {
        public DateTime Day { get; set; }
        public RepeatNone(DateTime day)
        {
            Day = day;
        }
        public override bool RepeatsOn(DateTime day) { return Day == day; }
        public override string ToString()
        {
            return Day.ToString(@"m/d/yyyy");
        }
    }
}

