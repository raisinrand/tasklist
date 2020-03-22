using System;
using System.IO;
using CommandLine;

namespace tasklist
{
    class Program
    {
        const string defaultFileName = "do.txt";
        const string defaultRecurringExtension = "-recurring";


        [Verb("populate",
            HelpText = "Populate days with their assigned recurring tasks. By default only applies to the first day in the list, if it exists.")]
        class PopulateOptions
        {
            [Option('f', "filename", Default = defaultFileName, HelpText = "The name of the tasklist file.")]
            public string FileName { get; set; }
            // default is null here because populate method is expected to set this on its own.
            [Option('r', "recurring", Default = null, HelpText = "The name of the recurring tasks file.")]
            public string RecurringFileName { get; set; }
            [Option('a', "all", Default = false, HelpText = "Applies to all days instead of just the first day.")]
            public bool All { get; set; }
        }
        [Verb("test", HelpText = "Test command.")]
        class TestOptions
        {
        }

        static int Main(string[] args)
        {
            try
            {
                return Parser.Default.ParseArguments<PopulateOptions, TestOptions>(args)
                    .MapResult(
                    (PopulateOptions opts) => RunPopulateAndReturnExitCode(opts),
                    (TestOptions opts) => RunTestAndReturnExitCode(opts),
                    errs => 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }

        static int RunPopulateAndReturnExitCode(PopulateOptions opts)
        {
            RunPopulate(opts);
            Console.WriteLine("Done.");
            return 1;
        }
        static void RunPopulate(PopulateOptions opts) {
            string recurringFileName = opts.RecurringFileName;
            if (recurringFileName == null)
            {
                recurringFileName = PathUtil.PathExtendFileName(opts.FileName, defaultRecurringExtension);
            }
            var recurringLoader = new RecurringTasksLoader(recurringFileName);
            TasklistLoader tasklistLoader = new TasklistLoader(opts.FileName);
            Tasklist l = tasklistLoader.Load();
            var recurring = recurringLoader.Load();
            var populator = new TasklistPopulator();
            if (opts.All)
            {
                populator.Populate(l, recurring);
            }
            else
            {
                if(l.tasksByDay.Count == 0) return;
                var targetDay = l.tasksByDay[0];
                if(!targetDay.day.HasValue) return;
                populator.Populate(targetDay, recurring);
            }
            tasklistLoader.Save(l);
        }

        static int RunTestAndReturnExitCode(TestOptions opts)
        {
            var loader2 = new RecurringTasksLoader(PathUtil.PathExtendFileName(defaultFileName, defaultRecurringExtension));
            RecurringTasks s = loader2.Load();
            loader2.Save(s);
            Console.WriteLine("RecurringTasks done.");

            TasklistLoader loader = new TasklistLoader(defaultFileName);
            Tasklist l = loader.Load();
            loader.Save(l);
            Console.WriteLine("Tasklist done.");
            return 1;
        }
    }
}
