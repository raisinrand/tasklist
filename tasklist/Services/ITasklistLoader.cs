using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public interface ITasklistLoader
    {
        Tasklist Load();
        bool Save(Tasklist tasklist);
        DateTime GetFileLastModifiedTime();
    }
}
