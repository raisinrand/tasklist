using System;
using System.Diagnostics;

namespace tasklist
{
    public class TaskCompleter
    {
        TimeOfDayToStringConverter todToStringConverter = new TimeOfDayToStringConverter();
        DateToStringConverter dateTimeToStringConverter = new DateToStringConverter();

        public TaskCompleter()
        {}

        public void Complete(DayTasks dayTasks, int index, DoneTasks done, TimeSpan? startTime) {
            ITodoTask task = dayTasks.tasks[index];
            DateTime completeTime = DateTime.Now;
            done.doneTaskLabels.Add(ConvertToDone(task,startTime ?? task.StartTime,completeTime.TimeOfDay));
            dayTasks.tasks.RemoveAt(index);
        }
        public DoneTask ConvertToDone(ITodoTask task, TimeSpan? startTime, TimeSpan completeTime) {
            string res = task.Name;
            if(startTime.HasValue) {
                res += $" {todToStringConverter.Convert(startTime.Value)}";
            }
            res += " - " + todToStringConverter.Convert(completeTime);
            return new DoneTask() { label = res };
        }
        
        public void Reschedule(Tasklist l, DayTasks dayTasks, int index, DateTime reassignDate, DoneTasks done) {
            Debug.Assert(reassignDate.Date.Equals(reassignDate));
            ITodoTask task = dayTasks.tasks[index];
            done.rescheduledTaskLabels.Add(ConvertToDoneSkip(task));
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
        public DoneTask ConvertToDoneReschedule(ITodoTask task,DateTime date) {
            string res = task.Name;
            res += " - " + dateTimeToStringConverter.Convert(date);
            return new DoneTask() { label = res };
        }
        
        public void Skip(DayTasks dayTasks, int index, DoneTasks done) {
            ITodoTask task = dayTasks.tasks[index];
            done.skippedTaskLabels.Add(ConvertToDoneSkip(task));
            dayTasks.tasks.RemoveAt(index);
        }
        public DoneTask ConvertToDoneSkip(ITodoTask task) {
            return new DoneTask() { label = task.Name };
        }
    }
}