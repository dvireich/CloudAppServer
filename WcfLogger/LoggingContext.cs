using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Reflection;
using PostSharp.Patterns.Diagnostics;

namespace WcfLogger
{
    [Log(AttributeExclude = true)]
    public class LoggingContext
    {
        #region Contexts methods
        public static bool SetCurrentContextDetails(LoggingContextDetails contextDetails)
        {
            string currContextId = GetCurrentContextId();
            if (currContextId == null)
            {
                return false;
            }

            AddContext(currContextId, contextDetails, true);

            return true;
        }

        public static bool ClearCurrentContextDetails()
        {
            string currContextId = GetCurrentContextId();
            if (currContextId == null)
            {
                return false;
            }

            RemoveContext(currContextId);

            return true;
        }

        protected static string GetCurrentContextId()
        {
            OperationContext currContext = OperationContext.Current;
            if (currContext == null)
            {
                return null;
            }

            return currContext.SessionId;
        }

        protected static void AddContext(string id, 
            LoggingContextDetails contextDetails, 
            bool replaceIfExist)
        {
            if (id == null)
            {
                return;
            }

            lock (Contexts)
            {
                if (replaceIfExist && Contexts.ContainsKey(id))
                {
                    Contexts.Remove(id);
                }

                if (!Contexts.ContainsKey(id) && contextDetails != null)
                {
                    Contexts.Add(id, contextDetails);
                }
            }
        }

        protected static void RemoveContext(string id)
        {
            if (id == null)
            {
                return;
            }

            lock (Contexts)
            {
                if (Contexts.ContainsKey(id))
                {
                    Contexts.Remove(id);
                }
            }
        }
        #endregion

        #region Properties

        #region Contexts
        private static Dictionary<string, LoggingContextDetails> _contexts =
            new Dictionary<string, LoggingContextDetails>();
        protected static Dictionary<string, LoggingContextDetails> Contexts
        {
            get { return _contexts; }
        }
        #endregion

        public static LoggingContext Current
        {
            get
            {
                LoggingContext res = new LoggingContext();

                string currContextId = GetCurrentContextId();

                lock (Contexts)
                {
                    if (Contexts.ContainsKey(currContextId))
                    {
                        res.Details = Contexts[currContextId];
                    }
                }

                return res;
            }
        }

        #region Details
        private LoggingContextDetails _details;
        public LoggingContextDetails Details
        {
            get { return _details ?? (_details = new LoggingContextDetails()); }
            protected set { _details = value; }
        }
        #endregion

        #endregion

        private LoggingArgument CreateArgumentForCommonLog()
        {
            LoggingArgument arg = new LoggingArgument();

            MethodInfo mi = Details.MethodDetails;
            if (mi != null)
            {
                arg.OperationName = mi.Name;

                if (Details.Inputs != null && Details.Inputs.Length > 0)
                {
                    arg.InputsData = new LoggingInputsData
                    {
                        InputParameters = mi.GetParameters().Where(p => !p.IsOut).ToArray(),
                        InputValues = Details.Inputs
                    };
                }
            }

            return arg;
        }

        public bool Log(Exception ex, string text, LoggingType logType)
        {
            LoggingArgument arg = CreateArgumentForCommonLog();
            arg.LogType = logType;

            if (ex != null)
            {
                arg.ExceptionData = new LoggingExceptionData
                {
                    Exception = ex
                };
            }

            if (text != null)
            {
                arg.InformationData = new LoggingInformationData
                {
                    Text = text
                };
            }

            return Details.LoggingStrategy.Log(arg);
        }

        public bool LogError(Exception ex, string text)
        {
            if (Details.LogErrors)
            {
                return Log(ex, text, LoggingType.Error);
            }

            return false;
        }

        public bool LogWarning(Exception ex, string text)
        {
            if (Details.LogWarnings)
            {
                return Log(ex, text, LoggingType.Warning);
            }

            return false;
        }

        public bool LogInformation(string text)
        {
            if (Details.LogInformation)
            {
                return Log(null, text, LoggingType.Information);
            }

            return false;
        }

    }
}
