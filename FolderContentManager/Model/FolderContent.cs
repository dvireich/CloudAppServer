using System.Runtime.Serialization;

namespace CloudAppServer.Model
{
    public class FolderContent : IFolderContent
    {
        public FolderContent(string name, string path, FolderContentType type)
        {
            Name = name;
            Path = path;
            Type = type;
        }

        public FolderContent()
        {

        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
    }
}
