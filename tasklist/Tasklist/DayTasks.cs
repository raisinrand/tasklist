using System;
using System.Collections.Generic;

namespace tasklist
{
    //contains data for all tasks assigned to a day. i don't like this name, though.
    public class DayTasks
    {
        public DateTime? day;
        public List<ITodoTask> tasks;
    }
}