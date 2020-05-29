using System;
using System.Diagnostics;
using ArgConvert.Converters;

namespace tasklist
{
    public class TaskCompleter
    {
        TimeOfDayToStringConverter todToStringConverter = new TimeOfDayToStringConverter();
        DateToStringConverter dateTimeToStringConverter = new DateToStringConverter();

        public TaskCompleter()
        {}

        public DoneTask ConvertToDone(ITodoTask task, TimeSpan? startTime = null, TimeSpan? completeTime = null) {
            return new DoneTask() { Name = task.Name, StartTime = startTime, CompleteTime = completeTime  };
        }
        public void Complete(DayTasks dayTasks, int index, DoneTasks done, TimeSpan? startTime, TimeSpan completeTime) {
            ITodoTask task = dayTasks.tasks[index];
            done.AddTask(ConvertToDone(task,startTime,completeTime),DoneType.Done);
            dayTasks.tasks.RemoveAt(index);
        }
        public void Reschedule(Tasklist l, DayTasks dayTasks, int index, DateTime reassignDate, DoneTasks done) {
            Debug.Assert(reassignDate.Date.Equals(reassignDate));
            ITodoTask task = dayTasks.tasks[index];
            done.AddTask(ConvertToDone(task),DoneType.Rescheduled);
            dayTasks.tasks.RemoveAt(index);
            DayTasks t = l.tasksByDay.Find(i => i.day == reassignDate);
            if(t == null) {
                t = new DayTasks() { day = reassignDate };
                l.tasksByDay.Add(t);
            }
            task.StartTime = null;
            task.ScheduledTime = null;
            t.tasks.Add(task);
        }
        public void Skip(DayTasks dayTasks, int index, DoneTasks done) {
            ITodoTask task = dayTasks.tasks[index];
            done.AddTask(ConvertToDone(task),DoneType.Skipped);
            dayTasks.tasks.RemoveAt(index);
        }
    }
}