using System.IO;

namespace FolderContentManager.Helpers
{
    public interface IFileManager
    {
        Stream GetFile(string path);

        void Delete(string path);

        void Move(string fromPath, string toPath);

        void Copy(string fromPath, string toPath);

        bool Exists(string path);

        Stream Create(string path);

        StreamReader OpenText(string path);

        StreamWriter CreateText(string path);

        string GetFileExtension(string fileName);
    }
}
