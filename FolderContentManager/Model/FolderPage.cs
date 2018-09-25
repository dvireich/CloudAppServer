using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentHelper.Model
{
    public class FolderPage : IFolderPage
    {
        public FolderPage(string name, string path, IFolderContent[] content)
        {
            Name = name;
            Path = path;
            Content = content;
            Type = FolderContentType.FolderPage;
        }

        public FolderPage()
        {
            Type = FolderContentType.FolderPage;
            Content = new IFolderContent[0];
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public IFolderContent[] Content { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
    }
}
