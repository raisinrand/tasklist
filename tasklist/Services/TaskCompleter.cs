using System;

namespace tasklist
{
    public class TaskCompleter
    {
        TimeOfDayToStringConverter todToStringConverter = new TimeOfDayToStringConverter();

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
    }
}