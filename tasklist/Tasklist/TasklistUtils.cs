namespace tasklist
{
    static class TasklistUtils
    {
        public static bool TryParseTaskIndexFromPrefix(DayTasks day, string prefix, out int res) {
            res = -1;
            for(int i = 0; i < day.tasks.Count; i++) {
                ITodoTask task = day.tasks[i];
                if(task.Name.StartsWith(prefix,true,null)) {
                    res = i;
                    return true;
                }
            }
            return false;
        }
    }
}