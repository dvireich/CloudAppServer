using CloudAppServer.Model;
using FolderContentManager.Model;

namespace FolderContentManager.Services
{
    public interface IFolderContentFolderService
    {
        IFolder GetParentFolder(IFolderContent folder);

        IFolder GetFolder(string name, string path);

        void CreateFolder(string name, string path);

        void DeleteFolder(string name, string path, int page);

        void RenameDirectory(string path, string oldName, string newName);

        void UpdateFolderChildrenPath(IFolderContent folderContent, string newPathPrefix, string oldPathPrefix);

        void UpdateFolderMetaData(FolderMetadata folderMetadata);

        void UpdateNextPageToWrite(IFolder folder);

        void CreateDirectory(string name, string path);

        void UpdateFolder(IFolder folder);

        void RenameFolder(string name, string newName, string path);

        void Copy(string copyFromName, string copyFromPath, string copyToName, string copyToPath);
    }
}
