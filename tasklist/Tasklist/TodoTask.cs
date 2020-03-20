using System;
using System.ComponentModel;
using System.Windows.Input;

namespace tasklist
{
    public class TodoTask : ITodoTask
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public int Difficulty { get; set; }
        public bool IsDue { get; set; }
        //what time task must be completed by, the scheduler will try to fit this task in before the duedate
        public DateTime DueDate { get; set; }
        public bool IsScheduledDay { get; set; }
        public bool IsScheduledTime { get; set; }
        public DateTime ScheduledTime { get; set; }
    }
}