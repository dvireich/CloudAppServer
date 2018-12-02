using System;

namespace FolderContentManager.Model
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
            SortType sortType,
            int numberOfElementOnPage)
        {
            Name = name;
            Path = path;
            NumOfPhysicalPages = numOfPages;
            NextPhysicalPageToWrite = nextPageToWrite;
            CreationTime = creationTime;
            ModificationTime = modificationTime;
            Type = FolderContentType.Folder;
            SortType = sortType;
            NumberOfElementPerPage = numberOfElementOnPage;
        }

        public FolderObj(string name, string path, DateTime creationTime, DateTime modificationTime) : this()
        {
            Name = name;
            Path = path;
            CreationTime = $"{creationTime:G}";
            ModificationTime = $"{modificationTime:G}";
        }

        public FolderObj(string name, string path, int numberOfElementOnPage) : this()
        {
            Name = name;
            Path = path;
            CreationTime = $"{DateTime.Now:G}";
            ModificationTime = $"{DateTime.Now:G}";
            SortType = SortType.Name;
            NumberOfElementPerPage = numberOfElementOnPage;
        }

        public FolderObj()
        {
            CreationTime = $"{DateTime.Now:G}";
            ModificationTime = $"{DateTime.Now:G}";
            Type = FolderContentType.Folder;
            NumOfPhysicalPages = NextPhysicalPageToWrite = 1;
            NumberOfElementPerPage = 20;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public long Size { get; set; }
        public int NumOfPhysicalPages { get; set; }
        public int NextPhysicalPageToWrite { get; set; }
        public SortType SortType { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
        public int NumberOfElementPerPage { get; set; }
    }
}
