using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public interface ILoader<T>
    {
        T Load();
        void Save(T tasklist);
    }
}
