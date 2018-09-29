using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace WcfLogger
{
    [Log(AttributeExclude = true)]
    class Log4netLoggingStrategy : ILoggingStrategy
    {
        private static readonly log4net.ILog log
       = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Log(LoggingArgument arg)
        {
            log.Debug(arg.ToString());
            return true;
        }
    }
}
