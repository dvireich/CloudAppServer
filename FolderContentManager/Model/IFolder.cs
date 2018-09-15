using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Model;

namespace CloudAppServer.Model
{
    public interface IFolder : IFolderContent
    {
        int NumOfPages { get; set; }
        int NextPageToWrite { get; set; }
    }
}
