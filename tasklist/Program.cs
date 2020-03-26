using System;
using System.IO;
using CommandLine;
using tasklist.CommandLine;

namespace tasklist
{
    class Program
    {
        // current day in help refers to first day in listing

        const string defaultDir = "";
        const string defaultFileName = "do.txt";
        const string defaultRecurringExtension = "-recurring";
        static readonly string defaultDoneFileName = Path.Join("log","done.txt");

        // TODO: this needs work. don't like how we're handling this at all.
        class BaseOptions {
            [Option('d', "directory", Default = defaultDir, HelpText = "The directory to use for relative paths.")]
            public string directory { get; set; }
            [Option('f', "filename", Default = defaultFileName, HelpText = "The name of the tasklist file.")]
            public string FileName { get; set; }
            [Option('r', "recurringfile", Default = null, HelpText = "The name of the recurring tasks file.")]
            public string RecurringFileName { get; set; }
            [Option('c', "donefile", Default = null, HelpText = "The name of the done tasks file.")]
            public string DoneFileName { get; set; }
        }
        class ProcessedBaseOptions {
            public string path;
            public string recurringPath;
            public string donePath;
            public ProcessedBaseOptions(BaseOptions opts) {
                string recurringFileName = opts.RecurringFileName;

                if (recurringFileName == null)
                {
                    recurringFileName = PathUtils.PathExtendFileName(opts.FileName, defaultRecurringExtension);
                }
                path = Path.Join(opts.directory,opts.FileName);
                recurringPath = Path.Join(opts.directory,recurringFileName);
                donePath = Path.Join(opts.directory,opts.DoneFileName ?? defaultDoneFileName);
            }
        }
        [Verb("populate",
            HelpText = "Populate days with their assigned recurring tasks. By default only applies to the current day, if it exists.")]
        class PopulateOptions : BaseOptions
        {
            [Option('a', "all", Default = false, HelpText = "Applies to all days instead of just the current day.")]
            public bool All { get; set; }
        }
        [Verb("push",
            HelpText = "Pushes all scheduled times back by a set amount.")]
        class PushOptions : BaseOptions
        {
            [Value(0, MetaName = "amount", HelpText = "The amount of time to push scheduled times back by.")]
            public string Amount { get; set; }
            [Option('s', "start", Default = null, HelpText = "Starting time of the range to push back.")]
            public string startTime { get; set; }
            [Option('e', "end", Default = null, HelpText = "Ending time of the range to push back.")]
            public string endTime { get; set; }
        }
        [Verb("complete", HelpText = "Marks the specified task as complete and removes it from the tasklist.")]
        class CompleteOptions : BaseOptions
        {
            [Value(0, MetaName = "task", HelpText = "The task to mark as completed, identified with a prefix.")]
            public string Task { get; set; }
        }
        [Verb("reschedule", HelpText = "Reassigns the specified task.")]
        class RescheduleOptions : BaseOptions
        {
            [Value(0, MetaName = "task", HelpText = "The task to mark as completed, identified with a prefix.")]
            public string Task { get; set; }
            [Option('t', "to", Default = null, HelpText = "The day to reschedule the task to. Defaults to the day after the current day.")]
            public string ToDate { get; set; }
        }
        [Verb("skip", HelpText = "Skips the specified task.")]
        class SkipOptions : BaseOptions
        {
            [Value(0, MetaName = "task", HelpText = "The task to mark as completed, identified with a prefix.")]
            public string Task { get; set; }
        }

        static int Main(string[] args)
        {
            try
            {
                return Parser.Default.ParseArguments<
                PopulateOptions,
                PushOptions,
                CompleteOptions,
                RescheduleOptions,
                SkipOptions
                >(args)
                    .MapResult(
                    (PopulateOptions opts) => RunPopulateAndReturnExitCode(opts),
                    (PushOptions opts) => RunPushAndReturnExitCode(opts),
                    (CompleteOptions opts) => RunCompleteAndReturnExitCode(opts),
                    (RescheduleOptions opts) => RunRescheduleAndReturnExitCode(opts),
                    (SkipOptions opts) => RunSkipAndReturnExitCode(opts),
                    errs => 1);
            }
            catch
            {
                throw;
                // Console.WriteLine(e.Message);
                // return 1;
            }
        }

        static int RunPopulateAndReturnExitCode(PopulateOptions opts)
        {
            RunPopulate(opts);
            Console.WriteLine("Done.");
            return 1;
        }
        static void RunPopulate(PopulateOptions opts)
        {
            var baseOpts = new ProcessedBaseOptions(opts);
            var recurringLoader = new RecurringTasksLoader(baseOpts.recurringPath);
            TasklistLoader tasklistLoader = new TasklistLoader(baseOpts.path);
            Tasklist l = tasklistLoader.Load();
            var recurring = recurringLoader.Load();
            var populator = new TasklistPopulator();
            if (opts.All)
            {
                populator.Populate(l, recurring);
            }
            else
            {
                DayTasks targetDay;
                if(!TryCurrentDay(l,out targetDay)) return;
                populator.Populate(targetDay, recurring);
            }
            tasklistLoader.Save(l);
            recurringLoader.Save(recurring);
        }


