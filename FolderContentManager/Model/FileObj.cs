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

        public FileObj(string name, string path, string fileType, string[] value, long size, string creationTime, string modificationTime) : 
            base(name, path, FolderContentType.File, creationTime, modificationTime)
        {
            FileType = fileType;
            Value = value;
            Size = size;
        }

        public FileObj(string name, string path, string fileType, string[] value, long size) :
            base(name, path, FolderContentType.File)
        {
            FileType = fileType;
            Value = value;
            Size = size;
        }

        public FileObj() : base(null, null, FolderContentType.File, DateTime.Now.ToLongDateString(), DateTime.Now.ToLongDateString())
        {
        }

        public long Size { get; set; }
    }
}
