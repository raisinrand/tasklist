using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public class RepeatNever : RepeatScheme
    {
        public override bool RepeatsOn(DateTime day)
        {
            return false;
        }
    }
}