        static int RunPushAndReturnExitCode(PushOptions opts)
        {
            RunPush(opts);
            Console.WriteLine("Done.");
            return 1;
        }
        static void RunPush(PushOptions opts)
        {
            var baseOpts = new ProcessedBaseOptions(opts);
            TasklistLoader tasklistLoader = new TasklistLoader(baseOpts.path);
            Tasklist l = tasklistLoader.Load();
            TasklistPusher pusher = new TasklistPusher();
            DayTasks targetDay;
            if(!TryCurrentDay(l,out targetDay)) return;
            TimeSpan amount = ArgConvert.ParseTimeSpan(opts.Amount).Value;
            TimeSpan? start = ArgConvert.ParseTimeOfDay(opts.startTime);
            TimeSpan? end = ArgConvert.ParseTimeOfDay(opts.endTime);
            pusher.Push(targetDay, amount, start, end);
            tasklistLoader.Save(l);
        }

        static int RunCompleteAndReturnExitCode(CompleteOptions opts)
        {
            RunComplete(opts);
            Console.WriteLine("Done.");
            return 1;
        }
        static void RunComplete(CompleteOptions opts)
        {
            var baseOpts = new ProcessedBaseOptions(opts);
            var tasklistLoader = new TasklistLoader(baseOpts.path);
            Tasklist l = tasklistLoader.Load();
            var doneLoader = new DoneTasksLoader(baseOpts.donePath);
            DoneTasks d = doneLoader.Load();
            TaskCompleter c = new TaskCompleter();
            DayTasks targetDay;
            //TODO: maybe exception in this case?
            if(!TryCurrentDay(l,out targetDay)) return;
            int index = ParseIndexExcept(targetDay,opts.Task);
            c.Complete(targetDay,index,d);
            tasklistLoader.Save(l);
            doneLoader.Save(d);
        }
 
        static int RunRescheduleAndReturnExitCode(RescheduleOptions opts)
        {
            RunReschedule(opts);
            Console.WriteLine("Done.");
            return 1;
        }
        static void RunReschedule(RescheduleOptions opts)
        {
            var baseOpts = new ProcessedBaseOptions(opts);
            var tasklistLoader = new TasklistLoader(baseOpts.path);
            Tasklist l = tasklistLoader.Load();
            var doneLoader = new DoneTasksLoader(baseOpts.donePath);
            DoneTasks d = doneLoader.Load();
            TaskCompleter c = new TaskCompleter();
            DayTasks targetDay;
            if(!TryCurrentDay(l,out targetDay)) return;
            int index = ParseIndexExcept(targetDay,opts.Task);
            DateTime date;
            if(opts.ToDate == null) date = targetDay.day.Value.AddDays(1);
            else date = ArgConvert.ParseDateTime(opts.ToDate).Value;
            c.Reschedule(l,targetDay,index,date,d);
            tasklistLoader.Save(l);
            doneLoader.Save(d);
        }

        static int RunSkipAndReturnExitCode(SkipOptions opts)
        {
            RunSkip(opts);
            Console.WriteLine("Done.");
            return 1;
        }
        static void RunSkip(SkipOptions opts)
        {
            var baseOpts = new ProcessedBaseOptions(opts);
            var tasklistLoader = new TasklistLoader(baseOpts.path);
            Tasklist l = tasklistLoader.Load();
            var doneLoader = new DoneTasksLoader(baseOpts.donePath);
            DoneTasks d = doneLoader.Load();
            TaskCompleter c = new TaskCompleter();
            DayTasks targetDay;
            if(!TryCurrentDay(l,out targetDay)) return;
            int index = ParseIndexExcept(targetDay,opts.Task);
            c.Skip(targetDay,index,d);
            tasklistLoader.Save(l);
            doneLoader.Save(d);
        }


        static int Test()
        {
            var loader2 = new RecurringTasksLoader(
                Path.Combine("test", PathUtils.PathExtendFileName(defaultFileName, defaultRecurringExtension)));
            RecurringTasks s = loader2.Load();
            loader2.Save(s);
            Console.WriteLine("RecurringTasks done.");

            TasklistLoader loader = new TasklistLoader(Path.Combine("test", defaultFileName));
            Tasklist l = loader.Load();
            loader.Save(l);
            Console.WriteLine("Tasklist done.");
            return 1;
        }

        // returns true if current day is successfully found
        static bool TryCurrentDay(Tasklist l, out DayTasks current) {
            current = null;
            if (l.tasksByDay.Count == 0) return false;
            var targetDay = l.tasksByDay[0];
            if (!targetDay.day.HasValue) return false;
            current = targetDay;
            return true;
        }

        static int ParseIndexExcept(DayTasks targetDay, string prefix) {
            int index;
            if(!TasklistUtils.TryParseTaskIndexFromPrefix(targetDay,prefix,out index)) {
                throw new ArgumentException($"Could not find task from prefix '{prefix}'");
            }
            return index;
        } 
    }
}
