using System;

namespace tasklist
{
    // Pushes scheduled times for tasks.
    public class TasklistPusher
    {
        public TasklistPusher()
        { }

        public void Push(DayTasks dayTasks, TimeSpan amount, TimeSpan? start, TimeSpan? end)
        {
            foreach (var task in dayTasks.tasks)
            {
                task.StartTime = PushTime(task.StartTime, amount, start, end);
                task.ScheduledTime = PushTime(task.ScheduledTime, amount, start, end);
            }
        }
        public TimeSpan? PushTime(TimeSpan? time, TimeSpan amount, TimeSpan? start, TimeSpan? end)
        {
            if (!time.HasValue
                || (start.HasValue && time.Value < start.Value)
                || (end.HasValue && time.Value > end.Value)
            ) return time;
            time += amount;
            time = time.Value.WrapTimeOfDay();
            return time;
        }
    }
}