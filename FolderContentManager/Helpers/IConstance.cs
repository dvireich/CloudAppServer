namespace FolderContentManager.Helpers
{
    public interface IConstance
    {
        string HomeFolderName { get; }

        string HomeFolderPath { get; }

        int MaxFolderContentOnPage { get; }

        string BaseFolderPath { get; set; }

        int DefaultNumberOfElementOnPage { get; set; }
    }
}
