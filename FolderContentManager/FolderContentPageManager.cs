using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentHelper.Interfaces;
using FolderContentHelper.Model;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentHelper
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class FolderContentPageManager : IFolderContentPageManager
    {
        private readonly IFileManager _fileManager;
        private readonly IJsonManager _jsonManager;
        private readonly IConstance _constance;
        public FolderContentPageManager(
            IFileManager fileManager, 
            IJsonManager jsonManager, 
            IConstance constance)
        {
            _fileManager = fileManager;
            _jsonManager = jsonManager;
            _constance = constance;
        }

        public FolderContentPageManager(IConstance constance)
        {
            _constance = constance;
            _jsonManager = new JsonManager(constance);
            _fileManager = new FileManager();
        }


        public void AddToFolderPage(int pageNum, IFolderPage pageToWriteContent, IFolderContent folderContent)
        {
            var contentList = pageToWriteContent.Content.ToList();
            contentList.Add(folderContent);
            pageToWriteContent.Content = contentList.ToArray();

            var jsonPagePath = _jsonManager.CreateFolderPageJsonPath(pageToWriteContent.Name, pageToWriteContent.Path, pageNum);
            _fileManager.WriteJson(jsonPagePath, pageToWriteContent);
        }

        public int GetNextPageToWrite(IFolder folder)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var page = _jsonManager.GetFolderPage(folder, i);
                if (page.Content.Length < _constance.MaxFolderContentOnPage) return i;
            }

            return folder.NumOfPages + 1;
        }

        public void CreateNewFolderPage(int pageNum, IFolder folder)
        {
            IFolderPage page = new FolderPage(folder.Name, folder.Path, new IFolderContent[0]);
            var jsonPagePath = _jsonManager.CreateFolderPageJsonPath(folder.Name, folder.Path, pageNum);
            _fileManager.WriteJson(jsonPagePath, page);
        }

        public void PerformPageCompression(IFolder folder)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var page = _jsonManager.GetFolderPage(folder, i);
                if (page.Content.Length == _constance.MaxFolderContentOnPage) continue;

                //Delete empty pages, but do not delete the first page
                if (page.Content.Length == 0 && i != 1)
                {
                    var pageToDeletePath = _jsonManager.CreateFolderPageJsonPath(page.Name, page.Path, i);
                    _fileManager.Delete(pageToDeletePath);
                    folder.NumOfPages--;
                    continue;
                }

                //This is the last page and it is not full -> its ok
                if (i == folder.NumOfPages) continue;

                var nextPage = _jsonManager.GetFolderPage(folder, i + 1);
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
                var nextPagePath = _jsonManager.CreateFolderPageJsonPath(nextPage.Name, nextPage.Path, i + 1);
                _fileManager.WriteJson(nextPagePath, nextPage);

                //Save the current page
                var pagePath = _jsonManager.CreateFolderPageJsonPath(page.Name, page.Path, i);
                _fileManager.WriteJson(pagePath, page);
            }

            //Save changes in the num of pages
            var pathToFolder = _jsonManager.CreateJsonPath(folder.Name, folder.Path, folder.Type);
            _fileManager.WriteJson(pathToFolder, folder);
        }

        public void UpdateRenameInParentPages(IFolder parentFolder, IFolderContent folderContent, string oldName)
        {
            for (var i = 1; i <= parentFolder.NumOfPages; i++)
            {
                var page = _jsonManager.GetFolderPage(parentFolder, i);
                if (page.Content == null) continue;

                var contentList = page.Content.ToList();
                var childContent = contentList.FirstOrDefault(fc => fc.Name == oldName &&
                                                                    fc.Path == folderContent.Path &&
                                                                    fc.Type == folderContent.Type);

                if (childContent == null) continue;

                childContent.Name = folderContent.Name;
                childContent.ModificationTime = $"{DateTime.Now:G}";
                page.Content = contentList.ToArray();
                var path = _jsonManager.CreateFolderPageJsonPath(parentFolder.Name, parentFolder.Path, i);
                _fileManager.WriteJson(path, page);
            }
        }

        private string ReplacePrefixString(string source, string oldPrefix, string newPrefix)
        {
            if (!source.StartsWith(oldPrefix)) return oldPrefix;

            var suffixPath = source.Substring(oldPrefix.Length, source.Length - oldPrefix.Length);
            return $"{newPrefix}{suffixPath}";

        }

        public void UpdateNewPathOnPage(IFolder folder, int pageNum, string oldPathPrefix, string newPathPrefix)
        {
            var page = _jsonManager.GetFolderPage(folder, pageNum);

            if (page.Path.StartsWith(oldPathPrefix))
            {
                var newPath = ReplacePrefixString(page.Path, oldPathPrefix, newPathPrefix);
                page.Path = newPath;
            }
            foreach (var fc in page.Content)
            {
                if (!fc.Path.StartsWith(oldPathPrefix)) continue;
                var newPath = ReplacePrefixString(fc.Path, oldPathPrefix, newPathPrefix);
                fc.Path = newPath;
            }

            var pathToPage = _jsonManager.CreateFolderPageJsonPath(page.Name, page.Path, pageNum);
            _fileManager.WriteJson(pathToPage, page);
        }

        public void MoveAllRootPagesToPath(IFolder folder,  string newPath)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var page = _jsonManager.GetFolderPage(folder, i);
                page.Path = ReplacePrefixString(page.Path, folder.Path, newPath);
                var pathToPage = _jsonManager.CreateFolderPageJsonPath(page.Name, newPath, i);
                _fileManager.WriteJson(pathToPage, page);
            }
        }

        public void UpdateDeleteFolderContentInParentPages(IFolder parentFolder,IFolderContent folder, int page)
        {
            var folderPage = _jsonManager.GetFolderPage(parentFolder, page);

            if (folderPage.Content == null) return;

            var contentList = folderPage.Content.ToList();
            contentList.RemoveAll(fc => fc.Name == folder.Name &&
                                        fc.Path == folder.Path &&
                                        fc.Type == folder.Type);

            folderPage.Content = contentList.ToArray();
            var pathToPage = _jsonManager.CreateFolderPageJsonPath(parentFolder.Name, parentFolder.Path, page);
            _fileManager.WriteJson(pathToPage, folderPage);

            PerformPageCompression(parentFolder);
        }

        public void ValidateNameInPages(IFolder folderToValidate, IFolderContent newFolderContent)
        {
            if (folderToValidate == null) return;

            for (var i = 1; i <= folderToValidate.NumOfPages; i++)
            {
                var page = _jsonManager.GetFolderPage(folderToValidate, i);
                if (page.Content.Any(f => f.Name == newFolderContent.Name && f.Type == newFolderContent.Type))
                {
                    throw new Exception("The name exists in this folder!");
                }
            }
        }

        public void RenameFolderPageInternalNameField(IFolder folder, string newName)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var page = _jsonManager.GetFolderPage(folder, i);
                page.Name = newName;
                var path = _jsonManager.CreateFolderPageJsonPath(folder.Name, folder.Path, i);
                _fileManager.WriteJson(path, page);
            }
        }
    }
}
