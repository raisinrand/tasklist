using System;

namespace tasklist
{
    public interface ITodoTask
    {
        string Name { get; }
        TimeSpan Duration { get; }
        int Difficulty { get; }
        bool IsScheduledDay { get; }
        bool IsScheduledTime { get; }
        DateTime ScheduledTime { get; }
    }
}
