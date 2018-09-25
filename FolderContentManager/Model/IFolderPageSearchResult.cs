using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentHelper.Model
{
    interface IFolderPageSearchResult
    {
        string Name { get; set; }
        string Path { get; set; }
        FolderContentType Type { get; set; }

        IFolderContent[] Results { get; set; }
    }
}
