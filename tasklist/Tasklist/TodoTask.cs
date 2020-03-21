using System;
using System.ComponentModel;
using System.Windows.Input;

namespace tasklist
{
    public class TodoTask : ITodoTask
    {
        public string Name { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public string Notes { get; set; }
    }
}