using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudAppServer.Model
{
    public class RestFileObj : RestFolderContent
    {
        public string FileType { get; set; }
        public string[] Value { get; set; }
        public long Size { get; set; }

        public IFile MapToIFileContent()
        {
            return new FileObj(Name, Path, FileType, Value, Size , CreationTime, ModificationTime);
        }
    }
}
