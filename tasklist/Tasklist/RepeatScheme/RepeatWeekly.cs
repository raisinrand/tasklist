using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace tasklist
{
    public class RepeatWeekly : RepeatScheme
    {
        public Dictionary<DayOfWeek,bool> DayAssignments { get; set; }
        public RepeatWeekly(Dictionary<DayOfWeek, bool> dayAssignments)
        {
            Debug.Assert(dayAssignments.Count == 7);
            DayAssignments = dayAssignments;
        }
        public override bool RepeatsOn(DateTime day)
        {
            return DayAssignments[day.DayOfWeek];
        }
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                RepeatWeekly p = (RepeatWeekly) obj;
                return p.DayAssignments.All( pair => pair.Value == DayAssignments[pair.Key]);
            }   
        }
        public override int GetHashCode()
        {
            int res = 0;
            foreach(KeyValuePair<DayOfWeek,bool> pair in DayAssignments){
                if(pair.Value) res += pair.Key.GetHashCode();
            }
            return res;
        }
    }
}
