using System;

namespace tasklist
{
    class Program
    {
        static void Main(string[] args)
        {
            ITasklistLoader loader = new TasklistLoader();

            Tasklist l = loader.Load();
            loader.Save(l);
        }
    }
}
