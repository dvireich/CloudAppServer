namespace FolderContentManager.Model
{
    public interface IFolderPage : IFolderContent
    {
        IFolderContent[] Content { get; set; }
    }
}
