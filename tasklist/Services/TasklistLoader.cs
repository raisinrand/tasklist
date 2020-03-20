using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;


//TODO references to settings are temporarily hardcoded
namespace tasklist
{
    public class TasklistLoader : ITasklistLoader
    {
        const string fileName = "do.txt";

        // ISettingsManager settingsManager;

        public TasklistLoader()
        {
            // settingsManager = ServiceLocator.Current.GetInstance<ISettingsManager>();
        }

        public Tasklist Load()
        {
            string path = GetLocalPath();
            string[] lines = File.ReadAllLines(path);
            Tasklist tasklist = ParseTasklist(lines);
            return tasklist;
        }
        public bool Save(Tasklist tasklist)
        {
            string[] lines = ConvertTasklistToText(tasklist);
            return Save(lines);
        }
        public bool Save(string[] lines)
        {
            string path = GetLocalPath();
            string backupPath = GetLocalBackupPath();
            try
            {
                //TODO: instead of doing this, write to a copy initially and if all succeeds then copy the copy.
                // regardless of success or failure, delete copy.
                string[] oldLines = File.ReadAllLines(path);
                try
                {
                    ParseTasklist(oldLines);
                    File.WriteAllLines(backupPath, oldLines);
                }
                catch
                {}

                File.WriteAllLines(path, lines);
                return true;
            }
            catch { throw; }
        }
        const string indent = "    ";
        const string unscheduledMarker = "UNSCHEDULED";
        const string repeatedInstancesMarker = indent + "REPEATED";
        const string repeatedTemplatesMarker = "REPEATED";

