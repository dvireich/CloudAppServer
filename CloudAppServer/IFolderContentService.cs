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

        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/FolderContent/Rename")]
        void Rename(FolderContentObj folderContent);
    }
}
