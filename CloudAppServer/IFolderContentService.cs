﻿using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using CloudAppServer.ServiceModel;

namespace CloudAppServer
{
    [ServiceContract]
    public interface IFolderContentService
    {
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
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/GetPage")]
        string GetFolderContent(PageRequest pageRequest);

        [OperationContract]
        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/GetNumberOfPages")]
        int GetNumberOfPage(NumberOfPageRequest numberOfPageRequest);

        [OperationContract]
        [WebInvoke(
            Method = "*",
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
            UriTemplate = "/FolderContent/ClearUpload")]
        void ClearUpload(int requestId);

        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/GetFileRequestId")]
        int GetFileRequestId(GetFileRequest getFileRequest);

        [OperationContract]
        [WebGet(
            UriTemplate = "/FolderContent/GetFile/requestId={requestId}")]
        Stream GetFile(string requestId);
    }
}
