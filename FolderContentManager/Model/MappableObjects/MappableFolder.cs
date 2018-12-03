using FolderContentManager.Repositories;

namespace FolderContentManager.Model.MappableObjects
{
    public class MappableFolder: IMappable<IFolder>
    {
        public MappableFolder()
        {
            Type = FolderContentType.Folder;
            NumOfPhysicalPages = NextPhysicalPageToWrite = 1;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public int NumOfPhysicalPages { get; set; }
        public int NextPhysicalPageToWrite { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
        public SortType SortType { get; set; }
        public int NumberOfElementPerPage { get; set; }

        public IFolder Map()
        {
            return new FolderObj(Name, Path, NumOfPhysicalPages, NextPhysicalPageToWrite, CreationTime, ModificationTime, SortType, NumberOfElementPerPage);
        }
    }
}
