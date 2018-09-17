using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace CloudAppServer.Model
{
    public class FolderObj : IFolder
    {
        public FolderObj(string name, string path, int numOfPages, int nextPageToWrite, DateTime creationTime, DateTime modificationTime)
        {
            Name = name;
            Path = path;
            NumOfPages = numOfPages;
            NextPageToWrite = nextPageToWrite;
            CreationTime = creationTime.ToString("{0:G}");
            ModificationTime = modificationTime.ToString("{0:G}");
            Type = FolderContentType.Folder;
        }

        public FolderObj(string name, string path, DateTime creationTime, DateTime modificationTime) : this()
        {
            Name = name;
            Path = path;
            CreationTime = string.Format("{0:G}", creationTime);
            ModificationTime = string.Format("{0:G}", modificationTime);
        }

        public FolderObj(string name, string path) : this()
        {
            Name = name;
            Path = path;
            CreationTime = string.Format("{0:G}", DateTime.Now);
            ModificationTime = string.Format("{0:G}", DateTime.Now);
        }

        public FolderObj()
        {
            CreationTime = string.Format("{0:G}", DateTime.Now);
            ModificationTime = string.Format("{0:G}", DateTime.Now);
            Type = FolderContentType.Folder;
            NumOfPages = NextPageToWrite = 1;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public int NumOfPages { get; set; }
        public int NextPageToWrite { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
    }
}
