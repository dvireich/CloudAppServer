using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using PostSharp.Patterns.Diagnostics;

namespace WcfLogger
{
    [Log(AttributeExclude = true)]
    public class LoggingArgument
    {
        public LoggingArgument()
        {
            LogTime = DateTime.Now;
            LogType = LoggingType.Information;
        }

        public DateTime LogTime { get; set; }
        public string OperationName { get; set; }
        public string CallerIp { get; set; }
        public int CallerPort { get; set; }
        public LoggingType LogType { get; set; }
        public LoggingInputsData InputsData { get; set; }
        public LoggingOutputsData OutputsData { get; set; }
        public LoggingReturnValueData ReturnValueData { get; set; }
        public LoggingExceptionData ExceptionData { get; set; }
        public LoggingInformationData InformationData { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} | ", LogTime.ToString("MM/dd/yyyy hh:mm:ss.ffff tt"));
            //sb.AppendLine();

            if(CallerIp != null && CallerPort > 0)
            {
                sb.AppendFormat("Caller Ip: {0} | ", CallerIp);
                sb.AppendFormat("Caller Port: {0} | ", CallerPort);
                //sb.AppendLine();
            }
            
            sb.AppendFormat("Operation: {0} | ", OperationName ?? String.Empty);
            //sb.AppendLine();
            sb.AppendFormat("Log type: {0} | ", LogType);
            //sb.AppendLine();
            

            if (InputsData != null)
            {
                string strInputs = InputsData.ToString();
                if (!string.IsNullOrEmpty(strInputs))
                {
                    sb.AppendFormat("Inputs: {0} | ", strInputs);
                    //sb.AppendLine();
                }
            }

            if (OutputsData != null)
            {
                string strOutputs = OutputsData.ToString();
                if (!string.IsNullOrEmpty(strOutputs))
                {
                    sb.AppendFormat("Outputs: {0} | ", strOutputs);
                    //sb.AppendLine();
                }
            }

            if (ReturnValueData != null)
            {
                string strReturnValue = ReturnValueData.ToString();
                if (!string.IsNullOrEmpty(strReturnValue))
                {
                    sb.AppendFormat("Return value: {0} | ", strReturnValue);
                    //sb.AppendLine();
                }
            }

            if (ExceptionData != null)
            {
                string strException = ExceptionData.ToString();
                if (!string.IsNullOrEmpty(strException))
                {
                    sb.AppendFormat("Exception: {0} | ", strException);
                    //sb.AppendLine();
                }
            }

            if (InformationData != null)
            {
                string strInformation = InformationData.ToString();
                if (!string.IsNullOrEmpty(strInformation))
                {
                    sb.AppendFormat("Information: {0} | ", strInformation);
                    //sb.AppendLine();
                }
            }
            sb.AppendLine();
            return sb.ToString();
        }

    }
}
