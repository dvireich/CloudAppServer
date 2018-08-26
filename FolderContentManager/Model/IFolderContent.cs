using System.Runtime.Serialization;

namespace CloudAppServer.Model
{
    public interface IFolderContent
    {
        string Name { get; set; }
        string Path { get; set; }
        FolderContentType Type { get; set; }
    }
}
