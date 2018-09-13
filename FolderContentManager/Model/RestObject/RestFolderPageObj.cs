using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentManager.Model.RestObject
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
        public FolderContent[] Content { get; set; }

        public IFolderPage MapToIFolderPage()
        {
            return new FolderPage(Name, Path, Content);
        }
    }
}
