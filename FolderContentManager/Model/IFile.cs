namespace FolderContentManager.Model
{

    public interface IFile : IFolderContent
    {
        string FileType { get; set; }
    }
}
