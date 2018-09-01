using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CloudAppServer.Model;

namespace FolderContentManager
{
    public class FolderContentManager
    {
        private string _baseFolderPath = "C:\\foldercontentmanager";
        private readonly string _homeFolderName = "home";
        private readonly IoHelper _ioHelper;
        private readonly JavaScriptSerializer _serializer;


        public FolderContentManager()
        {
            _ioHelper = new IoHelper();
            _serializer = new JavaScriptSerializer();
            InitializeBaseFolder();
            InitializeHomeFolder();
        }

        private void InitializeBaseFolder()
        {
            if (Directory.Exists(_baseFolderPath)) return;
            Directory.CreateDirectory(_baseFolderPath);
        }

        private void InitializeHomeFolder()
        {
            var homeFolderPath = CreateFolderPath(_homeFolderName, string.Empty);
            if (Directory.Exists(homeFolderPath)) return;
            Directory.CreateDirectory(homeFolderPath);

            IFolder homeFolder = new FolderObj(_homeFolderName, string.Empty, new IFolderContent[0]);
            var path = CreateJsonPath(homeFolder.Name, homeFolder.Path, FolderContentType.Folder);
            _ioHelper.WriteJson(path, homeFolder);
        }
        private bool IsFolderContentExist(string name, string path, FolderContentType type)
        {
            name = name.ToLower();
            path = path.ToLower();
            return File.Exists(CreateJsonPath(name, path, type));
        }

        private string CreateJsonPath(string name, string path, FolderContentType type)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/','\\');
            return string.IsNullOrEmpty(path) ? $"{_baseFolderPath}\\{name}{type.ToString()}.json" : $"{_baseFolderPath}\\{path}\\{name}{type.ToString()}.json";
        }

        private string CreateFolderPath(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/', '\\');
            return string.IsNullOrEmpty(path) ? $"{_baseFolderPath}\\{name}" : $"{_baseFolderPath}\\{path}\\{name}";
        }

