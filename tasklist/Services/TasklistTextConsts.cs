namespace tasklist
{
    static class TasklistTextDefs
    {
        public const string indent = "    ";
        public const string unscheduledMarker = "UNSCHEDULED";

        public static string Indent(int count) {
            string res = "";
            for(int i = 0; i < count; i++) {
                res += indent;
            }
            return res;
        }
    }
}