using System;
using System.IO;

namespace tasklist
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                TasklistLoader loader = new TasklistLoader();
                Tasklist l = loader.Load();
                loader.Save(l);
                Console.WriteLine("Tasklist done.");
            } catch(Exception e) {
                throw e;
            }

            try {
                var loader2 = new RecurringTasksLoader();
                RecurringTasksScheme s = loader2.Load();
                loader2.Save(s);
                Console.WriteLine("RecurringTasks done.");
            } catch(Exception e) {
                throw e;
            }
        }
    }
}
