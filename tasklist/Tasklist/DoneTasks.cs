using System;
using System.Collections;
using System.Collections.Generic;

namespace tasklist
{
    public enum DoneType
    {
        Done,
        Rescheduled,
        Skipped
    }
    public class DoneTasks
    {
        List<DoneTask> done = new List<DoneTask>();
        List<DoneTask> rescheduled = new List<DoneTask>();
        List<DoneTask> skipped = new List<DoneTask>();

        public IEnumerable<DoneTask> Done => done;
        public IEnumerable<DoneTask> Rescheduled => rescheduled;
        public IEnumerable<DoneTask> Skipped => skipped;

        public void AddTask(DoneTask t, DoneType type)
        {
            switch (type)
            {
                case DoneType.Done:
                    var index = done.BinarySearch(t,
                        Comparer<DoneTask>.Create((DoneTask a, DoneTask b) => { return a.CompleteTime.Value.CompareTo(b.CompleteTime.Value); })
                    );
                    if (index < 0) index = ~index;
                    done.Insert(index, t);
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
    public class DoneTask
    {
        public string Name { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? CompleteTime { get; set; }
        // not carried over from task notes for now, but can be specified on completion
        public string Notes { get; set; }
        public DoneTask() { }
        public DoneTask(TodoTask task)
        {
            Name = task.Name;
        }
    }
}