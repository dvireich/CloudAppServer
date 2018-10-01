using System;
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
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
        public long Size { get; set; }
        public string FileType { get; set; }

        public IFolderContent MapToIFolderContent()
        {
            if (Type == FolderContentType.File)
            {
                return new FileObj(Name, Path, FileType, Size, CreationTime, ModificationTime);
            }

            return new FolderContent(Name, Path, Type, CreationTime, ModificationTime);
        }
    }
}
