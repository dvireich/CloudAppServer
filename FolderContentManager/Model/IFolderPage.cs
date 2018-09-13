using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentManager.Model
{
    public interface IFolderPage : IFolderContent
    {
        IFolderContent[] Content { get; set; }
    }
}
