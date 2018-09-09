using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using CloudAppServer.ServiceModel;

namespace CloudAppServer
{
    [ServiceContract]
    public interface IFolderContentService
    {
        [OperationContract]
        [WebGet (
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "/FolderContent/name={name}&path={path}")]
        string GetFolderContent(string name, string path);

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "/FolderContent/RequestId")]
        int GetRequestId();

        [OperationContract]
        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/CreateFolder")]
        void CreateNewFolder(FolderContentObj newFolder);

        [OperationContract]
        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/DeleteFolder")]
        void DeleteFolder(FolderContentObj folder);

        [OperationContract]
        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/DeleteFile")]
        void DeleteFile(FolderContentObj file);

        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/Rename")]
        void Rename(FolderContentRenameObj folderContent);

        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/Copy")]
        void Copy(FolderContentCopyObj folderContent);

        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/CreateFile")]
        void CreateFile(CreateFolderContentFileObj folderContent);

        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/UpdateFileContent")]
        void UpdateFileContent(CreateFolderContentFileObj folderContent);

        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/FinishUploadFileContent")]
        void FinishUploadFileContent(int requestId);

        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/Cancel")]
        void Cancel(int requestId);

        [OperationContract]
        [WebGet(
            UriTemplate = "/FolderContent/GetFile/name={name}&path={path}")]
        Stream GetFile(string name, string path);
    }
}
