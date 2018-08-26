using CloudAppServer.ServiceModel;

namespace Test
{
    public class FolderContentService : IFolderContentService
    {
        public string GetFolderContent(string name, string path)
        {
            var folderContentManager = new FolderContentManager.FolderContentManager();
            return folderContentManager.GetFolderAsJson(name, path);
        }

        public void CreateNewFolder(NewFolderObj nameAndPath)
        {


            var folderContentManager = new FolderContentManager.FolderContentManager();
            folderContentManager.CreateFolder(nameAndPath.Name, nameAndPath.Path);
        }
    }
}
