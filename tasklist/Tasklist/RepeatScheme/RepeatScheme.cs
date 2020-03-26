using System;

namespace tasklist
{
    public abstract class RepeatScheme
    {
        abstract public bool RepeatsOn(DateTime day);
    }
}
