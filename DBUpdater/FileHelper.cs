using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUpdater
{
    public class FileHelper
    {
        public static void FillUpdateScriptFromFileInto(UpdateScript scriptFile, FileInfo info)
        {
            if (!info.Exists)
            {
                throw new FileNotFoundException(string.Format("Script file {0} not found", info.FullName), info.FullName);
            }
            
            scriptFile.FullFileName = info.FullName;
            DateTime date = info.LastWriteTime;
            scriptFile.FileModifiedDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, 0);
            scriptFile.File = info.Name;
        }
    }
}
