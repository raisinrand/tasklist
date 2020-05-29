using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ArgConvert.Converters;

namespace tasklist
{
    public class TasklistLoader : FileLoaderSpecified<Tasklist>, ILoader<Tasklist>
    {
        protected override string Path => fileName;
        string fileName;

        TimeOfDayToStringConverter timeOfDayToStringConverter = new TimeOfDayToStringConverter();

        public TasklistLoader(string fileName)
        {
            this.fileName = fileName;
        }

        protected override string[] Write(Tasklist tasklist)
        {
            List<string> lines = new List<string>();
            //start with -1 because nothing matches it, 0 matches to unscheduled days
            DateTime currentDate = DateTime.MinValue;
            tasklist.tasksByDay.Sort(Comparer<DayTasks>.Create((a, b) =>
            {
                if (a.day.HasValue && b.day.HasValue)
                    return a.day.Value.CompareTo(b.day.Value);
                //sort unscheduled day (null day value) to bottom
                int comp = 0;
                if (a.day.HasValue)
                    comp -= 1;
                if (b.day.HasValue)
                    comp += 1;
                return comp;
            }));
            foreach (DayTasks dayTasks in tasklist.tasksByDay)
            {
                //skip empty days
                if (dayTasks.tasks.Count == 0)
                    continue;

                if (dayTasks.day != null)
                {
                    lines.Add(((DateTime)dayTasks.day).ToShortDateString());
                }
                else
                {
                    currentDate = DateTime.MinValue;
                    lines.Add(TextDefs.unscheduledMarker);
                }
                foreach (TodoTask task in dayTasks.tasks.Where(i => i is TodoTask))
                {
                    string line = TextDefs.Indent(1) + WriteTodoTask((TodoTask)task);
                    lines.Add(line);
                }
            }
            return lines.ToArray();
        }
        string WriteTodoTask(TodoTask task)
        {
            string line = "";
            line += task.Name;

            if (task.StartTime.HasValue)
            {
                line += $" {timeOfDayToStringConverter.Convert(task.StartTime.Value)}";
            }

            if (task.ScheduledTime.HasValue)
                line += $" - {timeOfDayToStringConverter.Convert(task.ScheduledTime.Value)}";

            if (task.Notes != null)
            {
                line += TextDefs.FormattedTaskNote(task.Notes, 2);
            }
            return line;
        }
        protected override Tasklist Parse(string[] lines)
        {
            Tasklist tasklist = new Tasklist();
            DayTasks currentDayTasks = null;
            TodoTask lastAddedTask = null;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmedLine = line.Trim();

                //check if this line marks a new day
                DateTime dateMark;
                if (DateTime.TryParse(trimmedLine, out dateMark))
                {
                    //if so set this to the current day
                    currentDayTasks = new DayTasks();
                    currentDayTasks.day = dateMark;
                    currentDayTasks.tasks = new List<ITodoTask>();
                    tasklist.tasksByDay.Add(currentDayTasks);
                }
                //check if this line marks unscheduled tasks
                else if (line == TextDefs.unscheduledMarker)
                {
                    currentDayTasks = new DayTasks();
                    currentDayTasks.day = null;
                    currentDayTasks.tasks = new List<ITodoTask>();
                    tasklist.tasksByDay.Add(currentDayTasks);
                }
                //check if this line marks a note about the previous task
                else if (line.StartsWith(TextDefs.Indent(2)) || String.IsNullOrWhiteSpace(line))
                {
                    if (lastAddedTask == null) continue;
                    lastAddedTask.Notes += (String.IsNullOrWhiteSpace(lastAddedTask.Notes) ? "" : Environment.NewLine) + trimmedLine;

                }
                //otherwise read task information from this line
                else
                {
                    line = trimmedLine;
                    TodoTask task = ParseTodoTask(line, currentDayTasks.day);
                    currentDayTasks.tasks.Add(task);
                    lastAddedTask = task;
                }
            }

            // FilterEmptyDays(tasklist);
            tasklist.tasksByDay.Sort(
                (x, y) =>
                {
                    if (x.day.HasValue && y.day.HasValue) return x.day.Value.CompareTo(y.day.Value);
                    else return (x.day.HasValue ? -1 : 1) - (y.day.HasValue ? -1 : 1);
                }
            );
            if (HasMultipleUnscheduledDays(tasklist))
            {
                throw new ArgumentException("Tasklist file has multiple unscheduled days.");
            }
            return tasklist;
        }
        bool HasMultipleUnscheduledDays(Tasklist tasklist)
        {
            if (tasklist.tasksByDay.Count < 2) return false;
            else
            {
                return !tasklist.tasksByDay[tasklist.tasksByDay.Count - 2].day.HasValue;
            }
        }
        TodoTask ParseTodoTask(string input, DateTime? day)
        {
            bool isScheduledDay = day > DateTime.MinValue;
            TodoTask task = new TodoTask();

            Debug.Assert(input.Length >= 0);

            string[] dataSplit = input.Split(TextDefs.separator);
            int currentSplit = 0;
            task.Name = dataSplit[currentSplit].Trim();
            int startTimeIndex;
            task.StartTime = TextDefs.GetStartTimeFromTaskName(task.Name, timeOfDayToStringConverter, out startTimeIndex);
            if (startTimeIndex >= 0)
            {
                task.Name = task.Name.Remove(startTimeIndex).TrimEnd();
            }

            currentSplit++;

            if (isScheduledDay && dataSplit.Length > currentSplit)
            {
                string scheduledTimeText = dataSplit[currentSplit].Trim();
                var time = timeOfDayToStringConverter.ConvertBack(scheduledTimeText);
                if (time != null)
                {
                    task.ScheduledTime = (TimeSpan)time;
                    currentSplit++;
                }
                else throw new ArgumentException("Failed to parse time of day while loading tasklist.");
            }

            return task;
        }
        void FilterEmptyDays(Tasklist tasklist)
        {
            for (int i = 0; i < tasklist.tasksByDay.Count; i++)
            {
                if (tasklist.tasksByDay[i].tasks.Count == 0)
                {
                    tasklist.tasksByDay.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}