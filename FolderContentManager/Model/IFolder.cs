using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace CloudAppServer.Model
{
    public interface IFolder : IFolderContent
    {
        IFolderContent[] Content { get; set; }
    }
}
