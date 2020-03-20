using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tasklist
{
    public static class StringUtils
    {
        public static string[] SplitLines(this string s) {
            return s.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
        }
    }
}
