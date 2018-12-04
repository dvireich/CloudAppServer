namespace FolderContentManager.Helpers
{
    public class Constance : IConstance
    {
        public string HomeFolderName { get; } = "home";
        public string HomeFolderPath { get; } = "";
        public int MaxFolderContentOnPhysicalPage { get; } = 136;
        public string BaseFolderPath { get; set; } = "C:\\foldercontentmanager";
        public int DefaultNumberOfElementOnPage { get; set; } = 20;
        public string[] ReservedWords { get; set; } = {"search"};
    }
}
