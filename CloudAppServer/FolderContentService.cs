using System;
using System.IO;
using System.ServiceModel.Web;
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
            _folderContentManager = FolderContentManager.FolderContentManager.Instance;
        }

        private string FixPath(string path)
        {
            return path.Replace(',', '/');
        }

        public string GetFolderContent(string name, string path, string page)
        {
            name = name.Replace("\"", "");
            path = path.Replace("\"", "");
            var folderPage = _folderContentManager.GetFolderPage(name, FixPath(path), int.Parse(page));
            return folderPage == null ? null : _serializer.Serialize(folderPage);
        }

        public int GetNumOfFolderPages(string name, string path)
        {
            name = name.Replace("\"", "");
            path = path.Replace("\"", "");
            var folder = _folderContentManager.GetFolder(name, FixPath(path));
            return folder?.NumOfPages ?? -1;
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
            _folderContentManager.DeleteFolder(folder.Name, FixPath(folder.Path), folder.Page);
        }

        public void DeleteFile(FolderContentObj file)
        {
            if (file == null) return;
            _folderContentManager.DeleteFile(file.Name, FixPath(file.Path), file.Page);
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
            var values = new string[folderContent.NumOfChunks];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = null;
            }
            _fileService.CreateFile(folderContent.RequestId, new FileObj(folderContent.Name, 
                                                                         folderContent.Path, 
                                                                         folderContent.FileType,
                                                                         values));
        }

        public void UpdateFileContent(CreateFolderContentFileObj folderContent)
        {
            if (folderContent == null) return;

            _fileService.UpdateFileValue(folderContent.RequestId, folderContent.NewValueIndex , folderContent.NewValue);

            if (!_fileService.IsFileFullyUploaded(folderContent.RequestId)) return;
            
            var file =_fileService.GetFile(folderContent.RequestId);
            if(file == null) return;

            _folderContentManager.CreateFile(file.Name, file.Path, file.FileType, file.Value);
        }

        public void ClearUpload(int requestId)
        {
            _fileService.Finish(requestId);
        }

        public Stream GetFile(string name, string path)
        {
            name = name.Replace("\"", "");
            path = path.Replace("\"", "");
            var file = _folderContentManager.GetFile(name, FixPath(path));
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Disposition", "attachment; filename=" + name);
            WebOperationContext.Current.OutgoingResponse.ContentLength = file.Length;

            return file;
        }
    }
}
