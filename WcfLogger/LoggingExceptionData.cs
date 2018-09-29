using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfLogger
{
    [Log(AttributeExclude = true)]
    public class LoggingExceptionData
    {
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return ObjectToStringConverter.ConvertExceptionToString(Exception);
        }
    }
}
