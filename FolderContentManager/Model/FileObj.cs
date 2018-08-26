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
        public FileObj(string name, string path) : base(name, path, FolderContentType.File)
        {
        }

        public FileObj() : base(null, null, FolderContentType.File)
        {

        }

    }
}
