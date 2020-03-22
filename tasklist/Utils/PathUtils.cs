using System.IO;

namespace tasklist
{
    static class PathUtil
    {
        public static string PathExtendFileName(string path, string extension) {
            return Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path) + extension + Path.GetExtension(path));
        }
    }
}