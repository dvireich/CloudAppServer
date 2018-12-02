namespace FolderContentManager.Model
{
    public interface IFolderContent: IFolderContentTrackInfo
    {
        string Name { get; set; }
        string Path { get; set; }
        FolderContentType Type { get; set; }
        long Size { get; set; }
    }
}
