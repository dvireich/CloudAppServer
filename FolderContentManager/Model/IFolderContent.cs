using System.Runtime.Serialization;
using FolderContentHelper.Model;

namespace CloudAppServer.Model
{
    public interface IFolderContent: IFolderContentTrackInfo
    {
        string Name { get; set; }
        string Path { get; set; }
        FolderContentType Type { get; set; }
        long Size { get; set; }
    }
}
