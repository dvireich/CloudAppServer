using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Interfaces;
using FolderContentManager.Model;

namespace FolderContentHelper
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