        //TODO: this should write directly to stream to support big tasklist
        public static string[] ConvertTasklistToText(Tasklist tasklist)
        {
            List<string> lines = new List<string>();
            //start with -1 because nothing matches it, 0 matches to unscheduled days
            DateTime currentDate = DateTime.MinValue;
            tasklist.tasksByDay.Sort(Comparer<DayTasks>.Create((a,b) =>
            {
                if(a.day.HasValue && b.day.HasValue)
                    return a.day.Value.CompareTo(b.day.Value);
                //sort unscheduled day (null day value) to bottom
                int comp = 0;
                if (a.day.HasValue)
                    comp -= 1;
                if (b.day.HasValue)
                    comp += 1;
                return comp;
            }));
            foreach(DayTasks dayTasks in tasklist.tasksByDay)
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
                    lines.Add(unscheduledMarker);
                }
                foreach (TodoTask task in dayTasks.tasks.Where(i => i is TodoTask))
                {
                    string line = $"{indent}{WriteTodoTask((TodoTask)task)}";
                    lines.Add(line);
                }
                // var repeatedInstances = dayTasks.tasks.Where(i => i is TodoTaskRepeated && ((TodoTaskRepeated)i).IsModified);
                // if (repeatedInstances.Count() > 0)
                // {
                //     lines.Add(repeatedInstancesMarker);
                // }
                // foreach (TodoTaskRepeated task in repeatedInstances)
                // {
                //     string line = $"{indent}{indent}{WriteTodoTaskRepeatedInstance(task)}";
                //     lines.Add(line);
                // }

            }
            // //repeated templates
            // if(tasklist.repeatedTasks != null && tasklist.repeatedTasks.Count > 0)
            // {
            //     lines.Add(repeatedTemplatesMarker);
            //     foreach (TodoTaskRepeatedTemplate template in tasklist.repeatedTasks)
            //     {
            //         string line = $"{indent}{WriteTodoTaskRepeatedTemplate(template)}";
            //         lines.Add(line);
            //     }
            // }
            return lines.ToArray();
        }
        public static string WriteTodoTask(TodoTask task)
        {
            string line = "";
            line += task.Name + " ";
            if (task.IsScheduledTime)
                line += $"- {task.ScheduledTime.ToString("h:mm tt")} ";

            string tags = "";
            if (task.Difficulty != 1)
                tags += $"d{task.Difficulty} ";
            if (task.Duration.TotalHours != 0)
                tags += $"{task.Duration.TotalHours} ";
            if (tags != "" || task.IsDue)
                line += $"- {tags}";

            if (task.IsDue)
                line += $"- {task.DueDate.ToString("MM/dd")} ";
            //return line with last space chopped off
            return line.Substring(0, line.Length - 1);
        }
        // public static string WriteTodoTaskRepeatedInstance(TodoTaskRepeated task)
        // {
        //     string line = "";
        //     if (task.Completed)
        //         line += "-";
        //     line += task.Name + " ";
        //     if (task.LocalScheduledTimeOfDay != null)
        //         line += $"- {(new DateTime() + (TimeSpan)task.LocalScheduledTimeOfDay).ToString("h:mm tt")} ";
        //     //return line with last space chopped off
        //     return line.Substring(0, line.Length - 1);
        // }
        // public static string WriteTodoTaskRepeatedTemplate(TodoTaskRepeatedTemplate template)
        // {
        //     RepeatSchemeToStringConverter repeatSchemeToStringConverter = new RepeatSchemeToStringConverter();
        //     string line = template.name + " ";
        //     if (template.timeOfDay != null)
        //         line += $"- {(new DateTime() + (TimeSpan)template.timeOfDay).ToString("h:mm tt")} ";

        //     string tags = "";
        //     if (template.priority != 1)
        //         tags += $"p{template.priority} ";
        //     if (template.difficulty != 1)
        //         tags += $"d{template.difficulty} ";
        //     if (template.duration.TotalHours != 0)
        //         tags += $"{template.duration.TotalHours} ";
        //     line += $"- {tags}";

        //     line += $"- {repeatSchemeToStringConverter.Convert(template.repeatScheme)} ";
        //     //return line with last space chopped off
        //     return line.Substring(0, line.Length - 1);
        // }
        enum ReadState
        {
            Task,
            TaskRepeated,
            RepeatedTemplate
        }
        public static Tasklist ParseTasklist(string[] lines)
        {
            Tasklist tasklist = new Tasklist();
            // tasklist.repeatedTasks = new List<TodoTaskRepeatedTemplate>();
            tasklist.tasksByDay = new List<DayTasks>();
            DayTasks currentDayTasks = null;

            //TEMP completed tasks before this time are skipped
            DateTime skipTime = DateTime.Today;

            List<string> repeatedInstanceLines = new List<string>();
            List<DayTasks> repeatedInstanceDays = new List<DayTasks>();

            ReadState readState = ReadState.Task;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                //skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                string trimmedLine = line.Trim(' ', '\t');
                //skip lines that start with ---
                if (trimmedLine.Length >= 3 && trimmedLine.Substring(0, 3) == "---")
                    continue;

                //check if this line marks a new day
                DateTime dateMark;
                if (DateTime.TryParse(trimmedLine, out dateMark))
                {
                    //if so set this to the current day
                    currentDayTasks = new DayTasks();
                    currentDayTasks.day = dateMark;
                    currentDayTasks.tasks = new List<ITodoTask>();
                    tasklist.tasksByDay.Add(currentDayTasks);
                    readState = ReadState.Task;
                }
                //check if this line marks unscheduled tasks
                else if (line == unscheduledMarker)
                {
                    currentDayTasks = new DayTasks();
                    currentDayTasks.day = null;
                    currentDayTasks.tasks = new List<ITodoTask>();
                    tasklist.tasksByDay.Add(currentDayTasks);
                    readState = ReadState.Task;
                }
                else if (line == repeatedInstancesMarker)
                {
                    readState = ReadState.TaskRepeated;
                }
                else if (line == repeatedTemplatesMarker)
                {
                    readState = ReadState.RepeatedTemplate;
                }
                //otherwise read task information from this line
                else
                {
                    line = trimmedLine;
                    switch (readState)
                    {
                        case ReadState.Task:
                            TodoTask task = ParseTodoTask(line, currentDayTasks.day);
                            currentDayTasks.tasks.Add(task);
                            break;
                        // case ReadState.TaskRepeated:
                        //     //save repeated instance line for later, parse these last so that all templates are loaded already
                        //     repeatedInstanceLines.Add(line);
                        //     repeatedInstanceDays.Add(currentDayTasks);
                        //     break;
                        // case ReadState.RepeatedTemplate:
                        //     TodoTaskRepeatedTemplate template = ParseTodoTaskRepeatedTemplate(line);
                        //     tasklist.repeatedTasks.Add(template);
                        //     break;
                    }
                }
            }
            // for(int i = 0; i < repeatedInstanceLines.Count; i++)
            // {
            //     DayTasks dayTasks = repeatedInstanceDays[i];
            //     TodoTaskRepeated taskRepeated = ParseTodoTaskRepeatedInstance(repeatedInstanceLines[i], tasklist.repeatedTasks, (DateTime)dayTasks.day);
            //     if (taskRepeated.ScheduledTime >= skipTime)
            //         dayTasks.tasks.Add(taskRepeated);
            // }
            //filter out empty days
            for (int i = 0; i < tasklist.tasksByDay.Count; i++)
            {
                if (tasklist.tasksByDay[i].tasks.Count == 0)
                {
                    tasklist.tasksByDay.RemoveAt(i);
                    i--;
                }
            }
            return tasklist;
        }
        static TodoTask ParseTodoTask(string input, DateTime? day)
        {
            TodoTask task = new TodoTask
            {
                ScheduledTime = DateTime.MaxValue,
                IsScheduledDay = day > DateTime.MinValue,
                IsScheduledTime = false,
                Difficulty = 1,
                IsDue = false
            };
            if (task.IsScheduledDay)
                task.ScheduledTime = (DateTime)day;

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
            if (task.IsScheduledDay && dataSplit.Length > currentSplit)
            {
                string scheduledTimeText = dataSplit[currentSplit].Trim(' ');
                DateTime time;
                if (DateTime.TryParseExact(scheduledTimeText, "h:mm tt", null, DateTimeStyles.None, out time))
                {
                    task.IsScheduledTime = true;
                    task.ScheduledTime += time.TimeOfDay;
                    currentSplit++;
                }
            }

            //set properties
            //if there's another split, check it for properties
            if (dataSplit.Length > currentSplit)
            {
                string[] propertySplit = dataSplit[currentSplit].Split(' ');
                foreach (var property in propertySplit)
                {
                    //skip empty
                    if (property.Length == 0)
                        continue;
                    //decide which property is being defined based on prefix
                    switch (property[0])
                    {
                        case 'd':
                            task.Difficulty = int.Parse(property.Substring(1));
                            break;
                        default:
                            TimeSpan duration = TimeUtils.RoundedHrsToTimeSpan(float.Parse(property));
                            task.Duration = duration;
                            break;
                    }
                }
                currentSplit++;
            }

            //set duedate
            //if there's another split, check it for duedate
            if (dataSplit.Length > currentSplit)
            {
                string dueDateText = dataSplit[currentSplit].Trim(' ');
                DateTime dueDate = DateTime.Today;
                if (DateTime.TryParseExact(dueDateText, "MM/dd", null, DateTimeStyles.None, out dueDate))
                {
                    task.IsDue = true;
                    task.DueDate = dueDate;
                    currentSplit++;
                }
            }

            return task;
        }
        // static TodoTaskRepeated ParseTodoTaskRepeatedInstance(string input, List<TodoTaskRepeatedTemplate> templates, DateTime day)
        // {
        //     if (input.Length == 0)
        //         throw new ArgumentException();

        //     bool completed = input[0] == '-';
        //     if(completed)
        //     {
        //         input = input.Substring(1, input.Length - 1);
        //     }

        //     //split up this line's task information, delimited by dashes
        //     string[] dataSplit = input.Split('-');
        //     int currentSplit = 0;

        //     //get template
        //     string templateName = dataSplit[currentSplit].Trim(' ');
        //     TodoTaskRepeatedTemplate template = templates.Find(i => i.name == templateName);
        //     currentSplit++;

        //     //create repeatedtask
        //     TodoTaskRepeated task = new TodoTaskRepeated(template, day)
        //     {
        //         Completed = completed
        //     };

        //     //set scheduled time
        //     //if this task is scheduled, check the next split for the scheduled time
        //     if (dataSplit.Length > currentSplit)
        //     {
        //         string scheduledTimeText = dataSplit[currentSplit].Trim(' ');
        //         DateTime time;
        //         if (DateTime.TryParseExact(scheduledTimeText, "h:mm tt", null, DateTimeStyles.None, out time))
        //         {
        //             task.LocalScheduledTimeOfDay = time.TimeOfDay;
        //             currentSplit++;
        //         }
        //     }
        //     return task;
        // }
        // static TodoTaskRepeatedTemplate ParseTodoTaskRepeatedTemplate(string input)
        // {
        //     RepeatSchemeToStringConverter repeatSchemeToStringConverter = new RepeatSchemeToStringConverter();
        //     TodoTaskRepeatedTemplate template = new TodoTaskRepeatedTemplate()
        //     {
        //         difficulty = 1,
        //         priority = 1,
        //         repeatScheme = new RepeatNever()
        //     };
        //     //split up this line's task information, delimited by dashes
        //     string[] dataSplit = input.Split('-');
        //     int currentSplit = 0;

        //     //set name
        //     template.name = dataSplit[currentSplit].Trim(' ');
        //     currentSplit++;

        //     //set scheduled time
        //     //if this task is scheduled, check the next split for the scheduled time
        //     if (dataSplit.Length > currentSplit)
        //     {
        //         string scheduledTimeText = dataSplit[currentSplit].Trim(' ');
        //         DateTime time;
        //         if (DateTime.TryParseExact(scheduledTimeText, "h:mm tt", null, DateTimeStyles.None, out time))
        //         {
        //             template.timeOfDay = time.TimeOfDay;
        //             currentSplit++;
        //         }
        //     }

        //     //set properties
        //     //if there's another split, check it for properties
        //     if (dataSplit.Length > currentSplit)
        //     {
        //         string[] propertySplit = dataSplit[currentSplit].Split(' ');
        //         foreach (var property in propertySplit)
        //         {
        //             //skip empty
        //             if (property.Length == 0)
        //                 continue;
        //             //decide which property is being defined based on prefix
        //             switch (property[0])
        //             {
        //                 case 'd':
        //                     template.difficulty = int.Parse(property.Substring(1));
        //                     break;
        //                 case 'p':
        //                     template.priority = int.Parse(property.Substring(1));
        //                     break;
        //                 default:
        //                     TimeSpan duration = TimeUtils.RoundedHrsToTimeSpan(float.Parse(property));
        //                     template.duration = duration;
        //                     break;
        //             }
        //         }
        //         currentSplit++;

        //         //set repeatscheme
        //         //if there's another split, check it for repeatscheme
        //         if (dataSplit.Length > currentSplit)
        //         {
        //             string repeatSchemeText = dataSplit[currentSplit].Trim(' ');
        //             template.repeatScheme = (RepeatScheme)repeatSchemeToStringConverter.ConvertBack(repeatSchemeText);
        //         }
        //     }
        //     return template;
        // }

        public string GetLocalPath()
        {
            // return $"{settingsManager.CurrentSettings.Directory}/{fileName}";
            return $"C:/Users/thebi/Documents/work-local/tasklist-cmd/tasklist/test/{fileName}";
        }
        //TODO: use system.io.path for this kind of stuff
        public string GetLocalBackupPath()
        {
            return GetLocalPath().Insert(GetLocalPath().LastIndexOf('.'), "-backup");
        }
        public DateTime GetFileLastModifiedTime()
        {
            return File.GetLastWriteTime(GetLocalPath());
        }
    }
}