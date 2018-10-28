using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentHelper.Interfaces;
using FolderContentHelper.Model;
using FolderContentManager.Model;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentHelper
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class FolderContentFolderManager : IFolderContentFolderManager
    {
        private readonly IJsonManager _jsonManager;
        private readonly IFileManager _fileManager;
        private readonly IDirectoryManager _directoryManager;
        private readonly IFolderContentPageManager _folderContentPageManager;
        private readonly IConstance _constance;


        public FolderContentFolderManager(
            IJsonManager jsonManager, 
            IFileManager fileManager, 
            IDirectoryManager directoryManager, 
            IFolderContentPageManager folderContentPageManager, 
            IConstance constance)
        {
            _jsonManager = jsonManager;
            _fileManager = fileManager;
            _directoryManager = directoryManager;
            _folderContentPageManager = folderContentPageManager;
            _constance = constance;
        }

        public FolderContentFolderManager(IConstance constance)
        {
            _folderContentPageManager = new FolderContentPageManager(constance);
            _jsonManager = new JsonManager(constance);
            _directoryManager = new DirectoryManager();
            _fileManager = new FileManager();
            _constance = constance;
        }

        private string ReplacePrefixString(string source, string oldPrefix, string newPrefix)
        {
            if (!source.StartsWith(oldPrefix)) return oldPrefix;

            var suffixPath = source.Substring(oldPrefix.Length, source.Length - oldPrefix.Length);
            return $"{newPrefix}{suffixPath}";

        }

        public void UpdateChildrenPath(IFolderContent folderContent, string newPathPrefix, string oldPathPrefix)
        {
            if (folderContent.Type != FolderContentType.Folder) return;
            var folder = _jsonManager.GetFolder(folderContent.Name, folderContent.Path);

            if (folder.Path.StartsWith(oldPathPrefix))
            {
                var newPath = ReplacePrefixString(folder.Path, oldPathPrefix, newPathPrefix);
                folder.Path = newPath;
            }

            var folderPath = _jsonManager.CreateJsonPath(folder.Name, folder.Path, folder.Type);
            _fileManager.WriteJson(folderPath, folder);

            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                _folderContentPageManager.UpdateNewPathOnPage(folder, i, oldPathPrefix, newPathPrefix);
                var page = _jsonManager.GetFolderPage(folder, i);

                foreach (var fc in page.Content)
                {
                    UpdateChildrenPath(fc, newPathPrefix, oldPathPrefix);
                }
            }
        }

        public IFolder GetParentFolder(IFolderContent folder)
        {
            var parentName = GetParentName(folder);
            var parentPath = GetParentPath(folder);
            return _jsonManager.GetFolder(parentName, parentPath);
        }

        private string GetParentName(IFolderContent folder)
        {
            var path = folder.Path;

            if (string.IsNullOrEmpty(path)) return _constance.HomeFolderName;

            var spittedPathArr = path.Split('/').ToList();
            return spittedPathArr.Last();
        }

        private string GetParentPath(IFolderContent folder)
        {
            var path = folder.Path;

            if (string.IsNullOrEmpty(path)) return string.Empty;

            var spittedPathArr = path.Split('/').ToList();

            if (spittedPathArr.Count == 1) return string.Empty;

            spittedPathArr.RemoveAt(spittedPathArr.Count - 1);
            return spittedPathArr.Aggregate((i, j) => i + '/' + j);
        }

        public void DeleteFolder(string name, string path, int page)
        {
            if (!_jsonManager.IsFolderContentExist(name, path, FolderContentType.Folder)) return;

            var folder = _jsonManager.GetFolder(name, path);
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var folderPage = _jsonManager.GetFolderPage(folder, i);
                foreach (var folderContent in folderPage.Content)
                {
                    DeleteFolder(folderContent.Name, folderContent.Path, page);
                }
                var pathToFolderJson = _jsonManager.CreateJsonPath(folder.Name, folder.Path, folder.Type);
                _fileManager.Delete(pathToFolderJson);
                var pathToPage = _jsonManager.CreateFolderPageJsonPath(folderPage.Name, folderPage.Path, i);
                _fileManager.Delete(pathToPage);
            }

            var pathToFolder = CreateFolderPath(folder.Name, folder.Path);
            if (_directoryManager.Exists(pathToFolder))
            {
                _directoryManager.Delete(pathToFolder, true);
            }

            var parent = GetParentFolder(folder);
            _folderContentPageManager.UpdateDeleteFolderContentInParentPages(parent, folder, page);
        }

        public string CreateFolderPath(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/', '\\');
            return string.IsNullOrEmpty(path) ? $"{_constance.BaseFolderPath}\\{name}" : $"{_constance.BaseFolderPath}\\{path}\\{name}";
        }

        public void RenameDirectory(string path, string oldName, string newName)
        {
            var newDirPath = CreateFolderPath(newName, path);
            var oldDirPath = CreateFolderPath(oldName, path);

            _directoryManager.DirectoryCopy(oldDirPath, newDirPath, true);
            _directoryManager.Delete(oldDirPath, true);
        }

        public void CreateFolder(IFolder folder)
        {
            CreateDirectoryIfNotExists(folder);
            var jsonPath = _jsonManager.CreateJsonPath(folder.Name, folder.Path, FolderContentType.Folder);
            _fileManager.WriteJson(jsonPath, folder);

            _folderContentPageManager.CreateNewFolderPage(1, folder);
            UpdateNewFolderInParentData(folder);
        }

        public void CreateFolder(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            if (path.ToCharArray().Last() == '/')
            {
                var listOfChars = path.ToCharArray().ToList();
                listOfChars.RemoveAt(listOfChars.Count - 1);
                path = new string(listOfChars.ToArray());
            }
            ValidatePath(path);
            IFolder newFolder = new FolderObj(name, path);
            var parent = GetParentFolder(newFolder);
            _folderContentPageManager.ValidateNameInPages(parent, newFolder);
            CreateFolder(newFolder);
        }

        private void ValidatePath(string path)
        {
            path = path.ToLower();
            const string errorPathEmpty = "Path cannot be empty! it is at least must have 'home/'!";
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path)) throw new ArgumentException(errorPathEmpty);
        }

        public void CreateDirectoryIfNotExists(IFolder folder)
        {
            var directoryPath = CreateFolderPath(folder.Name, folder.Path);
            if (!_directoryManager.Exists(directoryPath))
            {
                _directoryManager.CreateDirectory(directoryPath);
            }
        }

        private void UpdateNewFolderInParentData(IFolderContent folder)
        {
            var parent = GetParentFolder(folder);
            if (parent == null) return;

            UpdateNextPageToWrite(parent);

            var pageToWriteContent = _jsonManager.GetFolderPage(parent, parent.NextPageToWrite);
            _folderContentPageManager.AddToFolderPage(parent.NextPageToWrite, pageToWriteContent, folder);
        }

        public void UpdateNextPageToWrite(IFolder folder)
        {
            folder.NextPageToWrite = _folderContentPageManager.GetNextPageToWrite(folder);
            if (folder.NextPageToWrite > folder.NumOfPages)
            {
                folder.NumOfPages++;
                _folderContentPageManager.CreateNewFolderPage(folder.NextPageToWrite, folder);
            }
            var path = _jsonManager.CreateJsonPath(folder.Name, folder.Path, folder.Type);
            _fileManager.WriteJson(path, folder);
        }

        public void UpdateFolderMetaData(FolderMetadata folderMetadata)
        {
            var folder = _jsonManager.GetFolder(folderMetadata.Name, folderMetadata.Path);
            folder.SortType = folderMetadata.SortType;
            var path = _jsonManager.CreateJsonPath(folder.Name, folder.Path, folder.Type);
            _fileManager.WriteJson(path, folder);
        }
    }
}
