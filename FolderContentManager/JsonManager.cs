using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Interfaces;
using FolderContentManager.Model;
using FolderContentManager.Model.RestObject;

namespace FolderContentManager
{
    public class JsonManager : IJsonManager
    {
        private IFileManager _fileManager;

        public JsonManager(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public JsonManager()
        {
            _fileManager = new FileManager();
        }

        public string BaseFolderPath { get; } = "C:\\foldercontentmanager";

        public IFolder GetFolder(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            return !IsFolderContentExist(name, path, FolderContentType.Folder) ? null : _fileManager.ReadJson<RestFolderObj>(CreateJsonPath(name, path, FolderContentType.Folder)).MapToIFolder();
        }

        public IFolderContent GetFolderContent(string name, string path, FolderContentType type)
        {
            return !IsFolderContentExist(name, path, type) ? null : _fileManager.ReadJson<RestFolderContent>(CreateJsonPath(name, path, type)).MapToIFolderContent();
        }

        public IFolderPage GetFolderPage(IFolder folder, int page)
        {
            return !IsFolderPageExist(folder.Name, folder.Path, page) ?
                null :
                _fileManager.ReadJson<RestFolderPageObj>(CreateFolderPageJsonPath(folder.Name, folder.Path, page)).MapToIFolderPage();
        }

        public IFolderContent GetFolderIfFolderType(IFolderContent folderContent)
        {
            return folderContent.Type == FolderContentType.Folder ? GetFolder(folderContent.Name, folderContent.Path) : folderContent;
        }

        public bool IsFolderContentExist(string name, string path, FolderContentType type)
        {
            name = name.ToLower();
            path = path.ToLower();
            return _fileManager.Exists(CreateJsonPath(name, path, type));
        }

        public bool IsFolderPageExist(string name, string path, int page)
        {
            name = name.ToLower();
            path = path.ToLower();
            return _fileManager.Exists(CreateFolderPageJsonPath(name, path, page));
        }

        public string CreateJsonPath(string name, string path, FolderContentType type)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/', '\\');
            return string.IsNullOrEmpty(path) ? $"{BaseFolderPath}\\{name}{type.ToString()}.json" : $"{BaseFolderPath}\\{path}\\{name}{type.ToString()}.json";
        }

        public string CreateFolderPageJsonPath(string name, string path, int page)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/', '\\');
            return string.IsNullOrEmpty(path) ?
                $"{BaseFolderPath}\\{name}{FolderContentType.FolderPage.ToString()}.{page}.json" :
                $"{BaseFolderPath}\\{path}\\{name}{FolderContentType.FolderPage.ToString()}.{page}.json";
        }
    }
}
