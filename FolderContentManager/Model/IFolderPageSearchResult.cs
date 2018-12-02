namespace FolderContentManager.Model
{
    interface IFolderPageSearchResult
    {
        string Name { get; set; }
        string Path { get; set; }
        FolderContentType Type { get; set; }

        IFolderContent[] Results { get; set; }
    }
}
