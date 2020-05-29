using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using ArgConvert.Converters;

namespace tasklist
{
    public class DoneTasksLoader : FileLoaderSpecified<DoneTasks>, ILoader<DoneTasks>
    {
        protected override string Path => fileName;
        string fileName;

        protected override bool IgnoreMissing { get { return true; } }
        
        TimeOfDayToStringConverter timeOfDayToStringConverter = new TimeOfDayToStringConverter();

        public DoneTasksLoader(string fileName)
        {
            this.fileName = fileName;
        }

        protected override string[] Write(DoneTasks tasks)
        {
            var lines = new List<string>();

            lines.AddRange(tasks.Done.SelectMany(i => WriteDoneTask(i)));
            if(tasks.Rescheduled.Count() > 0) {
                lines.Add("");
                lines.Add(TextDefs.rescheduledMarker + ":");
                lines.AddRange(TextDefs.Indent(1,tasks.Rescheduled.SelectMany(i => WriteDoneTask(i))));
            }
            if(tasks.Skipped.Count() > 0) {
                lines.Add("");
                lines.Add(TextDefs.skippedMarker + ":");
                lines.AddRange(TextDefs.Indent(1,tasks.Skipped.SelectMany(i => WriteDoneTask(i))));
            }
            return lines.ToArray();
        }
        string[] WriteDoneTask(DoneTask task)
        {
            string line = "";
            line += task.Name;

            if (task.StartTime.HasValue)
            {
                line += $" {timeOfDayToStringConverter.Convert(task.StartTime.Value)}";
            }
            if (task.CompleteTime.HasValue) {
                line += $" - {timeOfDayToStringConverter.Convert(task.CompleteTime)}";
            }

            if (task.Notes != null)
            {
                line += TextDefs.FormattedTaskNote(task.Notes, 1);
            }
            return line.SplitLines();
        }
        enum ParseMode {
            Done,
            Rescheduled,
            Skipped
        }
        protected override DoneTasks Parse(string[] lines)
        {
            DoneTasks res = new DoneTasks();
            ParseMode mode = ParseMode.Done;
            int indentLevel = 0;
            DoneTask lastAddedTask = null;
            foreach(string line in lines) {
                if(string.IsNullOrWhiteSpace(line)) continue;
                string trimmedLine = line.Trim();
                if(trimmedLine.StartsWith(TextDefs.rescheduledMarker)) {
                    mode = ParseMode.Rescheduled;
                    indentLevel = 1;
                    continue;
                }
                if(trimmedLine.StartsWith(TextDefs.skippedMarker)) {
                    mode = ParseMode.Skipped;
                    indentLevel = 1;
                    continue;
                }
                if(line.StartsWith(TextDefs.Indent(indentLevel+1)))
                {
                    if(lastAddedTask == null) continue;
                    lastAddedTask.Notes += (String.IsNullOrWhiteSpace(lastAddedTask.Notes) ? "" : Environment.NewLine) + trimmedLine;
                    continue;
                }
                DoneTask task = ParseDoneTask(trimmedLine);
                if(mode == ParseMode.Done) Debug.Assert(task.CompleteTime.HasValue, "Unexpected done task without completion time.");
                DoneType doneType = ModeToDoneType(mode);
                res.AddTask(task,doneType);
                lastAddedTask = task;
            }
            return res;
        }
        DoneTask ParseDoneTask(string input) {
            DoneTask task = new DoneTask();

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

            if (dataSplit.Length > currentSplit)
            {
                string scheduledTimeText = dataSplit[currentSplit].Trim();
                var time = timeOfDayToStringConverter.ConvertBack(scheduledTimeText);
                if (time != null)
                {
                    task.CompleteTime = (TimeSpan)time;
                    currentSplit++;
                }
                else throw new ArgumentException("Failed to parse time of day while loading tasklist.");
            }
            return task;
        }
        DoneType ModeToDoneType(ParseMode mode) {
            switch(mode) {
                case ParseMode.Done: return DoneType.Done;
                case ParseMode.Rescheduled: return DoneType.Rescheduled;
                case ParseMode.Skipped: return DoneType.Skipped;
            }
            throw new Exception("impossible");
        }
    }
}