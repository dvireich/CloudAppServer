using CloudAppServer.ServiceModel;

namespace CloudAppServer
{
    public class FolderContentService : IFolderContentService
    {
        private readonly FolderContentManager.FolderContentManager _folderContentManager;

        public FolderContentService()
        {
            _folderContentManager = new FolderContentManager.FolderContentManager();
            ;
        }

        private string FixPath(string path)
        {
            return path.Replace(',', '/');
        }

        public string GetFolderContent(string name, string path)
        {

            return _folderContentManager.GetFolderAsJson(name, FixPath(path));
        }

        public void CreateNewFolder(FolderContentObj newFolder)
        {
            if (newFolder == null) return;
            _folderContentManager.CreateFolder(newFolder.Name, FixPath(newFolder.Path));
        }

        public void DeleteFolder(FolderContentObj folder)
        {
            if (folder == null) return;
            _folderContentManager.DeleteFolder(folder.Name, FixPath(folder.Path));
        }


        public void Rename(FolderContentRenameObj folderContent)
        {
            if (folderContent == null) return;
            _folderContentManager.Rename(folderContent.Name, FixPath(folderContent.Path), folderContent.Type, folderContent.NewName);
        }

        public void Copy(FolderContentCopyObj folderContent)
        {
            if (folderContent == null) return;
            _folderContentManager.Copy(folderContent.FolderContentName, folderContent.FolderContentPath, folderContent.FolderContentType,
                folderContent.CopyToName, folderContent.CopyToPath);
        }
    }
}
