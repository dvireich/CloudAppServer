using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using CloudAppServer.ServiceModel;
using FolderContentManager.Model;
using FolderMetadata = CloudAppServer.ServiceModel.FolderMetadata;

namespace CloudAppServer
{
    
    [ServiceContract]
    public interface IFolderContentService
    {
        [OperationContract]
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*")]
        void GetOptions();

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "/FolderContent/ping")]
        bool Ping();

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "/FolderContent/Logout")]
        void Logout();

        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/GetPage")]
        string GetFolderContent(PageRequest pageRequest);

        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/UpdateFolderMetadata")]
        void UpdateFolderMetaData(FolderMetadata folderMetadata);

        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/GetNumberOfPages")]
        int GetNumberOfPage(NumberOfPageRequest numberOfPageRequest);

        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/GetSortType")]
        int GetSortType(FolderContentObj folderContent);

        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/GetNumberOfElementsOnPage")]
        int GetNumberOfElementsOnPage(FolderContentObj folderContent);

        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/Search")]
        string Search(SearchRequest searchRequest);

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "/FolderContent/RequestId")]
        int GetRequestId();

        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/CreateFolder")]
        void CreateNewFolder(FolderContentObj newFolder);

        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/DeleteFolder")]
        void DeleteFolder(FolderContentObj folder);

        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/DeleteFile")]
        void DeleteFile(FolderContentObj file);

        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/Rename")]
        void Rename(FolderContentRenameObj folderContent);

        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/Copy")]
        void Copy(FolderContentCopyObj folderContent);

        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/CreateFile")]
        void CreateFile(FolderContentFileObj folderContent);

        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/UpdateFileContent")]
        void UpdateFileContent(FolderContentFileObj folderContent);

        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/ClearUpload")]
        void ClearUpload(int requestId);

        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/CancelUpload")]
        void CancelUpload(int requestId);

        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/GetFileRequestId")]
        int GetFileRequestId(GetFileRequest getFileRequest);

        [OperationContract]
        [WebGet(
            UriTemplate = "/FolderContent/GetFile/requestId={requestId}")]
        Stream GetFile(string requestId);
    }
}
