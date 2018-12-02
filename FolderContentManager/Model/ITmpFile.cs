namespace FolderContentManager.Model
{
    public interface ITmpFile : IFile
    {
        string TmpCreationPath { get; set; }
    }
}
