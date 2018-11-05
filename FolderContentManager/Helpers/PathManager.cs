using Path = Pri.LongPath.Path;
namespace FolderContentManager.Helpers
{
    public class PathManager : IPathManager
    {
        public string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public string GetTempPath()
        {
            return Path.GetTempPath();
        }

        public string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName);
        }
    }
}
