using System;
using System.IO;

namespace tasklist
{
    class Program
    {
        static void Main(string[] args)
        {
            var loader2 = new RecurringTasksLoader();
            RecurringTasks s = loader2.Load();
            loader2.Save(s);
            Console.WriteLine("RecurringTasks done.");

            TasklistLoader loader = new TasklistLoader();
            Tasklist l = loader.Load();
            loader.Save(l);
            Console.WriteLine("Tasklist done.");
        }
    }
}
