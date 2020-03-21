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
    public class RecurringTaskTemplate
    {
        public string Name { get; set; }
        public TimeSpan? TimeOfDay { get; set; }
        public string Notes { get; set; }
        public RepeatScheme RepeatScheme { get; set; }
    }
}
