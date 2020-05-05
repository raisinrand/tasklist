using System;
using System.Collections.Generic;
using ArgConvert.Converters;

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
            RepeatScheme scheme = new RepeatNever();
            for(int i = 0; i < recurring.repeatedTasks.Count; i++) {
                var current = recurring.repeatedTasks[i];
                if(scheme != current.RepeatScheme) {
                    scheme = current.RepeatScheme;
                    lines.Add(RepeatSchemeLabel(scheme));
                }
                string[] taskLines = TextDefs.Indent(1, WriteTodoTaskRepeatedTemplate(current));
                lines.AddRange(taskLines);
            }
            return lines.ToArray();
        }
        string RepeatSchemeLabel(RepeatScheme scheme)
        {
            return $"{repeatSchemeToStringConverter.Convert(scheme)}:";
        }
        string[] WriteTodoTaskRepeatedTemplate(RecurringTaskTemplate template)
        {
            RepeatSchemeToStringConverter repeatSchemeToStringConverter = new RepeatSchemeToStringConverter();
            string line = template.Name;
            if (template.TimeOfDay != null)
                line += $" - {timeOfDayToStringConverter.Convert(template.TimeOfDay)}";
            if(template.Ordering != null) {
                string modeText = null;
                switch(template.Ordering.mode) {
                    case RecurringOrdering.Mode.After:
                        modeText = afterOrderingMarker;
                    break;
                    case RecurringOrdering.Mode.Before:
                        modeText = beforeOrderingMarker;
                    break;
                }
                line += $" - {modeText} {template.Ordering.targetPrefix}";
            }
            if (template.Notes != null)
            {
                line += TextDefs.FormattedTaskNote(template.Notes, 1);
            }
            return line.SplitLines();
        }
        protected override RecurringTasks Parse(string[] lines)
        {
            var recurring = new RecurringTasks();
            RepeatScheme currentScheme = new RepeatNever();
            RecurringTaskTemplate lastAddedTemplate = null;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmedLine = line.Trim();

                if (trimmedLine.EndsWith(TextDefs.repeatSchemeMarker))
                {
                    string schemeText = trimmedLine.Substring(0,trimmedLine.Length-TextDefs.repeatSchemeMarker.Length);
                    currentScheme = (RepeatScheme)repeatSchemeToStringConverter.ConvertBack(schemeText);
                }
                //check if this line marks a note about the previous task
                else if (line.StartsWith(TextDefs.Indent(2)) || String.IsNullOrWhiteSpace(line))
                {
                    if (lastAddedTemplate == null) continue;
                    lastAddedTemplate.Notes += (String.IsNullOrWhiteSpace(lastAddedTemplate.Notes) ? "" : Environment.NewLine) + trimmedLine;
                }
                //otherwise read task information from this line
                else
                {
                    line = trimmedLine;
                    RecurringTaskTemplate template = ParseRecurringTaskTemplate(line, currentScheme);
                    recurring.repeatedTasks.Add(template);
                    lastAddedTemplate = template;
                }
            }
            return recurring;
        }
        const string beforeOrderingMarker = "before";
        const string afterOrderingMarker = "after";
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
            template.Name = dataSplit[currentSplit].Trim();
            currentSplit++;

            //set scheduled time
            //if this task is scheduled, check the next split for the scheduled time

            // TODO: if there are more properties, while loop this to allow any order for splits
            if (dataSplit.Length > currentSplit)
            {
                string scheduledTimeText = dataSplit[currentSplit].Trim();
                var time = timeOfDayToStringConverter.ConvertBack(scheduledTimeText);
                if (time != null)
                {
                    template.TimeOfDay = (TimeSpan)time;
                    currentSplit++;
                }
            }
            if(dataSplit.Length > currentSplit) {
                string orderingText = dataSplit[currentSplit].Trim();
                RecurringOrdering.Mode? mode = null;
                if(orderingText.StartsWith(beforeOrderingMarker)) {
                    mode = RecurringOrdering.Mode.Before;
                    orderingText = orderingText.Substring(beforeOrderingMarker.Length);
                } else if(orderingText.StartsWith(afterOrderingMarker)) {
                    mode = RecurringOrdering.Mode.After;
                    orderingText = orderingText.Substring(afterOrderingMarker.Length);
                }
                if(mode.HasValue) {
                    template.Ordering = new RecurringOrdering {mode = mode.Value, targetPrefix = orderingText.Trim()};            
                    currentSplit++;
                }
            }
            if(dataSplit.Length > currentSplit) {
                throw new ArgumentException(
                    $"Failed to fully parse properties for recurring template {template.Name}"
                    +$" while loading recurring templates."
                    );
            }
            return template;
        }
    }
}