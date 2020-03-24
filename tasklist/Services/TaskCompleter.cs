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

        public void Complete(DayTasks dayTasks, int index, DoneTasks done) {
            ITodoTask task = dayTasks.tasks[index];
            DateTime completeTime = DateTime.Now;
            done.doneTaskLabels.Add(ConvertToLabel(task,completeTime.TimeOfDay));
            dayTasks.tasks.RemoveAt(index);
        }
        public string ConvertToLabel(ITodoTask task, TimeSpan completeTime) {
            string res = task.Name;
            res += " - " + todToStringConverter.Convert(completeTime);
            return res;
        }
        
        public void Reschedule(Tasklist l, DayTasks dayTasks, int index, DateTime reassignDate, DoneTasks done) {
            Debug.Assert(reassignDate.Date.Equals(reassignDate));
            ITodoTask task = dayTasks.tasks[index];
            done.rescheduledTaskLabels.Add(ConvertToLabelSkip(task));
            dayTasks.tasks.RemoveAt(index);
            DayTasks t = l.tasksByDay.Find(i => i.day == reassignDate);
            if(t == null) {
                t = new DayTasks() { day = reassignDate };
                l.tasksByDay.Add(t);
            }
            task.ScheduledTime = null;
            t.tasks.Add(task);
        }
        public string ConvertToLabelReschedule(ITodoTask task,DateTime date) {
            string res = task.Name;
            res += " - " + dateTimeToStringConverter.Convert(date);
            return res;
        }
        
        public void Skip(DayTasks dayTasks, int index, DoneTasks done) {
            ITodoTask task = dayTasks.tasks[index];
            done.skippedTaskLabels.Add(ConvertToLabelSkip(task));
            dayTasks.tasks.RemoveAt(index);
        }
        public string ConvertToLabelSkip(ITodoTask task) {
            return task.Name;
        }
    }
}