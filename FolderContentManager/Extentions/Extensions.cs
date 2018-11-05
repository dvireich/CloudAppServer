using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentManager
{
    public static class Extensions
    {
        public static string ReplacePrefixInString(this string source, string oldPrefix, string newPrefix)
        {
            if (!source.StartsWith(oldPrefix)) return oldPrefix;

            var suffixPath = source.Substring(oldPrefix.Length, source.Length - oldPrefix.Length);
            return $"{newPrefix}{suffixPath}";

        }
    }
}
