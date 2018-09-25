using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentHelper
{
    public interface IFileService
    {
        void CreateFile(int requestId, IFile file);

        void UpdateFileValue(int requestId, int index, string value);

        IFile GetFile(int requestId);

        void Finish(int requestId);

        int GetRequestId();

        bool IsFileFullyUploaded(int requestId);
    }
}
