using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web.Script.Serialization;
using CloudAppServer.Model;
using CloudAppServer.ServiceModel;
using FolderContentHelper;
using FolderContentHelper.Interfaces;
using FolderContentHelper.Model;
using FolderContentManager.Model;
using WcfLogger;

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
            var endpoint = OperationContext.Current.EndpointDispatcher.EndpointAddress.ToString();
            var userId = endpoint.Split('/').Last();
            _folderContentManager = FolderContentManagerToClient.Instance.GetClient(userId);
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

        [WcfLogging]
        public bool Ping()
        {
            return true;
        }

        [WcfLogging]
        public void Logout()
        {
            var endpoint = OperationContext.Current.EndpointDispatcher.EndpointAddress.ToString();
            var userId = endpoint.Split('/').Last();
            FolderContentManagerToClient.Instance.RemoveClient(userId);
        }

        [WcfLogging]
        public string GetFolderContent(PageRequest pageRequest)
        {
            if (pageRequest == null) return string.Empty;
            return Perform(() =>
            {
                FixNameAndPath(pageRequest.Name, pageRequest.Path, out var fixedName, out var fixedPath);
                var folderPage = _folderContentManager.GetFolderPage(fixedName, FixPath(fixedPath), int.Parse(pageRequest.Page));
                return folderPage == null ? null : _serializer.Serialize(folderPage);
            });
        }

        [WcfLogging]
        public int GetNumberOfPage(NumberOfPageRequest numberOfPageRequest)
        {
            if (numberOfPageRequest == null) return -1;
            return Perform(() =>
            {
                FixNameAndPath(numberOfPageRequest.Name, numberOfPageRequest.Path, out var fixedName, out var fixedPath);
                return _folderContentManager.GetNumOfFolderPages(fixedName, FixPath(fixedPath));
            });
        }

        [WcfLogging]
        public int GetRequestId()
        {
            return Perform(() => _fileService.GetRequestId());
        }

        [WcfLogging]
        public void CreateNewFolder(FolderContentObj newFolder)
        {
            Perform(() =>
            {
                if (newFolder == null) return;
                _folderContentManager.CreateFolder(newFolder.Name, FixPath(newFolder.Path));
            });
        }

        [WcfLogging]
        public void DeleteFolder(FolderContentObj folder)
        {
            Perform(() =>
            {
                if (folder == null) return;
                _folderContentManager.DeleteFolder(folder.Name, FixPath(folder.Path), folder.Page);
            });
        }

        [WcfLogging]
        public void DeleteFile(FolderContentObj file)
        {
            Perform(() =>
            {
                if (file == null) return;
                _folderContentManager.DeleteFile(file.Name, FixPath(file.Path), file.Page);
            });
        }

        [WcfLogging]
        public void Rename(FolderContentRenameObj folderContent)
        {
            Perform(() =>
            {
                if (folderContent == null) return;
                _folderContentManager.Rename(folderContent.Name, FixPath(folderContent.Path), folderContent.Type,
                    folderContent.NewName);
            });
        }

        [WcfLogging]
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

        [WcfLogging]
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

        [WcfLogging]
        public void ClearUpload(int requestId)
        {
            Perform(() =>
            {
                _fileService.Finish(requestId); 

            });
        }

        [WcfLogging]
        public int GetFileRequestId(GetFileRequest getFileRequest)
        {
            if (getFileRequest == null) return -1;
            return Perform(() =>
            {
                FixNameAndPath(getFileRequest.Name, getFileRequest.Path, out var fixedName, out var fixedPath);
                var file = _folderContentManager.GetFile(fixedName, FixPath(fixedPath));
                var requestId = _fileService.GetRequestIdForDownload();
                var downloadData = new FileDownloadData()
                {
                    FileName = fixedName,
                    FileStream = file
                };
                _fileService.PrepareFileToDownload(requestId, downloadData);

                return requestId;
            });
        }

        [WcfLogging(LogReturnVal = false)]
        public Stream GetFile(string requestId)
        {
            requestId = requestId.Replace("\"", "");
            var downloadData = _fileService.GetDownloadFileData(int.Parse(requestId));
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Disposition",
                "attachment; filename=" + downloadData.FileName);
            WebOperationContext.Current.OutgoingResponse.ContentLength = downloadData.FileStream.Length;

            return downloadData.FileStream;
        }

        [WcfLogging]
        public string Search(SearchRequest searchRequest)
        {
            if (searchRequest == null) return string.Empty;
            return Perform(() =>
            {
                FixNameAndPath(searchRequest.Name, searchRequest.Page, out var fixedName, out var fixedPage);
                var result = _folderContentManager.Search(fixedName, int.Parse(fixedPage));
                var folderPage = new FolderPageSearchResult(fixedName, result);
                return _serializer.Serialize(folderPage);
            });
        }
    }
}
