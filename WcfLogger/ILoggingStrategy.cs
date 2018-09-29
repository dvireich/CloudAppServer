using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfLogger
{
    public interface ILoggingStrategy
    {
        bool Log(LoggingArgument arg);
    }
}
