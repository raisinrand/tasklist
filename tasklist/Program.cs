using System;
using System.IO;
using CommandLine;
using AC = ArgConvert;

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
            [Option('p', "directory", Default = defaultDir, HelpText = "The directory to use for relative paths.")]
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
            [Option('d', "day", Default = null, HelpText= "Which day to populate.")]
            public string Day {get;set;}
        }
        [Verb("push",
            HelpText = "Pushes all scheduled times back by a set amount.")]
        class PushOptions : BaseOptions
        {
            [Value(0, MetaName = "amount", HelpText = "The amount of time to push scheduled times back by.")]
            public string Amount { get; set; }
            [Option('s', "start", Default = null, HelpText = "Starting time of the range to push back.")]
            public string StartTime { get; set; }
            [Option('e', "end", Default = null, HelpText = "Ending time of the range to push back.")]
            public string EndTime { get; set; }
        }
        [Verb("complete", HelpText = "Marks the specified task as complete and removes it from the tasklist.")]
        class CompleteOptions : BaseOptions
        {
            [Value(0, MetaName = "task", HelpText = "The task to mark as completed, identified with a prefix.")]
            public string Task { get; set; }
            [Option('s', "start", Default = null, HelpText = "Time that the task was started.")]
            public string StartTime { get; set; }
            [Option('t', "time", Default = null, HelpText = "Time that the task was complete.")]
            public string Time { get; set; }
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
        [Verb("start", HelpText = "Set starting time of next task to now.")]
        class StartOptions : BaseOptions
        {
            [Value(0, MetaName = "task", HelpText = "The task to set the starting time for.",Required=false,Default=null)]
            public string Task {get; set;}
        }

        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<
                PopulateOptions,
                PushOptions,
                CompleteOptions,
                RescheduleOptions,
                SkipOptions,
                StartOptions
                >(args)
                    .WithParsed<PopulateOptions>(opts => RunPopulate(opts))
                    .WithParsed<PushOptions>(opts => RunPush(opts))
                    .WithParsed<CompleteOptions>(opts => RunComplete(opts))
                    .WithParsed<RescheduleOptions>(opts => RunReschedule(opts))
                    .WithParsed<SkipOptions>(opts => RunSkip(opts))
                    .WithParsed<StartOptions>(opts => RunStart(opts))
                    .WithNotParsed(errs => {});
            }
            catch
            {
                throw;
                // Console.WriteLine(e.Message);
                // return 1;
            }
        }

        static void RunPopulate(PopulateOptions opts)
        {
            Populate(opts);
            Console.WriteLine("Done.");
        }
        static void Populate(PopulateOptions opts)
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
                DayTasks targetDay = null;
                if(opts.Day != null) {
                    DateTime date = AC.ArgConvert.ParseDate(opts.Day).Value;
                    targetDay = GetDayTasks(l,date);
                }
                else {
                    if(!TryCurrentDay(l,out targetDay)) return;
                }
                populator.Populate(targetDay, recurring);
            }
            tasklistLoader.Save(l);
            recurringLoader.Save(recurring);
        }


        static void RunPush(PushOptions opts)
        {
            Push(opts);
            Console.WriteLine("Done.");
        }
        static void Push(PushOptions opts)
        {
            var baseOpts = new ProcessedBaseOptions(opts);
            TasklistLoader tasklistLoader = new TasklistLoader(baseOpts.path);
            Tasklist l = tasklistLoader.Load();
            TasklistPusher pusher = new TasklistPusher();
            DayTasks targetDay;
            if(!TryCurrentDay(l,out targetDay)) return;
            TimeSpan amount = AC.ArgConvert.ParseTimeSpan(opts.Amount).Value;
            TimeSpan? start = AC.ArgConvert.ParseTimeOfDay(opts.StartTime);
            TimeSpan? end = AC.ArgConvert.ParseTimeOfDay(opts.EndTime);
            pusher.Push(targetDay, amount, start, end);
            tasklistLoader.Save(l);
        }

        static void RunComplete(CompleteOptions opts)
        {
            Complete(opts);
            Console.WriteLine("Done.");
        }
        static void Complete(CompleteOptions opts)
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
            TimeSpan? start = AC.ArgConvert.ParseTimeOfDay(opts.StartTime);
            TimeSpan time = AC.ArgConvert.ParseTimeOfDay(opts.Time) ?? DateTime.Now.TimeOfDay;
            c.Complete(targetDay,index,d,start,time);
            tasklistLoader.Save(l);
            doneLoader.Save(d);
        }
 
        static void RunReschedule(RescheduleOptions opts)
        {
            Reschedule(opts);
            Console.WriteLine("Done.");
        }
        static void Reschedule(RescheduleOptions opts)
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
            else date = AC.ArgConvert.ParseDate(opts.ToDate).Value;
            c.Reschedule(l,targetDay,index,date,d);
            tasklistLoader.Save(l);
            doneLoader.Save(d);
        }

        static void RunSkip(SkipOptions opts)
        {
            Skip(opts);
            Console.WriteLine("Done.");
        }
        static void Skip(SkipOptions opts)
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

        static void RunStart(StartOptions opts)
        {
            Start(opts);
            Console.WriteLine("Done.");
        }
        static void Start(StartOptions opts)
        {
            var baseOpts = new ProcessedBaseOptions(opts);
            var tasklistLoader = new TasklistLoader(baseOpts.path);
            Tasklist l = tasklistLoader.Load();
            DayTasks targetDay;
            if(!TryCurrentDay(l,out targetDay)) return;
            int index = 0;
            if(opts.Task != null) index = ParseIndexExcept(targetDay,opts.Task);
            TimeSpan now = DateTime.Now.TimeOfDay;
            targetDay.tasks[index].StartTime = now;
            tasklistLoader.Save(l);
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
        
        // returns true if current day is successfully found
        static DayTasks GetDayTasks(Tasklist l, DateTime date) {
            var f = l.tasksByDay.Find(i => i.day == date);
            if(f == null) {
                var r = new DayTasks() { day = date };
                l.tasksByDay.Add(r);
                return r;
            }
            return f;
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
