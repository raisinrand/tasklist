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
        public string name;
        public TimeSpan? timeOfDay;
        public string notes;
        public RepeatScheme repeatScheme;
    }
}
