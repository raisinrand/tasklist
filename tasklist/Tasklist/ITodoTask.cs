using System;

namespace tasklist
{
    public interface ITodoTask
    {
        string Name { get; set; }
        TimeSpan? StartTime {get; set;}
        TimeSpan? ScheduledTime { get; set; }
        string Notes { get; set; }
    }
}
