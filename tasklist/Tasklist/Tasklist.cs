using System;
using System.Collections.Generic;

namespace tasklist
{
    public class Tasklist
    {
        // public List<TodoTaskRepeatedTemplate> repeatedTasks;
        public List<DayTasks> tasksByDay = new List<DayTasks>();
        //TODO: make sure that EVERY TASKLIST IS GENERATED WITH "UNSCHEDULED" DAYTASK PLS
        public static Tasklist CreateDefault()
        {
            throw new NotImplementedException();
        }
    }
}
