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

    public interface IFile : IFolderContent, IFileTrackInfo
    {
        string FileType { get; set; }
        string[] Value { get; set; }
    }
}
