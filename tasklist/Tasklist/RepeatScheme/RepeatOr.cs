using System;

namespace tasklist
{
    public class RepeatOr : RepeatScheme
    {
        public RepeatScheme left;
        public RepeatScheme right;
        public override bool RepeatsOn(DateTime day)
        {
            return left.RepeatsOn(day) || right.RepeatsOn(day);
        }
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                RepeatOr p = (RepeatOr) obj;
                // does naive equality comparison bc we don't care :)
                return ( p.left.Equals(left) && p.right.Equals(right) ) || ( p.right.Equals(left) && p.left.Equals(right) );
            }   
        }
        public override int GetHashCode()
        {
            return left.GetHashCode() + right.GetHashCode();
        }
    }
}
