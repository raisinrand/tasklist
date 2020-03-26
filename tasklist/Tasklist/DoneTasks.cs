using System.Collections.Generic;

namespace tasklist
{
    public class DoneTasks {
        public List<DoneTask> doneTaskLabels = new List<DoneTask>();
        public List<DoneTask> rescheduledTaskLabels = new List<DoneTask>();
        public List<DoneTask> skippedTaskLabels = new List<DoneTask>();
    }
    public class DoneTask {
        public string label;
        // not carried over from task notes for now
        public string notes;
        public DoneTask() {}
        public DoneTask(TodoTask task) {
            label = task.Name;
        }
    }
}