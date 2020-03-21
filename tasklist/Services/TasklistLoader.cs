using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace tasklist
{
    public class TasklistLoader : FileLoaderSpecified<Tasklist>, ILoader<Tasklist>
    {
        protected override string FileName => "do.txt";

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
                    lines.Add(TasklistTextDefs.unscheduledMarker);
                }
                foreach (TodoTask task in dayTasks.tasks.Where(i => i is TodoTask))
                {
                    string line = TasklistTextDefs.Indent(1) + WriteTodoTask((TodoTask)task);
                    lines.Add(line);
                }
            }
            return lines.ToArray();
        }
        string WriteTodoTask(TodoTask task)
        {
            string line = "";
            line += task.Name + " ";
            if (task.ScheduledTime.HasValue)
                line += $"- {task.ScheduledTime.Value.ToString("h:mm tt")} ";

            if (task.Notes != null)
            {
                line += TasklistTextDefs.FormattedTaskNote(task.Notes);
            }
            //return line with last space chopped
            return line.Substring(0, line.Length - 1);
        }
        protected override Tasklist Parse(string[] lines)
        {
            Tasklist tasklist = new Tasklist();
            DayTasks currentDayTasks = null;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmedLine = line.Trim(' ', '\t');

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
                else if (line == TasklistTextDefs.unscheduledMarker)
                {
                    currentDayTasks = new DayTasks();
                    currentDayTasks.day = null;
                    currentDayTasks.tasks = new List<ITodoTask>();
                    tasklist.tasksByDay.Add(currentDayTasks);
                }
                //check if this line marks a note about the previous task
                else if (line.StartsWith(TasklistTextDefs.Indent(2)) || String.IsNullOrWhiteSpace(line))
                {
                    if (currentDayTasks.tasks.Count == 0) continue;
                    var prevTask = currentDayTasks.tasks[currentDayTasks.tasks.Count - 1] as TodoTask;
                    if (prevTask == null) continue;
                    prevTask.Notes += (String.IsNullOrWhiteSpace(prevTask.Notes) ? "" : Environment.NewLine) + trimmedLine;

                }
                //otherwise read task information from this line
                else
                {
                    line = trimmedLine;
                    TodoTask task = ParseTodoTask(line, currentDayTasks.day);
                    currentDayTasks.tasks.Add(task);
                }
            }

            FilterEmptyDays(tasklist);
            return tasklist;
        }
        TodoTask ParseTodoTask(string input, DateTime? day)
        {
            bool isScheduledDay = day > DateTime.MinValue;
            TodoTask task = new TodoTask
            {
                ScheduledTime = DateTime.MaxValue
            };

            if (input.Length == 0)
                return task;

            //split up this line's task information, delimited by dashes
            string[] dataSplit = input.Split('-');
            int currentSplit = 0;

            //set name
            task.Name = dataSplit[currentSplit].Trim(' ');
            currentSplit++;

            //set scheduled time
            //if this task is scheduled, check the next split for the scheduled time
            if (isScheduledDay && dataSplit.Length > currentSplit)
            {
                string scheduledTimeText = dataSplit[currentSplit].Trim(' ');
                DateTime time;
                if (DateTime.TryParseExact(scheduledTimeText, "h:mm tt", null, DateTimeStyles.None, out time))
                {
                    task.ScheduledTime = day + time.TimeOfDay;
                    currentSplit++;
                }
            }

            //set properties
            //if there's another split, check it for properties
            // if (dataSplit.Length > currentSplit)
            // {
            //     string[] propertySplit = dataSplit[currentSplit].Split(' ');
            //     foreach (var property in propertySplit)
            //     {
            //         //skip empty
            //         if (property.Length == 0)
            //             continue;
            //         //decide which property is being defined based on prefix
            //         switch (property[0])
            //         {
            //             case 'd':
            //                 task.Difficulty = int.Parse(property.Substring(1));
            //                 break;
            //             default:
            //                 TimeSpan duration = TimeUtils.RoundedHrsToTimeSpan(float.Parse(property));
            //                 task.Duration = duration;
            //                 break;
            //         }
            //     }
            //     currentSplit++;
            // }

            //set duedate
            //if there's another split, check it for duedate
            // if (dataSplit.Length > currentSplit)
            // {
            //     string dueDateText = dataSplit[currentSplit].Trim(' ');
            //     DateTime dueDate = DateTime.Today;
            //     if (DateTime.TryParseExact(dueDateText, "MM/dd", null, DateTimeStyles.None, out dueDate))
            //     {
            //         task.IsDue = true;
            //         task.DueDate = dueDate;
            //         currentSplit++;
            //     }
            // }

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