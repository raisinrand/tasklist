using System;

namespace tasklist
{
    static class TasklistTextDefs
    {
        public const string indent = "    ";
        public const string unscheduledMarker = "UNSCHEDULED";
        public const string repeatSchemeMarker = ":";

        public static string Indent(int count) {
            string res = "";
            for(int i = 0; i < count; i++) {
                res += indent;
            }
            return res;
        }
        public static string FormattedTaskNote(string note)
        {
            string[] lines = note.SplitLines();
            string res = "";
            for (int i = 0; i < lines.Length; i++)
            {
                res += Environment.NewLine + TasklistTextDefs.Indent(2) + lines[i];
            }
            return res;
        }
    }
}