using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    /// <summary>
    /// defines the template for a repeated todotask
    /// </summary>
    public class TodoTaskRepeatedTemplate
    {
        public string name;
        public TimeSpan duration;
        public int difficulty;
        public int priority;
        public TimeSpan? timeOfDay;
        public RepeatScheme repeatScheme;
    }
}
