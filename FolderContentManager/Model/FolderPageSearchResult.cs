using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentHelper.Model
{
    public class FolderPageSearchResult : IFolderPage
    {
        public FolderPageSearchResult(string searchString, IFolderContent[] content)
        {
            Name = "search";
            Path = searchString;
            Content = content;
            CreationTime = $"{DateTime.Now:G}";
            ModificationTime = $"{DateTime.Now:G}";
            Type = FolderContentType.FolderPageResult;
        }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public IFolderContent[] Content { get; set; }
    }
}
