using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CloudAppServer.ServiceModel;
using ContentManager;
using ContentManager.Helpers.Result;
using ContentManager.Managers;
using ContentManager.Model.Enums;
using WcfLogger;
using FolderMetadata = CloudAppServer.ServiceModel.FolderMetadata;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace CloudAppServer
{
    public class FolderContentService : IFolderContentService
    {
        private readonly FolderContentManager _folderContentManager;
        private readonly FolderContentSearchManager _folderContentSearchManager;
        private readonly UploadFileManager _uploadFileManager;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public FolderContentService()
        {
            var endpoint = OperationContext.Current.EndpointDispatcher.EndpointAddress.ToString();
            var userId = endpoint.Split('/').Last();
            var configuration  = ClientConfiguration.Instance.GetConfiguration(userId);
            _folderContentManager = new FolderContentManager(configuration);
            _folderContentSearchManager = new FolderContentSearchManager(configuration);
            _uploadFileManager = new UploadFileManager(configuration);
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
            ClientConfiguration.Instance.RemoveClient(userId);
        }

        [WcfLogging]
        public string GetPage(PageRequest pageRequest)
        {
            if (pageRequest == null) return string.Empty;

            var getFolderPageTask = _folderContentManager.GetFolderPageAsync(
                pageRequest.Name, 
                pageRequest.Path, 
                int.Parse(pageRequest.Page));

            Task.WaitAll(getFolderPageTask);
            var result = getFolderPageTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }

            return _serializer.Serialize(result.Data);
        }

        [WcfLogging]
        public void UpdateFolderMetaData(FolderMetadata folderMetadata)
        {
            if (folderMetadata == null) return;

            var updateResultTask = _folderContentManager.UpdateFolderMetaData(new ContentManager.Model.FolderMetadata()
            {
                Name = folderMetadata.Name,
                Path = folderMetadata.Path,
                SortType = folderMetadata.SortType,
                NumberOfPagesPerPage = folderMetadata.NumberOfPagesPerPage
            });

            Task.WaitAll(updateResultTask);
            var result = updateResultTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging]
        public long GetNumberOfPage(NumberOfPageRequest numberOfPageRequest)
        {
            if (numberOfPageRequest == null) return -1;

            Task<IResult<long>> numberOfPageTask;

            if (numberOfPageRequest.SearchMode)
            {
                numberOfPageTask = _folderContentSearchManager.GetNumOfFolderPagesAsync(
                    numberOfPageRequest.Name,
                    numberOfPageRequest.Path);
            }
            else
            {
                numberOfPageTask = _folderContentManager.GetNumOfFolderPagesAsync(
                    numberOfPageRequest.Name,
                    numberOfPageRequest.Path);
            }

            Task.WaitAll(numberOfPageTask);
            var result = numberOfPageTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }

            return result.Data;
        }

        [WcfLogging]
        public int GetSortType(FolderContentObj folderContent)
        {
            if (folderContent == null) return 0;

            var sortTypeResult = _folderContentManager.GetSortType(folderContent.Name, folderContent.Path);

            if (!sortTypeResult.IsSuccess)
            {
                throw new WebFaultException<string>(sortTypeResult.Exception.ToString(), HttpStatusCode.InternalServerError);
            }

            return (int)sortTypeResult.Data;
        }

        [WcfLogging]
        public int GetNumberOfElementsOnPage(FolderContentObj folderContent)
        {
            if (folderContent == null) return 0;

            var numberOfElementsToShowOnPageResult = _folderContentManager.GetNumberOfElementToShowOnPage(folderContent.Name, folderContent.Path);

            if (!numberOfElementsToShowOnPageResult.IsSuccess)
            {
                throw new WebFaultException<string>(numberOfElementsToShowOnPageResult.Exception.ToString(), HttpStatusCode.InternalServerError);
            }

            return numberOfElementsToShowOnPageResult.Data;
        }

        [WcfLogging]
        public string GetRequestId()
        {
            return Guid.NewGuid().ToString();
        }

        [WcfLogging]
        public void CreateNewFolder(FolderContentObj newFolder)
        {
            if (newFolder == null) return;

            var createFolderTask = _folderContentManager.CreateFolderAsync(newFolder.Name, newFolder.Path);
            Task.WaitAll(createFolderTask);

            var result = createFolderTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging]
        public void DeleteFolder(FolderContentObj folder)
        {
            if(folder == null) return;
            
            var deleteFolderTask = _folderContentManager.DeleteFolderAsync(folder.Name, folder.Path);
            Task.WaitAll(deleteFolderTask);

            var result = deleteFolderTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging]
        public void DeleteFile(FolderContentObj file)
        {
            if (file == null) return;

            var deleteFileTask = _folderContentManager.DeleteFileAsync(file.Name, file.Path);
            Task.WaitAll(deleteFileTask);

            var result = deleteFileTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging]
        public void Rename(FolderContentRenameObj folderContent)
        {
            if (folderContent == null) return;

            Task<IResult<Void>> renameTask = null;
            Enum.TryParse(folderContent.Type, true, out FolderContentType type);

            if (type == FolderContentType.File)
            {
                renameTask = _folderContentManager.RenameFileAsync(folderContent.Name, folderContent.Path, folderContent.NewName);
            }

            if (type == FolderContentType.Folder)
            {
                renameTask = _folderContentManager.RenameFolderAsync(folderContent.Name, folderContent.Path, folderContent.NewName);
            }

            Task.WaitAll(renameTask ?? throw new WebFaultException<string>("Rename argument 'type' must be File of Folder", HttpStatusCode.Forbidden));

            var result = renameTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging]
        public void Copy(FolderContentCopyObj folderContent)
        {
            if (folderContent == null) return;

            Enum.TryParse(folderContent.FolderContentType, true, out FolderContentType type);
            Task<IResult<Void>> renameTask = null;

            if (type == FolderContentType.File)
            {
                renameTask = _folderContentManager.CopyFileAsync(
                    folderContent.FolderContentPath, 
                    folderContent.FolderContentName, 
                    folderContent.CopyToPath,
                    folderContent.CopyToName);
            }

            if (type == FolderContentType.Folder)
            {
                renameTask = _folderContentManager.CopyFolderAsync(
                    folderContent.FolderContentPath, 
                    folderContent.FolderContentName, 
                    folderContent.CopyToPath,
                    folderContent.CopyToName);
            }

            Task.WaitAll(renameTask ?? throw new WebFaultException<string>("copy argument 'type' must be File of Folder", HttpStatusCode.Forbidden));

            var result = renameTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging]
        public void CreateFile(FolderContentFileObj folderContent)
        {
            if (folderContent == null) return;

            var createFileTask = _uploadFileManager.CreateUploadFileAsync(folderContent.Name, folderContent.Path);

            Task.WaitAll(createFileTask);

            var result = createFileTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging(LogArguments = false)]
        public void UpdateFileContent(FolderContentFileObj folderContent)
        {
            if (folderContent == null) return;

            var updateFileResult =
                _uploadFileManager.UpdateFileContentAsync(folderContent.Name, folderContent.Path, folderContent.NewValue);

            Task.WaitAll(updateFileResult);

            var result = updateFileResult.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging]
        public void FinishUpload(FolderContentFileObj folderContent)
        {
            if (folderContent == null) return;

            var finishUploadTask = _uploadFileManager.FinishUploadAsync(folderContent.Name, folderContent.Path);

            Task.WaitAll(finishUploadTask);

            var result = finishUploadTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging]
        public void ClearUpload(FolderContentFileObj folderContent)
        {
            if (folderContent == null) return;

            var cancelUploadTask = _uploadFileManager.CancelUploadAsync(folderContent.Name, folderContent.Path);

            Task.WaitAll(cancelUploadTask);
        }

        [WcfLogging]
        public void CancelUpload(FolderContentFileObj folderContent)
        {
            if (folderContent == null) return;

            var cancelUploadTask = _uploadFileManager.CancelUploadAsync(folderContent.Name, folderContent.Path);

            Task.WaitAll(cancelUploadTask);

            var result = cancelUploadTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        [WcfLogging(LogReturnVal = false)]
        public Stream GetFile(FolderContentFileObj folderContent)
        {
            var getStreamTask = _folderContentManager.GetFileStreamAsync(folderContent.Name, folderContent.Path);

            Task.WaitAll(getStreamTask);

            var result = getStreamTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }

            if (WebOperationContext.Current != null)
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Disposition",
                    "attachment; filename=" + folderContent.Name);
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";
                WebOperationContext.Current.OutgoingResponse.ContentLength = result.Data.Length;
            }

            OperationContext clientContext = OperationContext.Current;

            clientContext.OperationCompleted += delegate
            {
                result.Data?.Dispose();
            };

            return result.Data;
        }

        [WcfLogging]
        public string Search(SearchRequest searchRequest)
        {
            if (searchRequest == null) return string.Empty;

            int.TryParse(searchRequest.Page, out int page);
            var searchTask = _folderContentSearchManager.Search(searchRequest.Name, page);
            Task.WaitAll(searchTask);

            var result = searchTask.Result;

            if (!result.IsSuccess)
            {
                throw new WebFaultException<string>(result.Exception.ToString(), HttpStatusCode.InternalServerError);
            }

            return _serializer.Serialize(result.Data);
        }
    }
}
