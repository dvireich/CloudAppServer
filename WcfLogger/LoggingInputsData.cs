using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using PostSharp.Patterns.Diagnostics;

namespace WcfLogger
{
    [Log(AttributeExclude = true)]
    public class LoggingInputsData
    {
        public ParameterInfo[] InputParameters { get; set; }
        public object[] InputValues { get; set; }

        public override string ToString()
        {
            if (InputParameters == null && InputValues == null)
            {
                return string.Empty;
            }

            if (InputParameters == null)
            {
                return ObjectToStringConverter.ConvertCollectionToString(InputValues);
            }

            if (InputValues == null)
            {
                return ObjectToStringConverter.ConvertCollectionToString(InputParameters.Select(p => p.Name));
            }

            return ObjectToStringConverter.ConvertSomeToString(InputParameters.Select(p => p.Name).ToArray(), InputValues);
        }
    }
}
