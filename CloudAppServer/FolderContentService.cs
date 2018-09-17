using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Web.Script.Serialization;
using CloudAppServer.Model;
using CloudAppServer.ServiceModel;
using FolderContentManager;
using FolderContentManager.Interfaces;

namespace CloudAppServer
{
    public class FolderContentService : IFolderContentService
    {
        private readonly IFolderContentManager _folderContentManager;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly IFileService _fileService;

        public FolderContentService()
        {
            _fileService = FileService.Instance;
            _folderContentManager = FolderContentManager.FolderContentManager.Instance;
        }

        private T Perform<T>(Func<T> task)
        {
            try
            {
                return task();
            }
            catch(Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        private void Perform(Action task)
        {
            try
            {
                task();
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        private string FixPath(string path)
        {
            return path.Replace(',', '/');
        }

        private void FixNameAndPath(string name, string path, out string fixedNamed, out string fixedPath)
        {
            fixedNamed = name.Replace("\"", "");
            fixedPath = path.Replace("\"", "");
        }

        public string GetFolderContent(string name, string path, string page)
        {
            return Perform(() =>
            {
                FixNameAndPath(name, path, out var fixedName, out var fixedPath);
                var folderPage = _folderContentManager.GetFolderPage(fixedName, FixPath(fixedPath), int.Parse(page));
                return folderPage == null ? null : _serializer.Serialize(folderPage);
            });
        }

        public int GetNumOfFolderPages(string name, string path)
        {
            return Perform(() =>
            {
                FixNameAndPath(name, path, out var fixedName, out var fixedPath);
                return _folderContentManager.GetNumOfFolderPages(fixedName, FixPath(fixedPath));
            });
        }

        public int GetRequestId()
        {
            return Perform(() => _fileService.GetRequestId());
        }

        public void CreateNewFolder(FolderContentObj newFolder)
        {
            Perform(() =>
            {
                if (newFolder == null) return;
                _folderContentManager.CreateFolder(newFolder.Name, FixPath(newFolder.Path));
            });
        }

        public void DeleteFolder(FolderContentObj folder)
        {
            Perform(() =>
            {
                if (folder == null) return;
                _folderContentManager.DeleteFolder(folder.Name, FixPath(folder.Path), folder.Page);
            });
        }

        public void DeleteFile(FolderContentObj file)
        {
            Perform(() =>
            {
                if (file == null) return;
                _folderContentManager.DeleteFile(file.Name, FixPath(file.Path), file.Page);
            });
        }


        public void Rename(FolderContentRenameObj folderContent)
        {
            Perform(() =>
            {
                if (folderContent == null) return;
                _folderContentManager.Rename(folderContent.Name, FixPath(folderContent.Path), folderContent.Type,
                    folderContent.NewName);
            });
        }

        public void Copy(FolderContentCopyObj folderContent)
        {
            Perform(() =>
            {
                if (folderContent == null) return;
                _folderContentManager.Copy(folderContent.FolderContentName, folderContent.FolderContentPath,
                    folderContent.FolderContentType,
                    folderContent.CopyToName, folderContent.CopyToPath);
            });
        }

        public void CreateFile(CreateFolderContentFileObj folderContent)
        {
            Perform(() =>
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
                    values,
                    folderContent.Size));
            });
        }

        public void UpdateFileContent(CreateFolderContentFileObj folderContent)
        {
            Perform(() =>
            {
                if (folderContent == null) return;

                _fileService.UpdateFileValue(folderContent.RequestId, folderContent.NewValueIndex,
                    folderContent.NewValue);

                if (!_fileService.IsFileFullyUploaded(folderContent.RequestId)) return;

                var file = _fileService.GetFile(folderContent.RequestId);
                if (file == null) return;

                _folderContentManager.CreateFile(file.Name, file.Path, file.FileType, file.Value, file.Size);
            });
        }

        public void ClearUpload(int requestId)
        {
            Perform(() =>
            {
                _fileService.Finish(requestId); 

            });
        }

        public Stream GetFile(string name, string path)
        {
            return Perform(() =>
            {
                FixNameAndPath(name, path, out var fixedName, out var fixedPath);
                var file = _folderContentManager.GetFile(fixedName, FixPath(fixedPath));
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Disposition",
                    "attachment; filename=" + fixedName);
                WebOperationContext.Current.OutgoingResponse.ContentLength = file.Length;

                return file;
            });
        }
    }
}
