using FolderContentManager.Repositories;

namespace FolderContentManager.Model.MappableObjects
{
    public class MappableFolderContent : IMappable<IFolderContent>
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
        public long Size { get; set; }

        public virtual IFolderContent Map()
        {
            return new FolderContent(Name, Path, Type, CreationTime, ModificationTime, Size);
        }
    }
}
