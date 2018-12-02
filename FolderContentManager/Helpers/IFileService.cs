using FolderContentManager.Model;

namespace FolderContentManager.Helpers
{
    public interface IFileService
    {
        void CreateFile(int requestId, ITmpFile file);

        void UpdateFileValue(int requestId, string value, long sent, long size);

        ITmpFile GetFile(int requestId);

        void Finish(int requestId);

        int GetRequestId();

        int GetRequestIdForDownload();

        void PrepareFileToDownload(int requestId, FileDownloadData fileDownloadData);

        FileDownloadData GetDownloadFileData(int requestId);

        void Cancel(int requestId);
    }
}
