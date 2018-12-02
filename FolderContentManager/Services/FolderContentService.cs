using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FolderContentManager.Helpers;
using FolderContentManager.Model;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentManager.Services
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public,
        AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class FolderContentService : IFolderContentService
    {
        private readonly IConstance _constance;
        private readonly IFolderContentFolderService _folderContentFolderService;
        private readonly IFolderContentPageService _folderContentPageService;
        private readonly IFolderContentConcurrentManager _concurrentManager;
        private readonly IFolderContentFileService _folderContentFileService;
        private readonly ISearchCache _searchCache;

        public FolderContentService(IConstance constance, IFolderContentConcurrentManager concurrentManager)
        {
            _constance = constance;
            _concurrentManager = concurrentManager;
            _searchCache = new SearchCache();
            _folderContentPageService = new FolderContentPageService(constance);
            _folderContentFileService = new FolderContentFileService(constance);
            _folderContentFolderService = new FolderContentFolderService(constance);

            InitializeBaseFolder();
            InitializeHomeFolder();
        }

        public void CreateFolder(string name, string path)
        {
            _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() { new FolderContent(name, path, FolderContentType.Folder) }, () =>
                {
                    _searchCache.ClearCache();
                    _folderContentFolderService.CreateFolder(name, path);
                });
        }

        public void DeleteFolder(string name, string path, int page)
        {
            _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() { new FolderContent(name, path, FolderContentType.Folder) }, () =>
                {
                    _searchCache.ClearCache();
                    _folderContentFolderService.DeleteFolder(name, path, page);
                });
        }

        public void DeleteFile(string name, string path, int page)
        {
            _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() { new FolderContent(name, path, FolderContentType.File) },
                () =>
                {
                    _searchCache.ClearCache();
                    _folderContentFileService.DeleteFile(name, path, page);
                });
        }

        public Stream GetFile(string name, string path)
        {
            return _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() { new FolderContent(name, path, FolderContentType.File) },
                () => _folderContentFileService.GetFile(name, path));
        }

        public IFolderPage GetFolderPage(string name, string path, int page)
        {
            var folder = _folderContentFolderService.GetFolder(name, path);
            return _folderContentPageService.GetFolderPage(folder, page);
        }

        public int GetSortType(string name, string path)
        {
            var folder = _folderContentFolderService.GetFolder(name, path);
            if (folder == null) return 0;

            return (int)folder.SortType;
        }

        public int GetNumOfFolderPages(string name, string path)
        {
            if (name.ToLower() == "search")
            {
                if (!_searchCache.Contains(path))
                    throw new Exception($"The search string {path} does not appears in the searchCache!");

                var results = _searchCache.GetFromCache(path);
                var pages = (double)results.Length / _constance.MaxFolderContentOnPage;
                var pagesCeiling = (int)Math.Ceiling((decimal)pages);
                return pagesCeiling == 0 ? 1 : pagesCeiling;
            }

            var folder = _folderContentFolderService.GetFolder(name, path);
            if (folder == null) return -1;
            return _folderContentPageService.GetNumberOfPages(folder);
        }

        public void Rename(string name, string path, string typeStr, string newName)
        {
            Enum.TryParse(typeStr, true, out FolderContentType type);

            _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() { new FolderContent(name, path, type) }, () =>
                {
                    _searchCache.ClearCache();
                    if (type == FolderContentType.File)
                    {
                        _folderContentFileService.RenameFile(name, newName, path);
                    }

                    if (type == FolderContentType.Folder)
                    {
                        _folderContentFolderService.RenameFolder(name, newName, path);
                    }
                });
        }

        public void CreateFile(string name, string path, string fileType, string tmpCreationPath, long size)
        {
            _searchCache.ClearCache();
            _folderContentFileService.CreateFile(name, path, fileType, tmpCreationPath, size);
            _concurrentManager.ReleaseSynchronization(new List<IFolderContent>() { new FolderContent(name, path, FolderContentType.File) });
        }

        public void Copy(string copyObjName, string copyObjPath, string copyObjTypeStr, string copyToName, string copyToPath)
        {

            Enum.TryParse(copyObjTypeStr, true, out FolderContentType copyObjType);

            _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>()
                {
                    new FolderContent(copyToName, copyToPath, FolderContentType.Folder),
                    new FolderContent(copyObjName, copyObjPath, copyObjType)
                },
                () =>
                {
                    _searchCache.ClearCache();

                    if (copyObjType == FolderContentType.File)
                    {
                        _folderContentFileService.Copy(copyObjName, copyObjPath, copyToName, copyToPath);
                    }

                    if (copyObjType == FolderContentType.Folder)
                    {
                        _folderContentFolderService.Copy(copyObjName, copyObjPath, copyToName, copyToPath);
                    }
                });
        }

        public void UpdateFolderMetaData(FolderMetadata folderMetadata)
        {
            _folderContentFolderService.UpdateFolderMetaData(folderMetadata);
        }

        public IFolderContent[] Search(string name, int page)
        {
            return _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>()
                    {new FolderContent(_constance.HomeFolderName, _constance.HomeFolderPath, FolderContentType.Folder)},
                () =>
                {
                    var numOfElementToSkip = (page - 1) * _constance.MaxFolderContentOnPage;
                    if (_searchCache.Contains(name))
                    {
                        var searchResult = _searchCache.GetFromCache(name);
                        return searchResult.Skip(numOfElementToSkip).Take(_constance.MaxFolderContentOnPage).ToArray();
                    }

                    var result = new List<IFolderContent>();
                    var homeFolder = _folderContentFolderService.GetFolder(_constance.HomeFolderName, _constance.HomeFolderPath);
                    RecursiveSearch(name, homeFolder, result);

                    _searchCache.AddToCache(name, result.ToArray());

                    var resultPageArray = result.Skip(numOfElementToSkip).Take(_constance.MaxFolderContentOnPage)
                        .ToArray();
                    return resultPageArray;
                });
        }

        private void RecursiveSearch(string strToSearch, IFolder folder, List<IFolderContent> result)
        {
            for (var i = 1; i <= folder.NumOfPhysicalPages; i++)
            {
                var page = _folderContentPageService.GetFolderPage(folder, i);

                foreach (var folderContent in page.Content)
                {
                    if (folderContent.Name.Contains(strToSearch))
                    {
                        result.Add(folderContent);
                    }

                    if (folderContent.Type != FolderContentType.Folder) continue;

                    var folderToCheck = _folderContentFolderService.GetFolder(folderContent.Name, folderContent.Path);
                    RecursiveSearch(strToSearch, folderToCheck, result);
                }
            }
        }

        private void InitializeBaseFolder()
        {
            _folderContentFolderService.CreateDirectory(string.Empty, string.Empty);
        }

        private void InitializeHomeFolder()
        {
            var homeFolder = _folderContentFolderService.GetFolder(_constance.HomeFolderName, string.Empty);
            if (homeFolder != null) return;
            _folderContentFolderService.CreateFolder(_constance.HomeFolderName, string.Empty);
            homeFolder = _folderContentFolderService.GetFolder(_constance.HomeFolderName, string.Empty);
            _folderContentPageService.RemoveFolderContentFromPage(homeFolder, homeFolder, 1);
        }
    }
}
