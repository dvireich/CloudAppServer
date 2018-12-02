using FolderContentManager.Repositories;

namespace FolderContentManager.Model.MappableObjects
{
    public class MappableFolder: IMappable<IFolder>
    {
        public MappableFolder()
        {
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
        public SortType SortType { get; set; }
        public int NumberOfElementOnPage { get; set; }

        public IFolder Map()
        {
            return new FolderObj(Name, Path, NumOfPages, NextPageToWrite, CreationTime, ModificationTime, SortType, NumberOfElementOnPage);
        }
    }
}
