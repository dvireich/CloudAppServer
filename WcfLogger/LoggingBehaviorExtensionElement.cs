using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.Reflection;
using PostSharp.Patterns.Diagnostics;

namespace WcfLogger
{
    [Log(AttributeExclude = true)]
    public class LoggingBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(WcfLoggingAttribute); }
        }

        protected override object CreateBehavior()
        {
            return new WcfLoggingAttribute
            {
                Log = Log,
                LogBeforeCall = LogBeforeCall,
                LogAfterCall = LogAfterCall,
                LogErrors = LogErrors,
                LogWarnings = LogWarnings,
                LogInformation = LogInformation,
                LogReturnVal = LogReturnVal,
                LogArguments = LogArguments,
                LoggingStrategyType = ConvertStringToType(LoggingStrategyType)
            };
        }

        private Type ConvertStringToType(string strType)
        {
            if (string.IsNullOrEmpty(strType))
            {
                return null;
            }

            Type res = null;

            try
            {
                int firstCommaIndex = strType.IndexOf(",");
                if (firstCommaIndex > 0)
                {
                    string typeFullName = strType.Substring(0, firstCommaIndex);
                    string assemblyFullName = strType.Substring(firstCommaIndex + 1);

                    Assembly typeAssembly = Assembly.Load(assemblyFullName);
                    if (typeAssembly != null)
                    {
                        res = typeAssembly.GetType(typeFullName);
                    }
                }
            }
            catch
            {
            }

            return res;
        }

        #region Properties

        [ConfigurationProperty("Log", DefaultValue = true)]
        public bool Log
        {
            get { return (bool)this["Log"]; }
            set { this["Log"] = value; }
        }

        [ConfigurationProperty("LogArguments", DefaultValue = true)]
        public bool LogArguments
        {
            get { return (bool)this["LogArguments"]; }
            set { this["LogArguments"] = value; }
        }

        [ConfigurationProperty("logBeforeCall", DefaultValue = true)]
        public bool LogBeforeCall
        {
            get { return (bool)this["logBeforeCall"]; }
            set { this["logBeforeCall"] = value; }
        }


        [ConfigurationProperty("logAfterCall", DefaultValue = true)]
        public bool LogAfterCall
        {
            get { return (bool)this["logAfterCall"]; }
            set { this["logAfterCall"] = value; }
        }

        [ConfigurationProperty("logErrors", DefaultValue = true)]
        public bool LogErrors
        {
            get { return (bool)this["logErrors"]; }
            set { this["logErrors"] = value; }
        }

        [ConfigurationProperty("logWarnings", DefaultValue = true)]
        public bool LogWarnings
        {
            get { return (bool)this["logWarnings"]; }
            set { this["logWarnings"] = value; }
        }

        [ConfigurationProperty("logInformation", DefaultValue = true)]
        public bool LogInformation
        {
            get { return (bool)this["logInformation"]; }
            set { this["logInformation"] = value; }
        }

        [ConfigurationProperty("logReturnVal", DefaultValue = true)]
        public bool LogReturnVal
        {
            get { return (bool)this["logReturnVal"]; }
            set { this["logReturnVal"] = value; }
        }

        [ConfigurationProperty("loggingStrategyType")]
        public string LoggingStrategyType
        {
            get { return (string)this["loggingStrategyType"]; }
            set { this["loggingStrategyType"] = value; }
        }

        #endregion
    }
}
