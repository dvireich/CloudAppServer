using System;
using System.Web.Script.Serialization;
using CloudAppServer.Model;
using CloudAppServer.ServiceModel;
using FolderContentManager;

namespace CloudAppServer
{
    public class FolderContentService : IFolderContentService
    {
        private readonly FolderContentManager.FolderContentManager _folderContentManager;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly IFileService _fileService;

        public FolderContentService()
        {
            _fileService = FileService.Instance;
            _folderContentManager = new FolderContentManager.FolderContentManager();
        }

        private string FixPath(string path)
        {
            return path.Replace(',', '/');
        }

        public string GetFolderContent(string name, string path)
        {
            name = name.Replace("\"", "");
            path = path.Replace("\"", "");
            var folderContent = _folderContentManager.GetFolder(name, path);
            return folderContent == null ? null : _serializer.Serialize(folderContent);
        }

        public int GetRequestId()
        {
            return _fileService.GetRequestId();
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

        public void DeleteFile(FolderContentObj file)
        {
            if (file == null) return;
            _folderContentManager.DeleteFile(file.Name, FixPath(file.Path));
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

        public void CreateFile(CreateFolderContentFileObj folderContent)
        {
            if (folderContent == null) return;
            _fileService.CreateFile(folderContent.RequestId, new FileObj(folderContent.Name, 
                                                                         folderContent.Path, 
                                                                         folderContent.FileType,
                                                                         new string[folderContent.NumOfChunks]));
        }

        public void UpdateFileContent(CreateFolderContentFileObj folderContent)
        {
            if (folderContent == null) return;

            _fileService.UpdateFileValue(folderContent.RequestId, folderContent.NewValueIndex , folderContent.NewValue);

            if (!_fileService.IsFileFullyUploaded(folderContent.RequestId)) return;

            var file =_fileService.GetFile(folderContent.RequestId);
            _folderContentManager.CreateFile(file.Name, file.Path, file.FileType, file.Value);
            _fileService.Finish(folderContent.RequestId);
        }
    }
}
