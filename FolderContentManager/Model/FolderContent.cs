using System;
using System.Runtime.Serialization;

namespace CloudAppServer.Model
{
    public class FolderContent : IFolderContent
    {
        public FolderContent(string name, string path, FolderContentType type, string creationTime, string modificationTime, long size)
        {
            Name = name;
            Path = path;
            Type = type;
            CreationTime = creationTime;
            ModificationTime = modificationTime;
            Size = size;
        }

        public FolderContent(string name, string path, FolderContentType type, long size)
        {
            Name = name;
            Path = path;
            Type = type;
            CreationTime = $"{DateTime.Now:G}";
            ModificationTime = $"{DateTime.Now:G}";
            Size = size;
        }

        public FolderContent(string name, string path, FolderContentType type)
        {
            Name = name;
            Path = path;
            Type = type;
            CreationTime = $"{DateTime.Now:G}";
            ModificationTime = $"{DateTime.Now:G}";
        }

        public FolderContent()
        {
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public long Size { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
    }
}
