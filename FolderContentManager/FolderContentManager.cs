using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudAppServer.Model;
using FolderContentHelper.Interfaces;
using FolderContentHelper.Model;
using FolderContentManager;
using FolderContentManager.Interfaces;
using FolderContentManager.Model;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentHelper
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public,
        AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class FolderContentManager : IFolderContentManager
    {
        private readonly IFileManager _fileManager;
        private readonly IDirectoryManager _directoryManager;
        private readonly IJsonManager _jsonManager;
        private readonly IFolderContentFolderManager _folderContentFolderManager;
        private readonly IFolderContentPageManager _folderContentPageManager;
        private readonly IFolderContentFileManager _folderContentFileManager;
        private readonly IConstance _constance;
        private readonly ISearchCache _searchCache;
        private readonly IFolderContentConcurrentManager _concurrentManager;

        //Need to call the ctor only once in order to do the initializations once
        public FolderContentManager(IConstance constance, IFolderContentConcurrentManager concurrentManager)
        {
            _constance = constance;
            _fileManager = new FileManager();
            _jsonManager = new JsonManager(_constance);
            _directoryManager = new DirectoryManager();
            _searchCache = new SearchCache();

            _concurrentManager = concurrentManager;
            _folderContentPageManager = new FolderContentPageManager(constance);
            _folderContentFileManager = new FolderContentFileManager(constance);
            _folderContentFolderManager = new FolderContentFolderManager(constance);

            InitializeBaseFolder();
            InitializeHomeFolder();
        }

        public FolderContentManager(
            IFileManager fileManager,
            IDirectoryManager directoryManager,
            IJsonManager jsonManager,
            IFolderContentFolderManager folderContentFolderManager,
            IFolderContentPageManager folderContentPageManager,
            IFolderContentFileManager folderContentFileManager,
            IConstance constance,
            ISearchCache searchCache, IFolderContentConcurrentManager concurrentManager)
        {
            _fileManager = fileManager;
            _directoryManager = directoryManager;
            _jsonManager = jsonManager;
            _folderContentFolderManager = folderContentFolderManager;
            _folderContentPageManager = folderContentPageManager;
            _folderContentFileManager = folderContentFileManager;
            _constance = constance;
            _concurrentManager = concurrentManager;

            InitializeBaseFolder();
            InitializeHomeFolder();
        }

        private void InitializeBaseFolder()
        {
            if (_directoryManager.Exists(_constance.BaseFolderPath)) return;
            _directoryManager.CreateDirectory(_constance.BaseFolderPath);
        }


        private void InitializeHomeFolder()
        {
            var homeFolderPath = _folderContentFolderManager.CreateFolderPath(_constance.HomeFolderName, string.Empty);
            if (_directoryManager.Exists(homeFolderPath)) return;
            _directoryManager.CreateDirectory(homeFolderPath);

            IFolder homeFolder = new FolderObj(_constance.HomeFolderName, string.Empty);
            var jsonPath = _jsonManager.CreateJsonPath(homeFolder.Name, homeFolder.Path, FolderContentType.Folder);
            _fileManager.WriteJson(jsonPath, homeFolder);

            _folderContentPageManager.CreateNewFolderPage(1, homeFolder);
        }

        public IFolder GetFolderObj(string name, string path)
        {
            return _jsonManager.GetFolder(name, path);
        }

        public void UpdateFolderMetaData(FolderMetadata folderMetadata)
        {
            _folderContentFolderManager.UpdateFolderMetaData(folderMetadata);
        }

        public void CreateFolder(string name, string path)
        {
            _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() {new FolderContent(name, path, FolderContentType.Folder)}, () =>
                {
                    _searchCache.ClearCache();
                    _folderContentFolderManager.CreateFolder(name, path);
                });
        }

        public void DeleteFolder(string name, string path, int page)
        {
            _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() {new FolderContent(name, path, FolderContentType.Folder)}, () =>
                {
                    _searchCache.ClearCache();
                    _folderContentFolderManager.DeleteFolder(name, path, page);
                });
        }

        public void Rename(string name, string path, string typeStr, string newName)
        {
            Enum.TryParse(typeStr, true, out FolderContentType type);

            _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() {new FolderContent(name, path, type)}, () =>
                {
                    _searchCache.ClearCache();
                    var folderContent = _jsonManager.GetFolderContent(name, path, type);
                    if (folderContent == null) throw new Exception("folder content is not exists!");
                    var oldName = folderContent.Name;
                    var oldPath = $"{folderContent.Path}/{oldName}";
                    var newPath = $"{folderContent.Path}/{newName}";

                    folderContent = _jsonManager.GetFolderIfFolderType(folderContent);
                    folderContent.Name = newName;
                    folderContent.ModificationTime = $"{DateTime.Now:G}";

                    if (folderContent.Type == FolderContentType.Folder)
                    {
                        _folderContentFolderManager.RenameDirectory(folderContent.Path, oldName, newName);
                        RenameFolderPagesJsonFiles(folderContent as IFolder, oldName, newName);
                        _folderContentPageManager.RenameFolderPageInternalNameField(folderContent as IFolder, newName);
                    }

                    if (folderContent.Type == FolderContentType.File)
                    {
                        var oldNameFullPath = _folderContentFileManager.CreateFilePath(oldName, folderContent.Path);
                        var newNameFullPath = _folderContentFileManager.CreateFilePath(newName, folderContent.Path);
                        _fileManager.Move(oldNameFullPath, newNameFullPath);
                    }

                    var dirPath =
                        _jsonManager.CreateJsonPath(folderContent.Name, folderContent.Path, folderContent.Type);
                    _fileManager.WriteJson(dirPath, folderContent);
                    _fileManager.Delete(_jsonManager.CreateJsonPath(oldName, folderContent.Path, folderContent.Type));

                    var parent = _folderContentFolderManager.GetParentFolder(folderContent);
                    _folderContentPageManager.UpdateRenameInParentPages(parent, folderContent, oldName);
                    _folderContentFolderManager.UpdateChildrenPath(folderContent, newPath, oldPath);
                });
        }

        private void RenameFolderPagesJsonFiles(IFolder folder, string oldName, string newName)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var oldNameFullPath = _jsonManager.CreateFolderPageJsonPath(oldName, folder.Path, i);
                var newNameFullPath = _jsonManager.CreateFolderPageJsonPath(newName, folder.Path, i);
                _fileManager.Move(oldNameFullPath, newNameFullPath);
                if (_fileManager.Exists(oldNameFullPath))
                {
                    _fileManager.Delete(oldNameFullPath);
                }
            }
        }

        public void Copy(string copyObjName, string copyObjPath, string copyObjTypeStr, string copyToName,
            string copyToPath)
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
                    
                    var folderToCopyTo = _jsonManager.GetFolder(copyToName, copyToPath);
                    var folderContentToCopy = _jsonManager.GetFolderContent(copyObjName, copyObjPath, copyObjType);

                    ValidateFoldersOnCopy(folderToCopyTo, folderContentToCopy);

                    //Add the new folder content and fix the path
                    var folderContentToCopyOldPath = folderContentToCopy.Path;
                    folderContentToCopy.Path = string.IsNullOrEmpty(folderToCopyTo.Path)
                        ? folderToCopyTo.Name
                        : $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";

                    AddToTragetFolderPageTheSourceFolderOnCopy(folderToCopyTo, folderContentToCopy);

                    //Fix the path in the folder content json file
                    folderContentToCopy.Path = folderContentToCopyOldPath;
                    folderContentToCopy = _jsonManager.GetFolderIfFolderType(folderContentToCopy);

                    UpdateFolderContentToCopyJson(folderContentToCopy, folderToCopyTo);

                    switch (folderContentToCopy.Type)
                    {
                        //Fix the children path and move the directory
                        case FolderContentType.Folder:
                            HandleFolderCopy(folderContentToCopy, folderContentToCopyOldPath, folderToCopyTo);
                            break;
                        case FolderContentType.File:
                            HandleFileCopy(folderContentToCopy, folderContentToCopyOldPath);
                            break;
                        case FolderContentType.FolderPageResult:
                            break;
                        case FolderContentType.FolderPage:
                            break;
                    }
                });
        }

        private void AddToTragetFolderPageTheSourceFolderOnCopy(IFolder folderToCopyTo, IFolderContent folderContentToCopy)
        {
            var pageOfFolderToCopyTo =
                _jsonManager.GetFolderPage(folderToCopyTo, folderToCopyTo.NextPageToWrite);
            _folderContentPageManager.AddToFolderPage(folderToCopyTo.NextPageToWrite, pageOfFolderToCopyTo,
                folderContentToCopy);
        }

        private void HandleFileCopy(IFolderContent folderContentToCopy, string folderContentToCopyOldPath)
        {
            _fileManager.Copy(
                _folderContentFileManager.CreateFilePath(folderContentToCopy.Name,
                    folderContentToCopyOldPath),
                _folderContentFileManager.CreateFilePath(folderContentToCopy.Name,
                    folderContentToCopy.Path));
        }

        private void HandleFolderCopy(IFolderContent folderContentToCopy, string folderContentToCopyOldPath,
            IFolder folderToCopyTo)
        {
            _directoryManager.DirectoryCopy(
                _folderContentFolderManager.CreateFolderPath(folderContentToCopy.Name,
                    folderContentToCopyOldPath),
                $"{_folderContentFolderManager.CreateFolderPath(folderToCopyTo.Name, folderToCopyTo.Path)}\\{folderContentToCopy.Name}",
                true);

            //Move folder content to copy pages to the new location
            CopyFolderContentToCopyPagesToNewLocation((IFolder) folderContentToCopy, folderContentToCopyOldPath,
                folderToCopyTo);

            _folderContentFolderManager.UpdateChildrenPath(folderContentToCopy,
                $"{folderContentToCopy.Path}/{folderContentToCopy.Name}",
                $"{folderContentToCopyOldPath}/{folderContentToCopy.Name}");
        }

        private void CopyFolderContentToCopyPagesToNewLocation(IFolder folderContentToCopy,
            string folderContentToCopyOldPath, IFolderContent folderToCopyTo)
        {
            folderContentToCopy.Path = folderContentToCopyOldPath;
            var folderContentToCopyNewPath = string.IsNullOrEmpty(folderToCopyTo.Path)
                ? folderToCopyTo.Name
                : $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";
            _folderContentPageManager.MoveAllRootPagesToPath(folderContentToCopy, folderContentToCopyNewPath);
            folderContentToCopy.Path = folderContentToCopyNewPath;
        }

        private void UpdateFolderContentToCopyJson(IFolderContent folderContentToCopy, IFolder folderToCopyTo)
        {
            folderContentToCopy.Path = string.IsNullOrEmpty(folderToCopyTo.Path)
                ? folderToCopyTo.Name
                : $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";
            _fileManager.WriteJson(
                _jsonManager.CreateJsonPath(folderContentToCopy.Name, folderContentToCopy.Path,
                    folderContentToCopy.Type),
                folderContentToCopy);
        }

        private void ValidateFoldersOnCopy(IFolder folderToCopyTo, IFolderContent folderContentToCopy)
        {
            //Validate folder to copy to
            _folderContentFolderManager.UpdateNextPageToWrite(folderToCopyTo);
            if (folderToCopyTo == null)
                throw new Exception("The folder you are trying to copy to does not exists!");

            //Validate folder content to copy
            if (folderContentToCopy == null)
                throw new Exception("The folder content you are trying to copy does not exists!");
        }

        public void CreateFile(string name, string path, string fileType, string tmpCreationPath, long size)
        {
            _searchCache.ClearCache();
            _folderContentFileManager.CreateFile(name, path, fileType, tmpCreationPath, size);
            _concurrentManager.ReleaseSynchronization(new List<IFolderContent>(){new FolderContent(name, path, FolderContentType.File)});
        }

        public Stream GetFile(string name, string path)
        {
            return _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() {new FolderContent(name, path, FolderContentType.File)},
                () => _fileManager.GetFile(_folderContentFileManager.CreateFilePath(name, path)));
    }

        public void DeleteFile(string name, string path, int page)
        {
            _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() {new FolderContent(name, path, FolderContentType.File)},
                () =>
                {
                    _searchCache.ClearCache();
                    _folderContentFileManager.DeleteFile(name, path, page);
                });
        }

        public int GetNumOfFolderPages(string name, string path)
        {
            if (name.ToLower() == "search")
            {
                if (!_searchCache.Contains(path))
                    throw new Exception($"The search string {path} does not appears in the searchCache!");

                var results = _searchCache.GetFromCache(path);
                var pages = (double) results.Length / _constance.MaxFolderContentOnPage;
                var pagesCeiling = (int) Math.Ceiling((decimal) pages);
                return pagesCeiling == 0 ? 1 : pagesCeiling;
            }

            name = name.ToLower();
            path = path.ToLower();
            var folder = !_jsonManager.IsFolderContentExist(name, path, FolderContentType.Folder)
                ? null
                : _fileManager
                    .ReadJson<RestFolderObj>(_jsonManager.CreateJsonPath(name, path, FolderContentType.Folder))
                    .MapToIFolder();

            return folder?.NumOfPages ?? -1;
        }

        public IFolderPage GetFolderPage(string name, string path, int page)
        {
            var folder = _jsonManager.GetFolder(name, path);
            return _jsonManager.GetFolderPage(folder, page);
        }

        public IFile GetFileObj(string name, string path)
        {
            return _concurrentManager.PerformWithSynchronization(
                new List<IFolderContent>() {new FolderContent(name, path, FolderContentType.File)},
                () => _jsonManager.GetFileObj(name, path));

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
                    var homeFolder = _jsonManager.GetFolder(_constance.HomeFolderName, _constance.HomeFolderPath);
                    RecursiveSearch(name, homeFolder, result);

                    _searchCache.AddToCache(name, result.ToArray());

                    var resultPageArray = result.Skip(numOfElementToSkip).Take(_constance.MaxFolderContentOnPage)
                        .ToArray();
                    return resultPageArray;
                });
        }

        private void RecursiveSearch(string strToSearch, IFolder folder, List<IFolderContent> result)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var page = _jsonManager.GetFolderPage(folder, i);

                foreach (var folderContent in page.Content)
                {
                    if (folderContent.Name.Contains(strToSearch))
                    {
                        result.Add(folderContent);
                    }

                    if (folderContent.Type != FolderContentType.Folder) continue;

                    var folderToCheck = _jsonManager.GetFolder(folderContent.Name, folderContent.Path);
                    RecursiveSearch(strToSearch, folderToCheck, result);
                }
            }
        }
    }
}
