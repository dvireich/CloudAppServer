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
        private readonly JavaScriptSerializer _serializer;

        public FolderContentManager()
        {
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

            IFolder homeFolder = new FolderObj(_homeFolderName, string.Empty, null);
            WriteJson(homeFolder);
        }
        private bool IsFolderContentExist(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            return File.Exists(CreateJsonPath(name,path));
        }

        private string CreateJsonPath(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            return string.IsNullOrEmpty(path) ? $"{_baseFolderPath}\\{name}.json" : $"{_baseFolderPath}\\{path}\\{name}.json";
        }

        private string CreateFolderPath(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
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
            IFolder newFolder = new FolderObj(name, path, null);
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
            WriteJson(folder);
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

            if (parent.Content == null)
            {
                parent.Content = new IFolderContent[]{ folder };
            }
            else
            {
                var contentList = parent.Content.ToList();
                contentList.Add(folder);
                parent.Content = contentList.ToArray();
            }

            WriteJson(parent);
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
            WriteJson(parent);
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

        private T ReadJson<T>(string path)
        {
            path = path.ToLower();
            using (var jsonFile = File.OpenText(path))
            {
                var jsonText = jsonFile.ReadToEnd();
                var folderContent = _serializer.Deserialize<T>(jsonText);
                return folderContent;
            }
        }

        private void WriteJson(IFolderContent folder)
        {
            var serializedFolder = _serializer.Serialize(folder);
            var path = CreateJsonPath(folder.Name, folder.Path);
            File.Delete(path);
            using (var jsonFile = File.CreateText(path))
            {
                jsonFile.Write(serializedFolder);
            }
        }

        private IFolder GetFolder(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            return !IsFolderContentExist(name, path) ? null : ReadJson<RestFolderObj>(CreateJsonPath(name, path)).MapToIFolder();
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
            if (!IsFolderContentExist(name, path)) return;

            var folder = GetFolder(name, path);
            foreach (var folderContent in folder.Content)
            {
                DeleteFolder(folderContent.Name, folderContent.Path);
            }
            var pathToFolder = CreateJsonPath(folder.Name, folder.Path);
            File.Delete(pathToFolder);

            UpdateDeleteFolderInParentData(folder);
        }
    }
}
