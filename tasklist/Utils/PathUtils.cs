using System.IO;

namespace tasklist
{
    static class PathUtils
    {
        public static string PathExtendFileName(string path, string extension) {
            return Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path) + extension + Path.GetExtension(path));
        }
        //guarantees that directory for a file exists
        public static void GuaranteeDir(string path) {
            string dir = Path.GetDirectoryName(path);
            if(string.IsNullOrWhiteSpace(dir)) return;
            if(!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
        }
        public static void GuaranteeFile(string path) {
            GuaranteeDir(path);
            if(!File.Exists(path)) {
                File.Create(path);
            }
        }
    }
}