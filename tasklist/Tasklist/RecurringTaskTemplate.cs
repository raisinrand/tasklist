using System;

namespace tasklist
{
    /// <summary>
    /// defines the template for a repeated todotask
    /// </summary>
    public class RecurringTaskTemplate
    {
        public string Name { get; set; }
        public TimeSpan? TimeOfDay { get; set; }
        public string Notes { get; set; }
        public RepeatScheme RepeatScheme { get; set; }
        // can be null
        public RecurringOrdering Ordering { get; set; }
    }
}
