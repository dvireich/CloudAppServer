namespace FolderContentManager.Helpers
{
    public class Constance : IConstance
    {
        public string HomeFolderName { get; } = "home";
        public string HomeFolderPath { get; } = "";
        public int MaxFolderContentOnPage { get; } = 136;
        public string BaseFolderPath { get; set; } = "C:\\foldercontentmanager";
        public int DefaultNumberOfElementOnPage { get; set; } = 20;
    }
}
