using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentManager.Model;

namespace FolderContentManager.Services
{
    public interface IFolderContentService
    {
        void CreateFolder(string name, string path);

        void DeleteFolder(string name, string path, int page);

        void DeleteFile(string name, string path, int page);

        Stream GetFile(string name, string path);

        IFolderPage GetFolderPage(string name, string path, int page);

        int GetNumOfFolderPages(string name, string path, bool searchMode);

        void Rename(string name, string path, string typeStr, string newName);

        IFolderContent[] Search(string name, int page);

        int GetSortType(string name, string path);

        void CreateFile(string name, string path, string fileType, string tmpCreationPath, long size);

        void Copy(string copyObjName, string copyObjPath, string copyObjTypeStr, string copyToName, string copyToPath);

        void UpdateFolderMetaData(FolderMetadata folderMetadata);

        int GetNumberOfElementOnPage(string name, string path, bool searchMode);
    }
}
