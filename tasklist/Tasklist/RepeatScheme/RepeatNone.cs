using System;

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
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                RepeatNone p = (RepeatNone) obj; 
                return p.Day.Equals(Day);
            }   
        }
        public override int GetHashCode()
        {
            return Day.GetHashCode();
        }
    }
}

