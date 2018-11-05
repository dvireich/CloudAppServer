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
    public class FolderObj : IFolder
    {
        public FolderObj(
            string name, 
            string path, 
            int numOfPages, 
            int nextPageToWrite,
            string creationTime,
            string modificationTime, 
            SortType sortType)
        {
            Name = name;
            Path = path;
            NumOfPages = numOfPages;
            NextPageToWrite = nextPageToWrite;
            CreationTime = creationTime;
            ModificationTime = modificationTime;
            Type = FolderContentType.Folder;
            SortType = sortType;
        }

        public FolderObj(string name, string path, DateTime creationTime, DateTime modificationTime) : this()
        {
            Name = name;
            Path = path;
            CreationTime = $"{creationTime:G}";
            ModificationTime = $"{modificationTime:G}";
        }

        public FolderObj(string name, string path) : this()
        {
            Name = name;
            Path = path;
            CreationTime = $"{DateTime.Now:G}";
            ModificationTime = $"{DateTime.Now:G}";
            SortType = SortType.Name;
        }

        public FolderObj()
        {
            CreationTime = $"{DateTime.Now:G}";
            ModificationTime = $"{DateTime.Now:G}";
            Type = FolderContentType.Folder;
            NumOfPages = NextPageToWrite = 1;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public long Size { get; set; }
        public int NumOfPages { get; set; }
        public int NextPageToWrite { get; set; }
        public SortType SortType { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
    }
}
