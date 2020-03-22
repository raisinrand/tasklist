using System;
using System.Collections.Generic;

namespace tasklist
{
    public class Tasklist
    {
        // RI: tasksByDay should be sorted in ascending chronological order,
        // with the unscheduled day placed last (represented by a null day value).
        // There can only be one unscheduled day.
        public List<DayTasks> tasksByDay = new List<DayTasks>();
    }
}
