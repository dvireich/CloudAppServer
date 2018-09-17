using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Interfaces;

namespace FolderContentManager
{
    public class FolderContentFileManager : IFolderContentFileManager
    {
        private readonly IFileManager _fileManager;
        private readonly IJsonManager _jsonManager;
        private readonly IFolderContentFolderManager _folderContentFolderManager;
        private readonly IFolderContentPageManager _folderContentPageManager;
        public FolderContentFileManager(
            IFileManager fileManager, 
            IJsonManager jsonManager, 
            IFolderContentFolderManager folderContentFolderManager, 
            IFolderContentPageManager folderContentPageManager)
        {
            _fileManager = fileManager;
            _jsonManager = jsonManager;
            _folderContentFolderManager = folderContentFolderManager;
            _folderContentPageManager = folderContentPageManager;
        }
        #region Singelton

        private static FolderContentFileManager _instance = null;
        private static readonly object Padlock = new object();

        private FolderContentFileManager()
        {
            _folderContentPageManager = FolderContentPageManager.Instance;
            _folderContentFolderManager = FolderContentFolderManager.Instance;
            _jsonManager = new JsonManager();
            _fileManager = new FileManager();
        }

        public static FolderContentFileManager Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new FolderContentFileManager());
                }
            }
        }

        #endregion Singelton
        public void CreateFile(string name, string path, string fileType, string[] value, long size)
        {
            var file = new FileObj(name, path, fileType, new string[0], size);
            var parent = _folderContentFolderManager.GetParentFolder(file);
            _folderContentPageManager.ValidateNameInPages(parent, file);

            _fileManager.WriteJson(_jsonManager.CreateJsonPath(name, path, FolderContentType.File),
                file);

            _fileManager.WriteFileContent(CreateFilePath(name, path),
                value);

            _folderContentFolderManager.UpdateNextPageToWrite(parent);

            var page = _jsonManager.GetFolderPage(parent, parent.NextPageToWrite);
            var parentContent = page.Content.ToList();
            parentContent.Add(file);
            page.Content = parentContent.ToArray();

            _fileManager.WriteJson(_jsonManager.CreateFolderPageJsonPath(page.Name, page.Path, parent.NextPageToWrite),
                page);
        }

        public void DeleteFile(string name, string path, int page)
        {
            if (!_jsonManager.IsFolderContentExist(name, path, FolderContentType.File)) return;

            var file = _jsonManager.GetFolderContent(name, path, FolderContentType.File);

            var pathToFolderJson = _jsonManager.CreateJsonPath(file.Name, file.Path, file.Type);
            _fileManager.Delete(pathToFolderJson);

            var pathToFile = CreateFilePath(file.Name, file.Path);
            _fileManager.Delete(pathToFile);

            var parent = _folderContentFolderManager.GetParentFolder(file);
            _folderContentPageManager.UpdateDeleteFolderContentInParentPages(parent, file, page);
        }

        public string CreateFilePath(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/', '\\');
            return string.IsNullOrEmpty(path) ? $"{_jsonManager.BaseFolderPath}\\{name}" : $"{_jsonManager.BaseFolderPath}\\{path}\\{name}";
        }
    }
}
