using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace tasklist
{
    public class DoneTasksLoader : FileLoaderSpecified<DoneTasks>, ILoader<DoneTasks>
    {
        protected override string Path => fileName;
        string fileName;

        protected override bool IgnoreMissing { get { return true; } }

        public DoneTasksLoader(string fileName)
        {
            this.fileName = fileName;
        }

        protected override string[] Write(DoneTasks tasks)
        {
            var lines = new List<string>();

            lines.AddRange(tasks.doneTaskLabels);
            if(tasks.rescheduledTaskLabels.Count > 0) {
                lines.Add("");
                lines.Add(TextDefs.rescheduledMarker + ":");
                lines.AddRange(TextDefs.Indent(1,tasks.rescheduledTaskLabels));
            }
            if(tasks.skippedTaskLabels.Count > 0) {
                lines.Add("");
                lines.Add(TextDefs.skippedMarker + ":");
                lines.AddRange(TextDefs.Indent(1,tasks.skippedTaskLabels));
            }
            return lines.ToArray();
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
            foreach(string line in lines) {
                if(string.IsNullOrWhiteSpace(line)) continue;
                string trimmedLine = line.TrimWhitespace();
                if(trimmedLine.StartsWith(TextDefs.rescheduledMarker)) {
                    mode = ParseMode.Rescheduled;
                    continue;
                }
                if(trimmedLine.StartsWith(TextDefs.skippedMarker)) {
                    mode = ParseMode.Skipped;
                    continue;
                }

                switch(mode)
                {
                    case ParseMode.Done:
                        res.doneTaskLabels.Add(trimmedLine);
                    break;
                    case ParseMode.Rescheduled:
                        res.rescheduledTaskLabels.Add(trimmedLine);
                    break;
                    case ParseMode.Skipped:
                        res.skippedTaskLabels.Add(trimmedLine);
                    break;
                }
            }
            return res;
        }
    }
}