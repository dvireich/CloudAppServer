namespace FolderContentManager.Helpers
{
    public interface IPathManager
    {
        string Combine(params string[] paths);

        string GetTempPath();

        string GetFileExtension(string fileName);
    }
}
