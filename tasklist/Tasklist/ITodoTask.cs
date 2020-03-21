using System;

namespace tasklist
{
    public interface ITodoTask
    {
        string Name { get; set; }
        DateTime? ScheduledTime { get; set; }
        string Notes { get; set; }
    }
}
