using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace tasklist
{
    // Populates tasklists with specified recurring tasks.
    public class TasklistPopulator
    {
        public TasklistPopulator() 
        {}

        public void Populate(Tasklist tasklist, RecurringTasks recurring) {
            foreach(DayTasks dayTasks in tasklist.tasksByDay) {
                Populate(dayTasks, recurring);
            }
        }

        public void Populate(DayTasks dayTasks, RecurringTasks recurring) {
            if(!dayTasks.day.HasValue) return;
            var orderedQueue = new Queue<RecurringTaskTemplate>();
            foreach(RecurringTaskTemplate template in recurring.repeatedTasks) {
                if(template.Ordering != null) {
                    orderedQueue.Enqueue(template);
                    continue;
                } 
                if(!template.RepeatScheme.RepeatsOn(dayTasks.day.Value)) continue;
                bool dayHasTemplate = dayTasks.tasks.Exists(i => i.Name.Equals(template.Name));
                if(dayHasTemplate) continue;
                int insertIndex = dayTasks.tasks.Count;
                dayTasks.tasks.Insert(insertIndex,TaskFromTemplate(template));
            }
            foreach(RecurringTaskTemplate template in orderedQueue) {
                if(!template.RepeatScheme.RepeatsOn(dayTasks.day.Value)) continue;
                bool dayHasTemplate = dayTasks.tasks.Exists(i => i.Name.Equals(template.Name));
                if(dayHasTemplate) continue;
                int insertIndex = GetOrderedIndex(dayTasks, template);
                dayTasks.tasks.Insert(insertIndex,TaskFromTemplate(template));
            }
        }

        // requires that template ordering is not null
        int GetOrderedIndex(DayTasks dayTasks, RecurringTaskTemplate template) {
            int targetIndex;
            if(TasklistUtils.TryParseTaskIndexFromPrefix(dayTasks, template.Ordering.targetPrefix,out targetIndex)) {
                switch(template.Ordering.mode) {
                    case RecurringOrdering.Mode.Before:
                        return targetIndex;
                    case RecurringOrdering.Mode.After:
                        return targetIndex+1;
                }
            }
            return dayTasks.tasks.Count;
        }

        TodoTask TaskFromTemplate(RecurringTaskTemplate template) {
            return new TodoTask {
                Name = template.Name,
                ScheduledTime = template.TimeOfDay,
                Notes = template.Notes
            };
        }
    }
}