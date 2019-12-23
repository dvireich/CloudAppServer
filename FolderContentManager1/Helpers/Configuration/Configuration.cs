namespace ContentManager.Helpers.Configuration
{
    public class Configuration : IConfiguration
    {
        public string HomeFolderName { get; } = "home";
        public string HomeFolderPath { get; set; } = "";
        public string BaseFolderPath { get; set; } = "C:\\foldercontentmanager";
        public string BaseFolderName { get; set; } = "";
        public string TemporaryFileFolderName { get; set; } = "tempfiles";
        public int DefaultNumberOfElementToShowOnPage { get; set; } = 20;
    }
}
