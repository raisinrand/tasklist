using System;

namespace tasklist
{
    public class TodoTask : ITodoTask
    {
        public string Name { get; set; }
        public TimeSpan? StartTime {get; set;}
        public TimeSpan? ScheduledTime { get; set; }
        public string Notes { get; set; }
    }
}