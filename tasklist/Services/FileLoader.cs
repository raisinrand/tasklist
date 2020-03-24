using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;

namespace tasklist
{
    public abstract class FileLoader<T> : ILoader<T>
    {
        public T Load()
        {
            string path = GetLocalPath();
            string[] lines;
            if(!File.Exists(path) && IgnoreMissing) {
                lines = new string[0];
            }
            else lines = File.ReadAllLines(path);
            T obj = Parse(lines);
            return obj;
        }
        public void Save(T obj)
        {
            string[] lines = Write(obj);
            Save(lines);
        }
        public void Save(string[] lines)
        {
            string path = GetLocalPath();
            string backup = GetLocalBackupPath();
            string copy = GetLocalCopyPath();
            try
            {
                if(File.Exists(path)) {
                    File.Copy(path, backup, true);
                }
                File.WriteAllLines(copy, lines);
                File.Copy(copy, path, true);
            }
            finally
            {
                File.Delete(copy);
            }
        }

        protected abstract T Parse(string[] lines);
        protected abstract string[] Write(T obj);


        protected abstract string Path { get; }
        protected abstract string CopyExtension { get; }
        protected abstract string BackupExtension { get; }

        protected virtual bool IgnoreMissing { get { return false; } }

        string GetLocalPath()
        {
            return $"{Path}";
        }
        string GetLocalCopyPath()
        {
            return PathUtils.PathExtendFileName(Path, CopyExtension);
        }
        string GetLocalBackupPath()
        {
            return PathUtils.PathExtendFileName(Path, BackupExtension);
        }
    }
}