using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;

namespace tasklist
{
    public abstract class FileLoader<T>
    {
        public T Load()
        {
            string path = GetLocalPath();
            string[] lines = File.ReadAllLines(path);
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
                File.Copy(path, backup, true);
                File.WriteAllLines(copy, lines);
                File.Copy(copy, path, true);
            }
            finally
            {
                File.Delete(copy);
            }
        }

        //TODO: this should read directly from stream to support big tasklist
        protected abstract T Parse(string[] lines);
        //TODO: this should write directly to stream to support big tasklist
        protected abstract string[] Write(T obj);


        protected abstract string FileName { get; }
        protected abstract string CopyExtension { get; }
        protected abstract string BackupExtension { get; }

        // TODO: how do we do path? how do we configure loader through this interface?
        string GetLocalPath()
        {
            // return $"{settingsManager.CurrentSettings.Directory}/{fileName}";
            return $"C:/Users/thebi/Documents/work-local/tasklist-cmd/tasklist/test/{FileName}";
        }
        //TODO: use system.io.path for this kind of stuff
        string GetLocalCopyPath()
        {
            return GetLocalPath().Insert(GetLocalPath().LastIndexOf('.'), CopyExtension);
        }
        string GetLocalBackupPath()
        {
            return GetLocalPath().Insert(GetLocalPath().LastIndexOf('.'), BackupExtension);
        }
        public DateTime GetFileLastModifiedTime()
        {
            return File.GetLastWriteTime(GetLocalPath());
        }
    }
}