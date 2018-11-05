namespace FolderContentManager.Helpers
{
    public interface IDirectoryManager
    {
        void Delete(string path, bool recursive);

        void CreateDirectory(string path);

        bool Exists(string path);

        void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs);
    }
}
