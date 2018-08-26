using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace CloudAppServer.Model
{
    public class FolderObj : IFolder
    {
        public FolderObj(string name, string path, IFolderContent[] content)
        {
            Name = name;
            Path = path;
            Type = FolderContentType.Folder;
            Content = content;
        }
        public FolderObj()
        {
            Type = FolderContentType.Folder;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public IFolderContent[] Content { get; set; }
    }
}
