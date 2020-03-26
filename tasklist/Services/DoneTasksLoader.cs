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

            lines.AddRange(tasks.doneTaskLabels.SelectMany(i => WriteDoneTask(i)));
            if(tasks.rescheduledTaskLabels.Count > 0) {
                lines.Add("");
                lines.Add(TextDefs.rescheduledMarker + ":");
                lines.AddRange(TextDefs.Indent(1,tasks.rescheduledTaskLabels.SelectMany(i => WriteDoneTask(i))));
            }
            if(tasks.skippedTaskLabels.Count > 0) {
                lines.Add("");
                lines.Add(TextDefs.skippedMarker + ":");
                lines.AddRange(TextDefs.Indent(1,tasks.skippedTaskLabels.SelectMany(i => WriteDoneTask(i))));
            }
            return lines.ToArray();
        }
        string[] WriteDoneTask(DoneTask task)
        {
            string res = task.label;
            if (task.notes != null)
            {
                res += TextDefs.FormattedTaskNote(task.notes,1);
            }
            return res.SplitLines();
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
            foreach(string line in lines) {
                if(string.IsNullOrWhiteSpace(line)) continue;
                string trimmedLine = line.TrimWhitespace();
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
                    List<DoneTask> prevList;
                    switch(mode) {
                        case ParseMode.Rescheduled:
                            prevList = res.rescheduledTaskLabels;
                        break;
                        case ParseMode.Skipped:
                            prevList = res.skippedTaskLabels;
                        break;
                        default:
                            prevList = res.doneTaskLabels;
                        break;
                    }
                    if (prevList.Count == 0) continue;
                    var prevTask = prevList[prevList.Count - 1];
                    if (prevTask == null) continue;
                    prevTask.notes += (String.IsNullOrWhiteSpace(prevTask.notes) ? "" : Environment.NewLine) + trimmedLine;
                    continue;
                }

                DoneTask task = new DoneTask() { label=trimmedLine };

                switch(mode)
                {
                    case ParseMode.Done:
                        res.doneTaskLabels.Add(task);
                    break;
                    case ParseMode.Rescheduled:
                        res.rescheduledTaskLabels.Add(task);
                    break;
                    case ParseMode.Skipped:
                        res.skippedTaskLabels.Add(task);
                    break;
                }
            }
            return res;
        }
    }
}