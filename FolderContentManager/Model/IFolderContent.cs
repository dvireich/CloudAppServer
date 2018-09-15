using System.Runtime.Serialization;
using FolderContentManager.Model;

namespace CloudAppServer.Model
{
    public interface IFolderContent: IFolderContentTrackInfo
    {
        string Name { get; set; }
        string Path { get; set; }
        FolderContentType Type { get; set; }
    }
}
