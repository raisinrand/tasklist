using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public class Tasklist
    {
        // public List<TodoTaskRepeatedTemplate> repeatedTasks;
        public List<DayTasks> tasksByDay;
        //TODO: make sure that EVERY TASKLIST IS GENERATED WITH "UNSCHEDULED" DAYTASK PLS
        public static Tasklist CreateDefault()
        {
            throw new NotImplementedException();
        }
    }
    //contains data for all tasks assigned to a day. i don't like this name, though.
    public class DayTasks
    {
        public DateTime? day;
        public List<ITodoTask> tasks;
    }
}
