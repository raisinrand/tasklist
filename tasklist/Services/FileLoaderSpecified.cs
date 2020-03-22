using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


namespace tasklist
{
    public abstract class FileLoaderSpecified<T> : FileLoader<T>
    {
        protected override string CopyExtension => "-copy";
        protected override string BackupExtension => "-backup";
    }
}