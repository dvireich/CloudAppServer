using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentHelper.Model.RestObject
{
    public class RestFolderPageObj
    {
        public RestFolderPageObj()
        {
            Type = FolderContentType.FolderPage;
        }
        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public RestFolderContent[] Content { get; set; }

        public IFolderPage MapToIFolderPage()
        {
            var content = new IFolderContent[Content.Length];

            for (var i = 0; i < Content.Length; i++)
            {
                content[i] = Content[i].MapToIFolderContent();
            }
            return new FolderPage(Name, Path, content);
        }
    }
}
