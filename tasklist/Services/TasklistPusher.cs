using System;

namespace tasklist
{
    // Pushes scheduled times for tasks.
    public class TasklistPusher
    {
        public TasklistPusher() 
        {}

        public void Push(DayTasks dayTasks, TimeSpan amount, TimeSpan? start, TimeSpan? end) {
            foreach(var task in dayTasks.tasks) {
                if(!task.ScheduledTime.HasValue) continue;
                if(start.HasValue && task.ScheduledTime.Value < start.Value) continue;
                if(end.HasValue && task.ScheduledTime.Value > end.Value) continue;
                task.ScheduledTime += amount;
                task.ScheduledTime = task.ScheduledTime.Value.WrapTimeOfDay();
            }
        }
    }
}