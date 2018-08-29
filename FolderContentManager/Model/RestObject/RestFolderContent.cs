using System.Runtime.Serialization;

namespace CloudAppServer.Model
{
    public class RestFolderContent
    {
        public RestFolderContent()
        {

        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }

        public IFolderContent MapToIFolderContent()
        {
            return new FolderContent(Name, Path, Type);
        }
    }
}
