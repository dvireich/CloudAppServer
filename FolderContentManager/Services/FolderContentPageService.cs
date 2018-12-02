using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentManager.Helpers;
using FolderContentManager.Model;
using FolderContentManager.Repositories;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentManager.Services
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class FolderContentPageService : IFolderContentPageService
    {
        private readonly IConstance _constance;
        private readonly IFolderContentPageRepository _folderContentPageRepository;
        public FolderContentPageService(IConstance constance)
        {
            _constance = constance;
            _folderContentPageRepository = new FolderContentPageRepository(constance);
        }

        public void ValidateUniquenessOnAllFolderPages(IFolder folderToValidateIn, IFolderContent folderContentToValidate)
        {
            if (folderToValidateIn == null) return;

            for (var i = 1; i <= folderToValidateIn.NumOfPhysicalPages; i++)
            {
                var page = _folderContentPageRepository.GetFolderPage(folderToValidateIn.Name, folderToValidateIn.Path, i);
                if (page.Content.Any(f => f.Name == folderContentToValidate.Name && f.Type == folderContentToValidate.Type))
                {
                    throw new Exception("The name already exists in this folder!");
                }
            }
        }

        public void AddNewFolderPageToFolder(int pageNum, IFolder folder)
        {
            IFolderPage page = new FolderPage(folder.Name, folder.Path, new IFolderContent[0]);
            _folderContentPageRepository.CreateOrUpdateFolderPage(folder.Name, folder.Path, pageNum, page);
        }

        public int GetNextAvailablePageToWrite(IFolder folder)
        {
            for (var i = 1; i <= folder.NumOfPhysicalPages; i++)
            {
                var page = _folderContentPageRepository.GetFolderPage(folder.Name, folder.Path, i);
                if (page.Content.Length < _constance.MaxFolderContentOnPage) return i;
            }

            return folder.NumOfPhysicalPages + 1;
        }

        public void AddToFolderPage(IFolder folder, int pageNum, IFolderContent folderContent)
        {
            var page = _folderContentPageRepository.GetFolderPage(folder.Name, folder.Path, pageNum);
            var contentList = page.Content.ToList();
            contentList.Add(folderContent);
            page.Content = contentList.ToArray();
            _folderContentPageRepository.CreateOrUpdateFolderPage(folder.Name, folder.Path, pageNum, page);
        }

        public IFolderPage GetFolderPage(IFolder folder, int pageNum)
        {
            var allPagesData = GetAllPagesOfFolder(folder).ToList();
            allPagesData.Sort((folderContent1, folderContent2) => CompareFolderContent(folderContent1, folderContent1, folder.SortType));
            var folderPageContent = allPagesData.Skip(folder.NumberOfElementPerPage * pageNum).Take(folder.NumberOfElementPerPage);
            return new FolderPage(folder.Name, folder.Path, folderPageContent.ToArray());
        }

        public void DeletePage(IFolder folder, int pageNum)
        {
            _folderContentPageRepository.DeletePage(folder.Name, folder.Path, pageNum);
        }

        public void RemoveFolderContentFromPage(IFolder folder, IFolderContent folderContentToRemove, int page)
        {
            var folderPage = GetFolderPage(folder, page);

            if (folderPage.Content == null) return;

            var contentList = folderPage.Content.ToList();
            contentList.RemoveAll(fc => fc.Name == folderContentToRemove.Name &&
                                        fc.Path == folderContentToRemove.Path &&
                                        fc.Type == folderContentToRemove.Type);

            folderPage.Content = contentList.ToArray();
            _folderContentPageRepository.CreateOrUpdateFolderPage(folder.Name, folder.Path, page, folderPage);

            PerformPageCompression(folder);
        }

        public void UpdatePathOnPage(IFolder folder, int pageNum, string oldPathPrefix, string newPathPrefix)
        {
            var page = GetFolderPage(folder, pageNum);

            if (page.Path.StartsWith(oldPathPrefix))
            {
                page.Path = page.Path.ReplacePrefixInString(oldPathPrefix, newPathPrefix);
            }
            foreach (var fc in page.Content)
            {
                if (!fc.Path.StartsWith(oldPathPrefix)) continue;
                fc.Path = fc.Path.ReplacePrefixInString(oldPathPrefix, newPathPrefix);
            }
            _folderContentPageRepository.CreateOrUpdateFolderPage(page.Name, page.Path, pageNum, page);
        }

        public void MovePagesToNewLocation(IFolder folder, string sourceName, string sourcePath, string destName, string destPath)
        {
            for (var i = 1; i <= folder.NumOfPhysicalPages; i++)
            {
                var sourcePage = _folderContentPageRepository.GetFolderPage(sourceName, sourcePath, i);
                sourcePage.Path = destPath;
                sourcePage.Name = destName;
                _folderContentPageRepository.CreateOrUpdateFolderPage(destName, destPath, i, sourcePage);
                _folderContentPageRepository.DeletePage(sourceName, sourcePath, i);
            }
        }

        public void CopyPagesToNewLocation(IFolder folder, string sourceName, string sourcePath, string destName, string destPath)
        {
            for (var i = 1; i <= folder.NumOfPhysicalPages; i++)
            {
                var sourcePage = _folderContentPageRepository.GetFolderPage(sourceName, sourcePath, i);
                sourcePage.Path = destPath;
                sourcePage.Name = destName;
                _folderContentPageRepository.CreateOrUpdateFolderPage(destName, destPath, i, sourcePage);
            }
        }

        public void RenameFolderContentInFolderPages(IFolder folder, string oldName, string newName, IFolderContent renamedObject)
        {
            for (var i = 1; i <= folder.NumOfPhysicalPages; i++)
            {
                var page = GetFolderPage(folder, i);
                if (page.Content == null) continue;

                var contentList = page.Content.ToList();
                var childContent = contentList.FirstOrDefault(fc => fc.Name == oldName &&
                                                                    fc.Path == renamedObject.Path &&
                                                                    fc.Type == renamedObject.Type);
                if (childContent == null) continue;

                childContent.Name = newName;
                childContent.ModificationTime = $"{DateTime.Now:G}";
                page.Content = contentList.ToArray();
                _folderContentPageRepository.CreateOrUpdateFolderPage(folder.Name, folder.Path, i, page);
            }
        }

        public int GetNumberOfPages(IFolder folder)
        {
            var numberOfElementsPerPage = folder.NumberOfElementPerPage > 0 ? folder.NumberOfElementPerPage : _constance.DefaultNumberOfElementOnPage;
            var numOfElements = (folder.NumOfPhysicalPages - 1) * _constance.MaxFolderContentOnPage;
            var lastFolderPage = GetFolderPage(folder, folder.NumOfPhysicalPages);
            if (lastFolderPage != null)
            {
                numOfElements += lastFolderPage.Content.Length;
            }

            var numOfPages = (int)Math.Ceiling((double)numOfElements / numberOfElementsPerPage);
            return numOfPages == 0 ? 1 : numOfPages;

        }

        private void PerformPageCompression(IFolder folder)
        {
            for (var i = 1; i <= folder.NumOfPhysicalPages; i++)
            {
                var page = GetFolderPage(folder, i);
                if (page.Content.Length == _constance.MaxFolderContentOnPage) continue;

                //Delete empty pages, but do not delete the first page
                if (page.Content.Length == 0 && i != 1)
                {
                    DeletePage(folder, i);
                    folder.NumOfPhysicalPages--;
                    continue;
                }

                //This is the last page and it is not full -> its ok
                if (i == folder.NumOfPhysicalPages) continue;

                var nextPage = GetFolderPage(folder, i + 1);
                var numOfElementsToMove = _constance.MaxFolderContentOnPage - page.Content.Length;

                //Calculate the elements to move 
                var elementToMove = new HashSet<IFolderContent>();
                for (var j = 0; j < numOfElementsToMove && j < nextPage.Content.Length; j++)
                {
                    elementToMove.Add(nextPage.Content[j]);
                }

                //Delete the elements to move from the next page
                var nextPageContentList = nextPage.Content.ToList();
                nextPageContentList.RemoveAll(element => elementToMove.Contains(element));
                nextPage.Content = nextPageContentList.ToArray();

                //Add the elements to move to the current page
                var pageContentList = page.Content.ToList();
                pageContentList.AddRange(elementToMove);
                page.Content = pageContentList.ToArray();

                //Save the next page
                _folderContentPageRepository.CreateOrUpdateFolderPage(nextPage.Name, nextPage.Path, i + 1 , nextPage);

                //Save the current page
                _folderContentPageRepository.CreateOrUpdateFolderPage(page.Name, page.Path, i, page);
            }
        }

        private IEnumerable<IFolderContent> GetAllPagesOfFolder(IFolder folder)
        {
            var allFolderPages =
                _folderContentPageRepository.GetAllFolderPages(folder.Name, folder.Path, folder.NumOfPhysicalPages);
            var allFolderPagesContent = new List<IFolderContent>();
            foreach (var folderPage in allFolderPages)
            {
                allFolderPagesContent.AddRange(folderPage.Content);
            }

            return allFolderPagesContent;
        }

        private int CompareFolderContent(IFolderContent folderContent1, IFolderContent folderContent2, SortType sortType)
        {
            switch (sortType)
            {
                case SortType.Name:
                    return string.Compare(folderContent1.Name, folderContent2.Name, StringComparison.Ordinal);
                case SortType.Type:
                    return folderContent1.Type.CompareTo(folderContent2.Type);
                case SortType.DateCreated:
                    return string.Compare(folderContent1.CreationTime, folderContent2.CreationTime, StringComparison.Ordinal);
                case SortType.DateModified:
                    return string.Compare(folderContent1.ModificationTime, folderContent2.ModificationTime, StringComparison.Ordinal);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null);
            }
        }
    }
}
