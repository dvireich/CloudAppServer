using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using PostSharp.Patterns.Diagnostics;

namespace WcfLogger
{
    [Log(AttributeExclude = true)]
    public static class ObjectToStringConverter
    {
        public static string ConvertSomeToString(string[] valuesNames, object[] values)
        {
            if (values == null || valuesNames == null)
            {
                return string.Empty;
            }

            int valuesCount = values.Length;
            if (valuesNames.Length != valuesCount)
            {
                return string.Empty;
            }

            StringBuilder objectsData = new StringBuilder();
            for (int objectInx = 0; objectInx < valuesCount; objectInx++)
            {
                if (objectInx > 0)
                {
                    objectsData.Append("; ");
                }

                objectsData.Append(ConvertToString(valuesNames[objectInx], values[objectInx]));
            }

            return objectsData.ToString();
        }

        public static string ConvertToString(string valueName, object value)
        {
            return string.Format("{0}={1}", valueName, ConvertToString(value));
        }

        public static string ConvertToString(object value)
        {
            if (value == null)
            {
                return "null";
            }

            Type valueType = value.GetType();

            if (valueType == typeof(string) || valueType.IsEnum || IsParsable(valueType))
            {
                return value.ToString();
            }

            if (value is Exception)
            {
                return ConvertExceptionToString(value as Exception);
            }

            if (value is IEnumerable)
            {
                return ConvertCollectionToString(value as IEnumerable);
            }

            if (value is Type)
            {
                return ConvertTypeToString(value as Type);
            }

            return ConvertClassToString(value);
        }

        public static string ConvertCollectionToString(IEnumerable col)
        {
            if (col == null)
            {
                return "null";
            }

            StringBuilder sb = new StringBuilder("{");

            bool isFirstElement = true;

            foreach (var elem in col)
            {
                if (!isFirstElement)
                {
                    sb.Append(", ");
                }

                sb.Append(ConvertToString(elem));

                isFirstElement = false;
            }

            sb.Append("}");

            return sb.ToString();
        }

        public static string ConvertClassToString(object value)
        {
            if (value == null)
            {
                return "null";
            }

            StringBuilder sb = new StringBuilder("{");

            bool isFirstElement = true;

            IEnumerable<PropertyInfo> classProperties =
                value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead);
            foreach (PropertyInfo pi in classProperties)
            {
                if (!isFirstElement)
                {
                    sb.Append(", ");
                }

                sb.Append(ConvertToString(pi.Name, pi.GetValue(value, null)));

                isFirstElement = false;
            }

            IEnumerable<FieldInfo> classFields = value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo fi in classFields)
            {
                if (fi.IsStatic)
                {
                    continue;
                }

                if (!isFirstElement)
                {
                    sb.Append(", ");
                }

                sb.Append(ConvertToString(fi.Name, fi.GetValue(value)));

                isFirstElement = false;
            }

            sb.Append("}");

            return sb.ToString();
        }

        public static string ConvertExceptionToString(Exception e)
        {
            if (e == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(e.Message ?? string.Empty);

            if (e.InnerException != null)
            {
                sb.Append(" -> ");
                sb.Append(ConvertExceptionToString(e.InnerException));
            }

            return sb.ToString();
        }

        public static string ConvertTypeToString(Type t)
        {
            if (t == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(t.FullName);
            sb.Append(", ");
            sb.Append(t.Assembly.FullName);

            return sb.ToString();
        }

        private static bool IsParsable(Type t)
        {
            if (t == null)
            {
                return false;
            }

            MethodInfo mi = t.GetMethod("Parse", new[] { typeof(string) });
            return mi != null;
        }
    }
}
