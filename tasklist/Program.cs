using System;
using System.IO;
using CommandLine;

namespace tasklist
{
    class Program
    {
        [Verb("populate", HelpText = "Populate days with recurring tasks for those days.")]
        class PopulateOptions { //normal options here
        }
        [Verb("test", HelpText = "Test command.")]
        class TestOptions { //normal options here
        }

        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<PopulateOptions, TestOptions>(args)
                .MapResult(
                (PopulateOptions opts) => RunPopulateAndReturnExitCode(opts),
                (TestOptions opts) => RunTestAndReturnExitCode(opts),
                errs => 1);
        }

        static int RunPopulateAndReturnExitCode(PopulateOptions opts)
        {
            var recurringLoader = new RecurringTasksLoader();
            TasklistLoader tasklistLoader = new TasklistLoader();
            Tasklist l = tasklistLoader.Load();
            var recurring = recurringLoader.Load();
            var populator = new TasklistPopulator();
            populator.Populate(l,recurring);
            tasklistLoader.Save(l);
            Console.WriteLine("Done.");
            return 0;
        }

        static int RunTestAndReturnExitCode(TestOptions opts)
        {
            var loader2 = new RecurringTasksLoader();
            RecurringTasks s = loader2.Load();
            loader2.Save(s);
            Console.WriteLine("RecurringTasks done.");

            TasklistLoader loader = new TasklistLoader();
            Tasklist l = loader.Load();
            loader.Save(l);
            Console.WriteLine("Tasklist done.");
            return 0;
        }
    }
}
