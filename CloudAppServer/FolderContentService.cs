using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
using FolderMetadata = CloudAppServer.ServiceModel.FolderMetadata;

namespace CloudAppServer
{
    public class FolderContentService : IFolderContentService
    {
        private readonly IFolderContentManager _folderContentManager;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly IFileService _fileService;

        public FolderContentService()
        {
            var endpoint = OperationContext.Current.EndpointDispatcher.EndpointAddress.ToString();
            var userId = endpoint.Split('/').Last();
            _fileService = FolderContentManagerToClient.Instance.GetFileService(userId);
            _folderContentManager = FolderContentManagerToClient.Instance.GetFolderContentManager(userId);
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

        public void GetOptions()
        {
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

        public void UpdateFolderMetaData(FolderMetadata folderMetadata)
        {
            if (folderMetadata == null) return;
            Perform(() =>
            {
                _folderContentManager.UpdateFolderMetaData(new FolderContentManager.Model.FolderMetadata()
                {
                    Name = folderMetadata.Name,
                    Path = folderMetadata.Path,
                    SortType = folderMetadata.SortType
                });
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

        public int GetSortType(FolderContentObj folderContent)
        {
            if (folderContent == null) return 0;
            return Perform(() =>
            {
                FixNameAndPath(folderContent.Name, folderContent.Path, out var fixedName, out var fixedPath);
                var folder = _folderContentManager.GetFolderObj(fixedName, fixedPath);
                return (int)folder.SortType;
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

                _fileService.CreateFile(folderContent.RequestId, new TmpFileObj(folderContent.Name,
                    folderContent.Path,
                    folderContent.FileType,
                    folderContent.Size));
            });
        }

        public void UpdateFileContent(CreateFolderContentFileObj folderContent)
        {
            Perform(() =>
            {
                if (folderContent == null) return;

                _fileService.UpdateFileValue(folderContent.RequestId,folderContent.NewValue, folderContent.Sent, folderContent.Size);

                if (folderContent.Sent < folderContent.Size) return;
                var file = _fileService.GetFile(folderContent.RequestId);
                if (file == null) return;

                _folderContentManager.CreateFile(file.Name, file.Path, file.FileType, file.TmpCreationPath, file.Size);
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
        public void CancelUpload(int requestId)
        {
            Perform(() =>
            {
                _fileService.Cancel(requestId);

            });
        }

        [WcfLogging]
        public int GetFileRequestId(GetFileRequest getFileRequest)
        {
            if (getFileRequest == null) return -1;
            return Perform(() =>
            {
                FixNameAndPath(getFileRequest.Name, getFileRequest.Path, out var fixedName, out var fixedPath);
                var requestId = _fileService.GetRequestIdForDownload();
                var downloadData = new FileDownloadData()
                {
                    FileName = fixedName,
                    FilePath = FixPath(fixedPath)
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
            var fileStream = _folderContentManager.GetFile(downloadData.FileName, downloadData.FilePath);
            if (WebOperationContext.Current != null)
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Disposition",
                    "attachment; filename=" + downloadData.FileName);
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";
                WebOperationContext.Current.OutgoingResponse.ContentLength = fileStream.Length;
            }

            OperationContext clientContext = OperationContext.Current;
            clientContext.OperationCompleted += delegate
            {
                fileStream?.Dispose();
            };
            return fileStream;
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
