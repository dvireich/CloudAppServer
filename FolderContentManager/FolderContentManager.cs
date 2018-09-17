using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CloudAppServer.Model;
using FolderContentManager.Interfaces;
using FolderContentManager.Model;
using FolderContentManager.Model.RestObject;

namespace FolderContentManager
{
    public class FolderContentManager : IFolderContentManager
    {
        private readonly IFileManager _fileManager;
        private readonly IDirectoryManager _directoryManager;
        private readonly IJsonManager _jsonManager;
        private readonly IFolderContentFolderManager _folderContentFolderManager;
        private readonly IFolderContentPageManager _folderContentPageManager;
        private readonly IFolderContentFileManager _folderContentFileManager;
        private readonly IConstance _constance;

        #region Singelton
        private static FolderContentManager _instance = null;
        private static readonly object Padlock = new object();

        //Need to call the ctor only once in order to do the initializations once
        private FolderContentManager()
        {
            _constance = new Constance();
            _fileManager = new FileManager();
            _jsonManager = new JsonManager();
            _directoryManager = new DirectoryManager();

            _folderContentPageManager = FolderContentPageManager.Instance;
            _folderContentFileManager = FolderContentFileManager.Instance;
            _folderContentFolderManager = FolderContentFolderManager.Instance;
            

            InitializeBaseFolder();
            InitializeHomeFolder();
        }

        private FolderContentManager(IFileManager fileManager, IDirectoryManager directoryManager, IJsonManager jsonManager, IFolderContentFolderManager folderContentFolderManager, IFolderContentPageManager folderContentPageManager, IFolderContentFileManager folderContentFileManager, IConstance constance)
        {
            _fileManager = fileManager;
            _directoryManager = directoryManager;
            _jsonManager = jsonManager;
            _folderContentFolderManager = folderContentFolderManager;
            _folderContentPageManager = folderContentPageManager;
            _folderContentFileManager = folderContentFileManager;
            _constance = constance;

            InitializeBaseFolder();
            InitializeHomeFolder();
        }

