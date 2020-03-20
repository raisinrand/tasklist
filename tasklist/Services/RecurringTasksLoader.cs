using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


//TODO references to settings are temporarily hardcoded
namespace tasklist
{
    public class RecurringTasksLoader : FileLoaderSpecified<RecurringTasksScheme>, ILoader<RecurringTasksScheme>
    {
        protected override string FileName => "do-recurring.txt"; 

        protected override string[] Write(RecurringTasksScheme scheme)
        {
            List<string> lines = new List<string>();
            //repeated templates
            foreach (TodoTaskRepeatedTemplate template in scheme.repeatedTasks)
            {
                string line = WriteTodoTaskRepeatedTemplate(template);
                lines.Add(line);
            }
            return lines.ToArray();
        }
        string WriteTodoTaskRepeatedTemplate(TodoTaskRepeatedTemplate template)
        {
            RepeatSchemeToStringConverter repeatSchemeToStringConverter = new RepeatSchemeToStringConverter();
            string line = template.name + " ";
            if (template.timeOfDay != null)
                line += $"- {(new DateTime() + (TimeSpan)template.timeOfDay).ToString("h:mm tt")} ";

            string tags = "";
            if (template.priority != 1)
                tags += $"p{template.priority} ";
            if (template.difficulty != 1)
                tags += $"d{template.difficulty} ";
            if (template.duration.TotalHours != 0)
                tags += $"{template.duration.TotalHours} ";
            line += $"- {tags}";

            line += $"- {repeatSchemeToStringConverter.Convert(template.repeatScheme)} ";
            //return line with last space chopped off
            return line.Substring(0, line.Length - 1);
        }
        protected override RecurringTasksScheme Parse(string[] lines)
        {
            var scheme = new RecurringTasksScheme();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                //skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                    
                string trimmedLine = line.Trim(' ', '\t');
                //otherwise read task information from this line
                line = trimmedLine;
                TodoTaskRepeatedTemplate template = ParseTodoTaskRepeatedTemplate(line);
                scheme.repeatedTasks.Add(template);
            }
            return scheme;
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
        TodoTaskRepeatedTemplate ParseTodoTaskRepeatedTemplate(string input)
        {
            RepeatSchemeToStringConverter repeatSchemeToStringConverter = new RepeatSchemeToStringConverter();
            TodoTaskRepeatedTemplate template = new TodoTaskRepeatedTemplate()
            {
                difficulty = 1,
                priority = 1,
                repeatScheme = new RepeatNever()
            };
            //split up this line's task information, delimited by dashes
            string[] dataSplit = input.Split('-');
            int currentSplit = 0;

            //set name
            template.name = dataSplit[currentSplit].Trim(' ');
            currentSplit++;

            //set scheduled time
            //if this task is scheduled, check the next split for the scheduled time
            if (dataSplit.Length > currentSplit)
            {
                string scheduledTimeText = dataSplit[currentSplit].Trim(' ');
                DateTime time;
                if (DateTime.TryParseExact(scheduledTimeText, "h:mm tt", null, DateTimeStyles.None, out time))
                {
                    template.timeOfDay = time.TimeOfDay;
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
                            template.difficulty = int.Parse(property.Substring(1));
                            break;
                        case 'p':
                            template.priority = int.Parse(property.Substring(1));
                            break;
                        default:
                            TimeSpan duration = TimeUtils.RoundedHrsToTimeSpan(float.Parse(property));
                            template.duration = duration;
                            break;
                    }
                }
                currentSplit++;

                //set repeatscheme
                //if there's another split, check it for repeatscheme
                if (dataSplit.Length > currentSplit)
                {
                    string repeatSchemeText = dataSplit[currentSplit].Trim(' ');
                    template.repeatScheme = (RepeatScheme)repeatSchemeToStringConverter.ConvertBack(repeatSchemeText);
                }
            }
            return template;
        }
    }
}