        private void ValidatePath(string path)
        {
            path = path.ToLower();
            const string errorPathEmpty = "Path cannot be empty! it is at least must have 'home/'!";
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path)) throw new ArgumentException(errorPathEmpty);
        }

        public void CreateFolder(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            if (path.ToCharArray().Last() == '/')
            {
                var listOfChars = path.ToCharArray().ToList();
                listOfChars.RemoveAt(listOfChars.Count-1);
                path = new string(listOfChars.ToArray());
            }
            ValidatePath(path);
            IFolder newFolder = new FolderObj(name, path, new IFolderContent[0]);
            ValidateName(newFolder);
            CreateFolder(newFolder);
        }

        private void ValidateName(IFolderContent newFolder)
        {
            var parent = GetParentFolder(newFolder);
            if (parent == null || parent.Content == null) return;
            if (parent.Content.Any(f => f.Name == newFolder.Name && f.Type == newFolder.Type))
            {
                throw new Exception("The name exists in this folder!");
            }
        }

        public void CreateFolder(IFolder folder)
        {
            CreateDirectoryIfNotExists(folder);
            var path = CreateJsonPath(folder.Name, folder.Path, FolderContentType.Folder);
            _ioHelper.WriteJson(path, folder);
            UpdateNewFolderInParentData(folder);
        }

        public void CreateDirectoryIfNotExists(IFolder folder)
        {
            var directoryPath = CreateFolderPath(folder.Name, folder.Path);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void UpdateNewFolderInParentData(IFolderContent folder)
        {
            var parent = GetParentFolder(folder);
            if (parent == null) return;

            var contentList = parent.Content.ToList();
            contentList.Add(folder);
            parent.Content = contentList.ToArray();
            var path = CreateJsonPath(parent.Name, parent.Path, parent.Type);
            _ioHelper.WriteJson(path, parent);
        }

        private void UpdateDeleteFolderInParentData(IFolderContent folder)
        {
            var parent = GetParentFolder(folder);

            if (parent.Content == null) return;

            var contentList = parent.Content.ToList();
            contentList.RemoveAll(fc => fc.Name == folder.Name &&
                                        fc.Path == folder.Path &&
                                        fc.Type == folder.Type);

            parent.Content = contentList.ToArray();
            var path = CreateJsonPath(parent.Name, parent.Path, parent.Type);
            _ioHelper.WriteJson(path, parent);
        }

        private void UpdateRenameInParentData(IFolderContent folderContent, string oldName)
        {
            var parent = GetParentFolder(folderContent);

            if (parent.Content == null) return;

            var contentList = parent.Content.ToList();
            var childContent = contentList.FirstOrDefault(fc => fc.Name == oldName &&
                                        fc.Path == folderContent.Path &&
                                        fc.Type == folderContent.Type);

            if (childContent == null) return;

            childContent.Name = folderContent.Name;

            parent.Content = contentList.ToArray();
            var path = CreateJsonPath(parent.Name, parent.Path, parent.Type);
            _ioHelper.WriteJson(path, parent);
        }

        private IFolder GetParentFolder(IFolderContent folder)
        {
            var parentName = GetParentName(folder);
            var parentPath = GetParentPath(folder);
            return GetFolder(parentName, parentPath);
        }

        private string GetParentPath(IFolderContent folder)
        {
            var path = folder.Path;

            if (string.IsNullOrEmpty(path)) return string.Empty;

            var spittedPathArr = path.Split('/').ToList();

            if (spittedPathArr.Count == 1) return string.Empty;

            spittedPathArr.RemoveAt(spittedPathArr.Count -1);
            return spittedPathArr.Aggregate((i, j) => i + '/' + j);
        }

        private string GetParentName(IFolderContent folder)
        {
            var path = folder.Path;

            if (string.IsNullOrEmpty(path)) return _homeFolderName;

            var spittedPathArr = path.Split('/').ToList();
            return spittedPathArr.Last();
        }

        

        private IFolder GetFolder(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            return !IsFolderContentExist(name, path, FolderContentType.Folder) ? null : _ioHelper.ReadJson<RestFolderObj>(CreateJsonPath(name, path, FolderContentType.Folder)).MapToIFolder();
        }

        private IFolderContent GetFolderContent(string name, string path, FolderContentType type)
        {
            return !IsFolderContentExist(name, path, type) ? null : _ioHelper.ReadJson<RestFolderContent>(CreateJsonPath(name, path, type)).MapToIFolderContent();
        }

        public string GetFolderAsJson(string name, string path)
        {
            name = name.Replace("\"", "");
            path = path.Replace("\"", "");
            var folder = GetFolder(name, path);
            if (folder == null) return null;
            var serializeFolder = _serializer.Serialize(folder);
            return serializeFolder;
        }

        public void DeleteFolder(string name, string path)
        {
            if (!IsFolderContentExist(name, path, FolderContentType.Folder)) return;

            var folder = GetFolder(name, path);
            foreach (var folderContent in folder.Content)
            {
                DeleteFolder(folderContent.Name, folderContent.Path);
            }
            var pathToFolderJson = CreateJsonPath(folder.Name, folder.Path, folder.Type);
            File.Delete(pathToFolderJson);


            var pathToFolder = CreateFolderPath(folder.Name, folder.Path);
            if (Directory.Exists(pathToFolder))
            {
                Directory.Delete(pathToFolder,true);
            }

            UpdateDeleteFolderInParentData(folder);
        }

        private void UpdateChildrenPath(IFolderContent folderContent, string newPathPrefix, string oldPathPrefix)
        {
            if (folderContent.Type != FolderContentType.Folder) return;
            var folder = GetFolder(folderContent.Name, folderContent.Path);

            foreach (var t in folder.Content)
            {
                ChangePathOnRenameInParentContentList(t, oldPathPrefix, newPathPrefix);
                ChangePathOnRenameInChildData(t, oldPathPrefix, newPathPrefix);
            }
            var folderPath = CreateJsonPath(folder.Name, folder.Path, folder.Type);
            _ioHelper.WriteJson(folderPath, folder);
        }

        private void ChangePathOnRenameInParentContentList(IFolderContent fc, string oldPathPrefix, string newPathPrefix)
        {
            if (!fc.Path.StartsWith(oldPathPrefix)) return;
            var newPath = ReplacePrefixString(fc.Path, oldPathPrefix, newPathPrefix);
            fc.Path = newPath;
        }

        private void ChangePathOnRenameInChildData(IFolderContent t, string oldPathPrefix, string newPathPrefix)
        {
            var fc = GetFolderIfFolderType(t);
            fc.Path = ReplacePrefixString(fc.Path, oldPathPrefix, newPathPrefix);
            var dirPath = CreateJsonPath(fc.Name, fc.Path, fc.Type);
            _ioHelper.WriteJson(dirPath, fc);
            UpdateChildrenPath(fc, newPathPrefix, oldPathPrefix);
        }

        private IFolderContent GetFolderIfFolderType(IFolderContent folderContent)
        {
            return folderContent.Type == FolderContentType.Folder ? GetFolder(folderContent.Name, folderContent.Path) : folderContent;
        }

        private string ReplacePrefixString(string source, string oldPrefix, string newPrefix)
        {
            if (!source.StartsWith(oldPrefix)) return oldPrefix;

            var suffixPath = source.Substring(oldPrefix.Length, source.Length - oldPrefix.Length);
            return $"{newPrefix}{suffixPath}";

        }
        public void Rename(string name, string path, string typeStr, string newName)
        {
            Enum.TryParse(typeStr, true, out FolderContentType type);
            var folderContent = GetFolderContent(name, path, type);
            if(folderContent == null) throw new Exception("folder content is not exists!");
            var oldName = folderContent.Name;
            var oldPath = $"{folderContent.Path}/{oldName}";
            var newPath = $"{folderContent.Path}/{newName}";

            folderContent = GetFolderIfFolderType(folderContent);
            folderContent.Name = newName;
            RenameDirectory(folderContent.Path, oldName, newName);
            var dirPath = CreateJsonPath(folderContent.Name, folderContent.Path, folderContent.Type);
            _ioHelper.WriteJson(dirPath, folderContent);
            File.Delete(CreateJsonPath(oldName, folderContent.Path, folderContent.Type));
            UpdateRenameInParentData(folderContent, oldName);

            
            UpdateChildrenPath(folderContent, newPath, oldPath);
        }

        private void RenameDirectory(string path, string oldName, string newName)
        {
            var newDirPath = CreateFolderPath(newName, path);
            var oldDirPath = CreateFolderPath(oldName, path);

            _ioHelper.DirectoryCopy(oldDirPath, newDirPath, true);
            Directory.Delete(oldDirPath,true);
        }

        public void Copy(string copyObjName, string copyObjPath, string copyObjTypeStr, string copyToName,
            string copyToPath)
        {
            var folderToCopyTo = GetFolder(copyToName, copyToPath);
            if(folderToCopyTo == null) throw new Exception("The folder you are trying to copy to does not exists!");

            Enum.TryParse(copyObjTypeStr, true, out FolderContentType copyObjType);

            var folderContentToCopy = GetFolderContent(copyObjName, copyObjPath, copyObjType);
            if(folderContentToCopy == null) throw new Exception("The folder content you are trying to copy does not exists!");

            var contentList = folderToCopyTo.Content.ToList();
            contentList.Add(folderContentToCopy);
            folderToCopyTo.Content = contentList.ToArray();

            _ioHelper.WriteJson(CreateJsonPath(folderToCopyTo.Name, folderToCopyTo.Path, folderToCopyTo.Type),
                                folderToCopyTo);

            folderContentToCopy = GetFolderIfFolderType(folderContentToCopy);
            var folderContentToCopyOldPath = folderContentToCopy.Path;
            folderContentToCopy.Path = string.IsNullOrEmpty(folderToCopyTo.Path) ? folderToCopyTo.Name :
                                                                                   $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";
            _ioHelper.WriteJson(CreateJsonPath(folderContentToCopy.Name, folderContentToCopy.Path, folderContentToCopy.Type),
                                folderContentToCopy);

            if (folderContentToCopy.Type == FolderContentType.Folder)
            {
                _ioHelper.DirectoryCopy(CreateFolderPath(folderContentToCopy.Name, folderContentToCopyOldPath),
                                        $"{CreateFolderPath(folderToCopyTo.Name, folderToCopyTo.Path)}\\{folderContentToCopy.Name}",
                                        true);

                UpdateChildrenPath(folderContentToCopy, folderContentToCopy.Path, folderContentToCopyOldPath);
            }

        }
    }
}
