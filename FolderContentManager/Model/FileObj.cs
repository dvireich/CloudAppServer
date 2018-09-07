using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudAppServer.Model
{
    public class FileObj : FolderContent, IFile
    {

        public string FileType { get; set; }
        public string[] Value { get; set; }

        public FileObj(string name, string path, string fileType, string[] value) : base(name, path, FolderContentType.File)
        {
            FileType = fileType;
            Value = value;
        }

        public FileObj() : base(null, null, FolderContentType.File)
        {

        }

    }
}
