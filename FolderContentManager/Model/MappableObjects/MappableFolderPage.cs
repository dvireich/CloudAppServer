using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Repositories;

namespace FolderContentHelper.Model.RestObject
{
    public class MappableFolderPage : IMappable<IFolderPage>
    {
        public MappableFolderPage()
        {
            Type = FolderContentType.FolderPage;
        }
        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public MappableFolderContent[] Content { get; set; }

        public IFolderPage Map()
        {
            var content = new IFolderContent[Content.Length];

            for (var i = 0; i < Content.Length; i++)
            {
                content[i] = Content[i].Map();
            }
            return new FolderPage(Name, Path, content);
        }
    }
}
