using System;
using System.IO;

namespace tasklist
{
    class Program
    {
        static void Main(string[] args)
        {
            TasklistLoader loader = new TasklistLoader();
            Tasklist l = loader.Load();
            loader.Save(l);
            Console.WriteLine("Tasklist done.");

            var loader2 = new RecurringTasksLoader();
            RecurringTasksScheme s = loader2.Load();
            loader2.Save(s);
            Console.WriteLine("RecurringTasks done.");
        }
    }
}
