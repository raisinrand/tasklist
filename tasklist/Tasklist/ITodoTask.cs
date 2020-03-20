using System;

namespace tasklist
{
    public interface ITodoTask
    {
        bool Completed { get; set; }
        string Name { get; }
        TimeSpan Duration { get; }
        int Difficulty { get; }
        int Priority { get; }
        bool IsScheduledDay { get; }
        bool IsScheduledTime { get; }
        DateTime ScheduledTime { get; }
    }
}
