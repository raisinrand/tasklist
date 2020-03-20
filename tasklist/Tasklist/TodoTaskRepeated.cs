using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public class TodoTaskRepeated : ITodoTask
    {
        public TodoTaskRepeatedTemplate Source { get; private set; }
        public bool Completed { get; set; }
        public string Name => Source.name;
        public TimeSpan Duration => Source.duration;
        public int Difficulty => Source.difficulty;
        public int Priority => Source.priority;
        public bool IsScheduledDay => true;
        public bool IsScheduledTime => LocalScheduledTimeOfDay != null || Source.timeOfDay != null;
        public DateTime Day { get; set; }
        //scheduled time for this instance of this repeatedtask
        public TimeSpan? LocalScheduledTimeOfDay { get; set; }
        public DateTime ScheduledTime => Day + (LocalScheduledTimeOfDay ?? Source.timeOfDay ?? TimeSpan.Zero);
        public bool IsModified
        {
            get
            {
                bool r = false;
                r |= LocalScheduledTimeOfDay != null;
                r |= Completed;
                return r;
            }
        }
        public TodoTaskRepeated(TodoTaskRepeatedTemplate source, DateTime day)
        {
            Source = source;
            Day = day;
        }
    }
}
