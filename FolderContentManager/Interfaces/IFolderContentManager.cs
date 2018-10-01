using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentHelper.Model;

namespace FolderContentHelper.Interfaces
{
    public interface IFolderContentManager
    {
        void CreateFolder(string name, string path);

        void DeleteFolder(string name, string path, int page);

        void Rename(string name, string path, string typeStr, string newName);

        void Copy(string copyObjName, string copyObjPath, string copyObjTypeStr, string copyToName,
            string copyToPath);

        void CreateFile(string name, string path, string fileType, string tmpCreationPath, long size);

        Stream GetFile(string name, string path);

        void DeleteFile(string name, string path, int page);

        int GetNumOfFolderPages(string name, string path);

        IFolderPage GetFolderPage(string name, string path, int page);

        IFolderContent[] Search(string name, int page);
    }
}
