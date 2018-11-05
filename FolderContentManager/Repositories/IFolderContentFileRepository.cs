using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentManager.Repositories
{
    interface IFolderContentFileRepository
    {
        IFile GetFolderContentFile(string name, string path);

        void CreateOrUpdateFolderContentFile(string name, string path, IFile file);

        Stream GetFile(string name, string path);

        void Delete(string name, string path);

        void Move(string moveToName, string moveToPath, string moveFromFullPath);

        void Move(string moveToName, string moveToPath, string moveFromName, string moveFromPath);

        void Copy(string copyToName, string copyToPath, string copyFromName, string copyFromPath);
    }
}
