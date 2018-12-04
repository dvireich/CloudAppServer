namespace FolderContentManager.Helpers
{
    public interface IConstance
    {
        string HomeFolderName { get; }

        string HomeFolderPath { get; }

        int MaxFolderContentOnPhysicalPage { get; }

        string BaseFolderPath { get; set; }

        int DefaultNumberOfElementOnPage { get; set; }

        string[] ReservedWords { get; set; }
    }
}
