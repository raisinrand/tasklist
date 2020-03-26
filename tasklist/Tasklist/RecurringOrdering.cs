using System;

namespace tasklist
{
    public class RecurringOrdering
    {
        public enum Mode {
            Before,
            After
        }
        public Mode mode;
        public string targetPrefix;
    }
}
