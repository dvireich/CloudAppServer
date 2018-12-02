using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentManager.Model;

namespace FolderContentManager.Services
{
    public interface IFolderContentPageService
    {
        void ValidateUniquenessOnAllFolderPages(IFolder folderToValidateIn, IFolderContent folderContentToValidate);

        void AddNewFolderPageToFolder(int pageNum, IFolder folder);

        int GetNextAvailablePageToWrite(IFolder folder);

        void AddToFolderPage(IFolder folder, int pageNum, IFolderContent folderContent);

        IFolderPage GetFolderPage(IFolder folder, int pageNum);

        void DeletePage(IFolder folder, int pageNum);

        void RemoveFolderContentFromPage(IFolder folder, IFolderContent folderContentToRemove, int page);

        void UpdatePathOnPage(IFolder folder, int pageNum, string oldPathPrefix, string newPathPrefix);

        void MovePagesToNewLocation(IFolder folder, string sourceName, string sourcePath, string destName, string destPath);

        void RenameFolderContentInFolderPages(IFolder folder, string oldName, string newName, IFolderContent renamedObject);

        void CopyPagesToNewLocation(IFolder folder, string sourceName, string sourcePath, string destName, string destPath);

        int GetNumberOfPages(IFolder folder);

    }
}