        public static FolderContentManager Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new FolderContentManager());
                }
            }
        }

        #endregion Singelton

        private void InitializeBaseFolder()
        {
            if (_directoryManager.Exists(_jsonManager.BaseFolderPath)) return;
            _directoryManager.CreateDirectory(_jsonManager.BaseFolderPath);
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
        
        public void CreateFolder(string name, string path)
        {
            _folderContentFolderManager.CreateFolder(name, path);
        }

        public void DeleteFolder(string name, string path, int page)
        {
            _folderContentFolderManager.DeleteFolder(name, path, page);
        }

        public void Rename(string name, string path, string typeStr, string newName)
        {
            Enum.TryParse(typeStr, true, out FolderContentType type);
            var folderContent = _jsonManager.GetFolderContent(name, path, type);
            if(folderContent == null) throw new Exception("folder content is not exists!");
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
            
            var dirPath = _jsonManager.CreateJsonPath(folderContent.Name, folderContent.Path, folderContent.Type);
            _fileManager.WriteJson(dirPath, folderContent);
            _fileManager.Delete(_jsonManager.CreateJsonPath(oldName, folderContent.Path, folderContent.Type));

            var parent = _folderContentFolderManager.GetParentFolder(folderContent);
            _folderContentPageManager.UpdateRenameInParentPages(parent, folderContent, oldName);
            _folderContentFolderManager.UpdateChildrenPath(folderContent, newPath, oldPath);
        }

        private void RenameFolderPagesJsonFiles(IFolder folder,string oldName, string newName)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var oldNameFullPath = _jsonManager.CreateFolderPageJsonPath(oldName, folder.Path, i);
                var newNameFullPath = _jsonManager.CreateFolderPageJsonPath(newName, folder.Path, i);
                _fileManager.Move(oldNameFullPath, newNameFullPath);
                _fileManager.Delete(oldNameFullPath);
            }
        }

        public void Copy(string copyObjName, string copyObjPath, string copyObjTypeStr, string copyToName,
            string copyToPath)
        {
            //Validate folder to copy to
            var folderToCopyTo = _jsonManager.GetFolder(copyToName, copyToPath);
            _folderContentFolderManager.UpdateNextPageToWrite(folderToCopyTo);
            if(folderToCopyTo == null) throw new Exception("The folder you are trying to copy to does not exists!");

            //Validate folder content to copy
            Enum.TryParse(copyObjTypeStr, true, out FolderContentType copyObjType);
            var folderContentToCopy = _jsonManager.GetFolderContent(copyObjName, copyObjPath, copyObjType);
            if(folderContentToCopy == null) throw new Exception("The folder content you are trying to copy does not exists!");

            //Add the new folder content and fix the path
            var folderContentToCopyOldPath = folderContentToCopy.Path;
            folderContentToCopy.Path = string.IsNullOrEmpty(folderToCopyTo.Path) ? folderToCopyTo.Name :
                $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";

            var pageOfFolderToCopyTo = _jsonManager.GetFolderPage(folderToCopyTo, folderToCopyTo.NextPageToWrite);
            _folderContentPageManager.AddToFolderPage(folderToCopyTo.NextPageToWrite, pageOfFolderToCopyTo, folderContentToCopy);


            //Fix the path in the folder content json file
            folderContentToCopy.Path = folderContentToCopyOldPath;
            folderContentToCopy = _jsonManager.GetFolderIfFolderType(folderContentToCopy);
            folderContentToCopy.Path = string.IsNullOrEmpty(folderToCopyTo.Path) ? folderToCopyTo.Name :
                                                                                   $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";
            _fileManager.WriteJson(_jsonManager.CreateJsonPath(folderContentToCopy.Name, folderContentToCopy.Path, folderContentToCopy.Type),
                                folderContentToCopy);

            switch (folderContentToCopy.Type)
            {
                //Fix the children path and move the directory
                case FolderContentType.Folder:
                    _directoryManager.DirectoryCopy(
                        _folderContentFolderManager.CreateFolderPath(folderContentToCopy.Name, folderContentToCopyOldPath),
                        $"{_folderContentFolderManager.CreateFolderPath(folderToCopyTo.Name, folderToCopyTo.Path)}\\{folderContentToCopy.Name}",
                        true);

               
                    _folderContentFolderManager.UpdateChildrenPath(folderContentToCopy,
                        $"{folderContentToCopy.Path}/{folderContentToCopy.Name}",
                        $"{folderContentToCopyOldPath}/{folderContentToCopy.Name}");
                    break;
                case FolderContentType.File:
                    _fileManager.Copy(
                        _folderContentFileManager.CreateFilePath(folderContentToCopy.Name, folderContentToCopyOldPath),
                        _folderContentFileManager.CreateFilePath(folderContentToCopy.Name, folderContentToCopy.Path));
                    break;
            }
        }

        public void CreateFile(string name, string path, string fileType, string[] value, long size)
        {
           _folderContentFileManager.CreateFile(name, path, fileType, value, size);
        }

        public Stream GetFile(string name, string path)
        {
            return _fileManager.GetFile(_folderContentFileManager.CreateFilePath(name, path));
        }

        public void DeleteFile(string name, string path, int page)
        {
            _folderContentFileManager.DeleteFile(name, path, page);
        }

        public int GetNumOfFolderPages(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            var folder = !_jsonManager.IsFolderContentExist(name, path, FolderContentType.Folder) ? 
                null : 
                _fileManager.ReadJson<RestFolderObj>(_jsonManager.CreateJsonPath(name, path, FolderContentType.Folder)).MapToIFolder();

            return folder?.NumOfPages ?? -1;;
        }

        public IFolderPage GetFolderPage(string name, string path, int page)
        {
            var folder = _jsonManager.GetFolder(name, path);
            return _jsonManager.GetFolderPage(folder, page);
        }
    }
}
