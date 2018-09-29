using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfLogger
{
    [Log(AttributeExclude = true)]
    public class ConsoleLoggingStrategy : ILoggingStrategy
    {
        public ConsoleLoggingStrategy()
        {
            InvokeLogColor = ConsoleColor.Green;
            ResultLogColor = ConsoleColor.Cyan;
            ErrorLogColor = ConsoleColor.Red;
            WarningLogColor = ConsoleColor.Yellow;
            InformationLogColor = ConsoleColor.Blue;
        }

        #region ILoggingStrategy Members

        public bool Log(LoggingArgument arg)
        {
            if (arg == null)
            {
                return false;
            }

            ConsoleColor oldForeground = Console.ForegroundColor;

            switch (arg.LogType)
            {
                case LoggingType.Invoke:
                    Console.ForegroundColor = InvokeLogColor;
                    break;
                case LoggingType.Result:
                    Console.ForegroundColor = ResultLogColor;
                    break;
                case LoggingType.Error:
                    Console.ForegroundColor = ErrorLogColor;
                    break;
                case LoggingType.Warning:
                    Console.ForegroundColor = WarningLogColor;
                    break;
                case LoggingType.Information:
                    Console.ForegroundColor = InformationLogColor;
                    break;
                default:
                    break;
            }

            Console.WriteLine(arg.ToString());

            Console.ForegroundColor = oldForeground;

            return true;
        }

        #endregion

        #region Properties
        public ConsoleColor InvokeLogColor { get; set; }
        public ConsoleColor ResultLogColor { get; set; }
        public ConsoleColor ErrorLogColor { get; set; }
        public ConsoleColor WarningLogColor { get; set; }
        public ConsoleColor InformationLogColor { get; set; }
        #endregion

    }
}
