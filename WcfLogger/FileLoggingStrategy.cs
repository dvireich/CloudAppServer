using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PostSharp.Patterns.Diagnostics;

namespace WcfLogger
{
    [Log(AttributeExclude = true)]
    public class FileLoggingStrategy : ILoggingStrategy
    {
        public bool Log(LoggingArgument arg)
        {
            if (arg == null)
            {
                return false;
            }

            try
            {
                string logFilePath = FilePath ?? "C:\\Example.txt";

                using (FileStream fs = File.Open(logFilePath, FileMode.Append, FileAccess.Write))
                {
                    using (TextWriter tw = new StreamWriter(fs))
                    {
                        tw.Write(arg.ToString());
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public string FilePath { get; set; }
    }
}
