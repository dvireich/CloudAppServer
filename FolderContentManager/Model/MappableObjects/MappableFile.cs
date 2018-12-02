using FolderContentManager.Repositories;

namespace FolderContentManager.Model.MappableObjects
{
    public class MappableFile : IMappable<IFile>
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public FolderContentType Type { get; set; }
        public string CreationTime { get; set; }
        public string ModificationTime { get; set; }
        public long Size { get; set; }
        public string FileType { get; set; }

        public IFile Map()
        {
            return new FileObj(Name, Path, FileType, Size, CreationTime, ModificationTime);
        }
    }
}
