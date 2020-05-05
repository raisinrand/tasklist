using System;
using System.Collections.Generic;

namespace tasklist
{
    public enum DoneType {
        Done,
        Rescheduled,
        Skipped
    }
    public class DoneTasks {
        SortedList<TimeSpan,DoneTask> done = new SortedList<TimeSpan,DoneTask>();
        List<DoneTask> rescheduled = new List<DoneTask>();
        List<DoneTask> skipped = new List<DoneTask>();

        public IEnumerable<DoneTask> Done => done.Values;
        public IEnumerable<DoneTask> Rescheduled => rescheduled;
        public IEnumerable<DoneTask> Skipped => skipped;

        public void AddTask(DoneTask t, DoneType type) {
            switch(type) {
                case DoneType.Done:
                    done.Add(t.CompleteTime.Value,t);
                break;
                case DoneType.Rescheduled:
                    rescheduled.Add(t);
                break;
                case DoneType.Skipped:
                    skipped.Add(t);
                break;
            }
        }
    }
    public class DoneTask {
        public string Name {get;set;}
        public TimeSpan? StartTime {get;set;}
        public TimeSpan? CompleteTime {get;set;}
        // not carried over from task notes for now, but can be specified on completion
        public string Notes {get;set;}
        public DoneTask() {}
        public DoneTask(TodoTask task) {
            Name = task.Name;
        }
    }
}