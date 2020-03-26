using System;
using System.Collections.Generic;
using System.Linq;

namespace tasklist
{
    public class RecurringTasksLoader : FileLoaderSpecified<RecurringTasks>, ILoader<RecurringTasks>
    {
        RepeatSchemeToStringConverter repeatSchemeToStringConverter = new RepeatSchemeToStringConverter();

        TimeOfDayToStringConverter timeOfDayToStringConverter = new TimeOfDayToStringConverter();

        protected override string Path => fileName;
        string fileName;

        public RecurringTasksLoader(string fileName)
        {
            this.fileName = fileName;
        }

        protected override string[] Write(RecurringTasks recurring)
        {
            List<string> lines = new List<string>();
            List<RecurringTaskTemplate> l = new List<RecurringTaskTemplate>(recurring.repeatedTasks);
            while(l.Count > 0) {
                RecurringTaskTemplate current = l[0];
                RepeatScheme scheme = current.RepeatScheme;
                lines.Add(RepeatSchemeLabel(scheme));
                var matches = l.Where(i => i.RepeatScheme.Equals(scheme)).ToArray();
                foreach(var template in matches){
                    string line = TextDefs.Indent(1) + WriteTodoTaskRepeatedTemplate(template);
                    lines.Add(line);
                    l.Remove(template);
                }
            }
            return lines.ToArray();
        }
        string RepeatSchemeLabel(RepeatScheme scheme)
        {
            return $"{repeatSchemeToStringConverter.Convert(scheme)}:";
        }
        string WriteTodoTaskRepeatedTemplate(RecurringTaskTemplate template)
        {
            RepeatSchemeToStringConverter repeatSchemeToStringConverter = new RepeatSchemeToStringConverter();
            string line = template.Name + " ";
            if (template.TimeOfDay != null)
                line += $"- {timeOfDayToStringConverter.Convert(template.TimeOfDay)} ";
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
                string trimmedLine = line.TrimWhitespace();

                if (trimmedLine.EndsWith(TextDefs.repeatSchemeMarker))
                {
                    string schemeText = trimmedLine.Substring(0,trimmedLine.Length-TextDefs.repeatSchemeMarker.Length);
                    currentScheme = (RepeatScheme)repeatSchemeToStringConverter.ConvertBack(schemeText);
                }
                //check if this line marks a note about the previous task
                else if (line.StartsWith(TextDefs.Indent(2)) || String.IsNullOrWhiteSpace(line))
                {
                    if (recurring.repeatedTasks.Count == 0) continue;
                    var prevTask = recurring.repeatedTasks[recurring.repeatedTasks.Count - 1] as RecurringTaskTemplate;
                    if (prevTask == null) continue;
                    prevTask.Notes += (String.IsNullOrWhiteSpace(prevTask.Notes) ? "" : Environment.NewLine) + trimmedLine;
                }
                //otherwise read task information from this line
                else
                {
                    line = trimmedLine;
                    RecurringTaskTemplate template = ParseRecurringTaskTemplate(line, currentScheme);
                    recurring.repeatedTasks.Add(template);
                }
            }
            return recurring;
        }
        RecurringTaskTemplate ParseRecurringTaskTemplate(string input, RepeatScheme scheme)
        {
            RecurringTaskTemplate template = new RecurringTaskTemplate()
            {
                RepeatScheme = scheme
            };
            //split up this line's task information, delimited by dashes
            string[] dataSplit = input.Split('-');
            int currentSplit = 0;

            //set name
            template.Name = dataSplit[currentSplit].TrimWhitespace();
            currentSplit++;

            //set scheduled time
            //if this task is scheduled, check the next split for the scheduled time
            if (dataSplit.Length > currentSplit)
            {
                string scheduledTimeText = dataSplit[currentSplit].TrimWhitespace();
                var time = timeOfDayToStringConverter.ConvertBack(scheduledTimeText);
                if (time != null)
                {
                    template.TimeOfDay = (TimeSpan)time;
                    currentSplit++;
                } else throw new ArgumentException("Failed to parse time of day while loading recurring tasks.");
            }
            return template;
        }
    }
}