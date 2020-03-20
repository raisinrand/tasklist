using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public abstract class RepeatScheme
    {
        abstract public bool RepeatsOn(DateTime day);
    }
}
