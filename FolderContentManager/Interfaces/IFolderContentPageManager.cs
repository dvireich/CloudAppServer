using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentHelper.Model;

namespace FolderContentHelper.Interfaces
{
    public interface IFolderContentPageManager
    {
        void AddToFolderPage(int pageNum, IFolderPage pageToWriteContent, IFolderContent folderContent);

        int GetNextPageToWrite(IFolder folder);

        void CreateNewFolderPage(int pageNum, IFolder folder);

        void PerformPageCompression(IFolder folder);

        void UpdateRenameInParentPages(IFolder parentFolder, IFolderContent folderContent, string oldName);
        void UpdateNewPathOnPage(IFolder folder, int pageNum, string oldPathPrefix, string newPathPrefix);

        void UpdateDeleteFolderContentInParentPages(IFolder parentFolder, IFolderContent folder, int page);

        void ValidateNameInPages(IFolder folderToValidate, IFolderContent newFolderContent);

        void RenameFolderPageInternalNameField(IFolder folder, string newName);

        void MoveAllRootPagesToPath(IFolder folder, string newPath);
    }
}
