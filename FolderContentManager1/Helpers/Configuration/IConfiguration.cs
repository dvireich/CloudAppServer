namespace ContentManager.Helpers.Configuration
{
    public interface IConfiguration
    {
        string HomeFolderName { get; }

        string HomeFolderPath { get; }

        string BaseFolderPath { get; set; }

        string BaseFolderName { get; set; }

        string TemporaryFileFolderName { get; set; }

        int DefaultNumberOfElementToShowOnPage { get; set; }
    }
}
