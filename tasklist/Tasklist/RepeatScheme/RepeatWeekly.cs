using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO: make this just hold bool for whether we repeat on each day, not just one day of week
namespace tasklist
{
    public class RepeatWeekly : RepeatScheme
    {
        public Dictionary<DayOfWeek,bool> DayAssignments { get; set; }
        public RepeatWeekly(Dictionary<DayOfWeek, bool> dayAssignments)
        {
            DayAssignments = dayAssignments;
        }
        public override bool RepeatsOn(DateTime day)
        {
            return DayAssignments[day.DayOfWeek];
        }
    }
}
