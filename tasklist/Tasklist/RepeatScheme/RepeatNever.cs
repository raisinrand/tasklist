using System;

namespace tasklist
{
    public class RepeatNever : RepeatScheme
    {
        public override bool RepeatsOn(DateTime day)
        {
            return false;
        }
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else return true;
        }
        public override int GetHashCode()
        {
            return this.GetType().GetHashCode();
        }
    }
}
