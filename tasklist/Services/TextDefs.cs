using System;
using System.Collections.Generic;
using System.Linq;
using ArgConvert.Converters;

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


        // maybe move this
        // null if can't be found
        public static TimeSpan? GetStartTimeFromTaskName(string name, TimeOfDayToStringConverter converter, out int index) {
            int[] todLengthRange = converter.PossibleLengthRange();
            for(int i = todLengthRange[1]-1; i >= todLengthRange[0]; i-- ) {
                int checkIndex = name.Length-i;
                if(checkIndex < 0) continue;
                string potentialText = name.Substring(checkIndex);
                TimeSpan? res = (TimeSpan?)converter.ConvertBack(potentialText);
                if(res.HasValue) {
                    index = checkIndex;
                    return res;
                }
            }
            index = -1;
            return null;
        }
    }
}