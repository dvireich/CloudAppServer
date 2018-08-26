using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace CloudAppServer.Model
{
    public class RestFolderObj
    {

        public RestFolderObj()
        {
            Type = FolderContentType.Folder;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public FolderContent[] Content { get; set; }

        public IFolder MapToIFolder()
        {
            return new FolderObj(Name, Path, Content);
        }
    }
}
