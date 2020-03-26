using System;
using System.Collections.Generic;
using System.Linq;

namespace tasklist
{
    static class TextDefs
    {
        public const string indent = "    ";
        // do.txt
        public const string unscheduledMarker = "UNSCHEDULED";
        // do-recurring.txt
        public const string repeatSchemeMarker = ":";
        // done.txt
        public const string rescheduledMarker = "rescheduled";
        public const string skippedMarker = "skipped";

        public static string Indent(int count) {
            string res = "";
            for(int i = 0; i < count; i++) {
                res += indent;
            }
            return res;
        }
        public static string FormattedTaskNote(string note, int indentLevel)
        {
            string[] lines = note.SplitLines();
            string res = "";
            for (int i = 0; i < lines.Length; i++)
            {
                res += Environment.NewLine + TextDefs.Indent(indentLevel) + lines[i];
            }
            return res;
        }

        public static string[] Indent(int count, IEnumerable<string> lines) {
            string[] res = lines.ToArray();
            int i = 0;
            foreach(string line in lines)
            {
                res[i] = Indent(count) + line;
                i++;
            }
            return res;
        }
    }
}