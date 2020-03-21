using System;
using System.Collections.Generic;
using System.Globalization;

namespace tasklist
{
    public class RecurringTasksLoader : FileLoaderSpecified<RecurringTasks>, ILoader<RecurringTasks>
    {
        protected override string FileName => "do-recurring.txt";

        RepeatSchemeToStringConverter repeatSchemeToStringConverter = new RepeatSchemeToStringConverter();

        protected override string[] Write(RecurringTasks recurring)
        {
            List<string> lines = new List<string>();
            RepeatScheme currentScheme = new RepeatNever();
            //repeated templates
            foreach (RecurringTaskTemplate template in recurring.repeatedTasks)
            {
                if(template.repeatScheme != currentScheme) {
                    lines.Add(RepeatSchemeLabel(template.repeatScheme));
                    currentScheme = template.repeatScheme;
                }
                string line = TasklistTextDefs.Indent(1)+WriteTodoTaskRepeatedTemplate(template);
                lines.Add(line);
            }
            return lines.ToArray();
        }
        string RepeatSchemeLabel(RepeatScheme scheme) {
            return $"{repeatSchemeToStringConverter.Convert(scheme)}:";
        }
        string WriteTodoTaskRepeatedTemplate(RecurringTaskTemplate template)
        {
            RepeatSchemeToStringConverter repeatSchemeToStringConverter = new RepeatSchemeToStringConverter();
            string line = template.name + " ";
            if (template.timeOfDay != null)
                line += $"- {(new DateTime() + (TimeSpan)template.timeOfDay).ToString("h:mm tt")} ";
            //return line with last space chopped off
            return line.Substring(0, line.Length - 1);
        }
        protected override RecurringTasks Parse(string[] lines)
        {
            var recurring = new RecurringTasks();
            RepeatScheme currentScheme = new RepeatNever();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmedLine = line.Trim(' ', '\t');

                if (trimmedLine.EndsWith(TasklistTextDefs.repeatSchemeMarker))
                {
                    currentScheme = (RepeatScheme)repeatSchemeToStringConverter.ConvertBack(trimmedLine);
                }
                //check if this line marks a note about the previous task
                else if (line.StartsWith(TasklistTextDefs.Indent(2)) || String.IsNullOrWhiteSpace(line))
                {
                    if (recurring.repeatedTasks.Count == 0) continue;
                    var prevTask = recurring.repeatedTasks[recurring.repeatedTasks.Count - 1] as RecurringTaskTemplate;
                    if (prevTask == null) continue;
                    prevTask.notes += (String.IsNullOrWhiteSpace(prevTask.notes) ? "" : Environment.NewLine) + trimmedLine;
                }
                //otherwise read task information from this line
                else {
                    line = trimmedLine;
                    RecurringTaskTemplate template = ParseRecurringTaskTemplate(line,currentScheme);
                    recurring.repeatedTasks.Add(template);
                }
            }
            return recurring;
        }
        RecurringTaskTemplate ParseRecurringTaskTemplate(string input, RepeatScheme scheme)
        {
            RecurringTaskTemplate template = new RecurringTaskTemplate()
            {
                repeatScheme = scheme
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
            return template;
        }
    }
}