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
                if(!dayTasks.day.HasValue) continue;
                foreach(RecurringTaskTemplate template in recurring.repeatedTasks) {
                    if(!template.RepeatScheme.RepeatsOn(dayTasks.day.Value)) continue;
                    // TODO maybe use more robust matching?
                    bool dayHasTemplate = dayTasks.tasks.Exists(i => i.Name == template.Name);
                    if(!dayHasTemplate) {
                        dayTasks.tasks.Add(TaskFromTemplate(template));
                    }
                }
            }
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