using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CloudAppServer.Model;
using FolderContentManager.Model;
using FolderContentManager.Model.RestObject;

namespace FolderContentManager
{
    public class FolderContentManager
    {
        private string _baseFolderPath = "C:\\foldercontentmanager";
        private readonly string _homeFolderName = "home";
        private readonly IoHelper _ioHelper;

        private readonly int _maxFolderContentOnPage = 10;

        #region Singelton
        private static FolderContentManager _instance = null;
        private static readonly object Padlock = new object();
        private FolderContentManager()
        {
            _ioHelper = new IoHelper();
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
            if (Directory.Exists(_baseFolderPath)) return;
            Directory.CreateDirectory(_baseFolderPath);
        }

        private void InitializeHomeFolder()
        {
            var homeFolderPath = CreateFolderPath(_homeFolderName, string.Empty);
            if (Directory.Exists(homeFolderPath)) return;
            Directory.CreateDirectory(homeFolderPath);

            IFolder homeFolder = new FolderObj(_homeFolderName, string.Empty);
            var jsonPath = CreateJsonPath(homeFolder.Name, homeFolder.Path, FolderContentType.Folder);
            _ioHelper.WriteJson(jsonPath, homeFolder);

            CreateNewFolderPage(1, homeFolder);
        }
        private bool IsFolderContentExist(string name, string path, FolderContentType type)
        {
            name = name.ToLower();
            path = path.ToLower();
            return File.Exists(CreateJsonPath(name, path, type));
        }

        private bool IsFolderPageExist(string name, string path, int page)
        {
            name = name.ToLower();
            path = path.ToLower();
            return File.Exists(CreateFolderPageJsonPath(name, path, page));
        }

        private string CreateJsonPath(string name, string path, FolderContentType type)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/','\\');
            return string.IsNullOrEmpty(path) ? $"{_baseFolderPath}\\{name}{type.ToString()}.json" : $"{_baseFolderPath}\\{path}\\{name}{type.ToString()}.json";
        }

        private string CreateFolderPageJsonPath(string name, string path, int page)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/', '\\');
            return string.IsNullOrEmpty(path) ? 
                $"{_baseFolderPath}\\{name}{FolderContentType.FolderPage.ToString()}.{page}.json" : 
                $"{_baseFolderPath}\\{path}\\{name}{FolderContentType.FolderPage.ToString()}.{page}.json";
        }


        private string CreateFilePath(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/', '\\');
            return string.IsNullOrEmpty(path) ? $"{_baseFolderPath}\\{name}" : $"{_baseFolderPath}\\{path}\\{name}";
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
            IFolder newFolder = new FolderObj(name, path);
            ValidateName(newFolder);
            CreateFolder(newFolder);
        }

        private void ValidateName(IFolderContent newFolder)
        {
            var parent = GetParentFolder(newFolder);
            if (parent == null) return;

            for (var i = 1; i <= parent.NumOfPages; i++)
            {
                var page = GetFolderPage(parent, i);
                if (page.Content.Any(f => f.Name == newFolder.Name && f.Type == newFolder.Type))
                {
                    throw new Exception("The name exists in this folder!");
                }
            }
        }

        public void CreateFolder(IFolder folder)
        {
            CreateDirectoryIfNotExists(folder);
            var jsonPath = CreateJsonPath(folder.Name, folder.Path, FolderContentType.Folder);
            _ioHelper.WriteJson(jsonPath, folder);

            CreateNewFolderPage(1, folder);
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

        private void AddToFolderPage(int pageNum ,IFolderPage pageToWriteContent, IFolderContent folderContent)
        {
            var contentList = pageToWriteContent.Content.ToList();
            contentList.Add(folderContent);
            pageToWriteContent.Content = contentList.ToArray();

            var jsonPagePath = CreateFolderPageJsonPath(pageToWriteContent.Name, pageToWriteContent.Path, pageNum);
            _ioHelper.WriteJson(jsonPagePath, pageToWriteContent);
        }

        private void CreateNewFolderPage(int pageNum, IFolder folder)
        {
            IFolderPage page = new FolderPage(folder.Name, folder.Path, new IFolderContent[0]);
            var jsonPagePath = CreateFolderPageJsonPath(folder.Name, folder.Path, pageNum);
            _ioHelper.WriteJson(jsonPagePath, page);
        }


        private void UpdateNewFolderInParentData(IFolderContent folder)
        {
            var parent = GetParentFolder(folder);
            if (parent == null) return;

            UpdateNextPageToWrite(parent);

            var pageToWriteContent = GetFolderPage(parent, parent.NextPageToWrite);
            AddToFolderPage(parent.NextPageToWrite, pageToWriteContent, folder);
        }

        private int GetNextPageToWrite(IFolder folder)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var page = GetFolderPage(folder, i);
                if (page.Content.Length < _maxFolderContentOnPage) return i;
            }

            return folder.NumOfPages + 1;
        }

        private void UpdateNextPageToWrite(IFolder folder)
        {
            folder.NextPageToWrite = GetNextPageToWrite(folder);
            if (folder.NextPageToWrite > folder.NumOfPages)
            {
                folder.NumOfPages++;
                CreateNewFolderPage(folder.NextPageToWrite, folder);
            }
            var path = CreateJsonPath(folder.Name, folder.Path, folder.Type);
            _ioHelper.WriteJson(path, folder);
        }

        private void UpdateDeleteFolderContentInParentData(IFolderContent folder, int page)
        {
            var parent = GetParentFolder(folder);
            var folderPage = GetFolderPage(parent, page);

            if (folderPage.Content == null) return;

            var contentList = folderPage.Content.ToList();
            contentList.RemoveAll(fc => fc.Name == folder.Name &&
                                        fc.Path == folder.Path &&
                                        fc.Type == folder.Type);

            folderPage.Content = contentList.ToArray();
            var pathToPage = CreateFolderPageJsonPath(parent.Name, parent.Path, page);
            _ioHelper.WriteJson(pathToPage, folderPage);

            PerformPageCompression(parent);
            //UpdateNextPageToWrite(parent);
        }

        private void UpdateRenameInParentData(IFolderContent folderContent, string oldName)
        {
            var parent = GetParentFolder(folderContent);

            for (var i = 1; i <= parent.NumOfPages; i++)
            {
                var page = GetFolderPage(parent, i);
                if (page.Content == null) continue;

                var contentList = page.Content.ToList();
                var childContent = contentList.FirstOrDefault(fc => fc.Name == oldName &&
                                                                    fc.Path == folderContent.Path &&
                                                                    fc.Type == folderContent.Type);

                if (childContent == null) continue;

                childContent.Name = folderContent.Name;

                page.Content = contentList.ToArray();
                var path = CreateFolderPageJsonPath(parent.Name, parent.Path, i);
                _ioHelper.WriteJson(path, page);
            }
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

        

        public IFolder GetFolder(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            return !IsFolderContentExist(name, path, FolderContentType.Folder) ? null : _ioHelper.ReadJson<RestFolderObj>(CreateJsonPath(name, path, FolderContentType.Folder)).MapToIFolder();
        }

        private IFolderContent GetFolderContent(string name, string path, FolderContentType type)
        {
            return !IsFolderContentExist(name, path, type) ? null : _ioHelper.ReadJson<RestFolderContent>(CreateJsonPath(name, path, type)).MapToIFolderContent();
        }

        private IFolderPage GetFolderPage(IFolder folder, int page)
        {
            return !IsFolderPageExist(folder.Name, folder.Path, page) ? 
                null : 
                _ioHelper.ReadJson<RestFolderPageObj>(CreateFolderPageJsonPath(folder.Name, folder.Path, page)).MapToIFolderPage();
        }

        public IFolderPage GetFolderPage(string name, string path, int page)
        {
            var folder = GetFolder(name, path);

            //Return the highest page that exists
            page = page < folder.NumOfPages ? page : folder.NumOfPages;

            var folderPage = GetFolderPage(folder, page);
            return folderPage;
        }

        public void DeleteFolder(string name, string path, int page)
        {
            if (!IsFolderContentExist(name, path, FolderContentType.Folder)) return;

            var folder = GetFolder(name, path);
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var folderPage = GetFolderPage(folder, i);
                foreach (var folderContent in folderPage.Content)
                {
                    DeleteFolder(folderContent.Name, folderContent.Path, page);
                }
                var pathToFolderJson = CreateJsonPath(folder.Name, folder.Path, folder.Type);
                File.Delete(pathToFolderJson);
                var pathToPage = CreateFolderPageJsonPath(folderPage.Name, folderPage.Path, i);
                File.Delete(pathToPage);
            }
            
            var pathToFolder = CreateFolderPath(folder.Name, folder.Path);
            if (Directory.Exists(pathToFolder))
            {
                Directory.Delete(pathToFolder,true);
            }

            UpdateDeleteFolderContentInParentData(folder, page);
        }

        private void UpdateChildrenPath(IFolderContent folderContent, string newPathPrefix, string oldPathPrefix)
        {
            if (folderContent.Type != FolderContentType.Folder) return;
            var folder = GetFolder(folderContent.Name, folderContent.Path);

            if (folder.Path.StartsWith(oldPathPrefix))
            {
                var newPath = ReplacePrefixString(folder.Path, oldPathPrefix, newPathPrefix);
                folder.Path = newPath;
            }

            var folderPath = CreateJsonPath(folder.Name, folder.Path, folder.Type);
            _ioHelper.WriteJson(folderPath, folder);

            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var page = GetFolderPage(folder, i);

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

                var pathToPage = CreateFolderPageJsonPath(page.Name, page.Path, i);
                _ioHelper.WriteJson(pathToPage, page);

                foreach (var fc in page.Content)
                {
                    UpdateChildrenPath(fc, newPathPrefix, oldPathPrefix);
                }
            }
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

            if (folderContent.Type == FolderContentType.Folder)
            {
                RenameDirectory(folderContent.Path, oldName, newName);
                RenameFolderPagesJsonFiles(folderContent as IFolder, oldName, newName);
                RenameFolderPageInternalName(folderContent as IFolder, newName);
            }

            if (folderContent.Type == FolderContentType.File)
            {
                var oldNameFullPath = CreateFilePath(oldName, folderContent.Path);
                var newNameFullPath = CreateFilePath(newName, folderContent.Path);
                File.Move(oldNameFullPath, newNameFullPath);
            }
            
            var dirPath = CreateJsonPath(folderContent.Name, folderContent.Path, folderContent.Type);
            _ioHelper.WriteJson(dirPath, folderContent);
            File.Delete(CreateJsonPath(oldName, folderContent.Path, folderContent.Type));



            UpdateRenameInParentData(folderContent, oldName);

            UpdateChildrenPath(folderContent, newPath, oldPath);
        }

        private void RenameFolderPagesJsonFiles(IFolder folder,string oldName, string newName)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var oldNameFullPath = CreateFolderPageJsonPath(oldName, folder.Path, i);
                var newNameFullPath = CreateFolderPageJsonPath(newName, folder.Path, i);
                File.Move(oldNameFullPath, newNameFullPath);
                File.Delete(oldNameFullPath);
            }
        }

        private void RenameFolderPageInternalName(IFolder folder, string newName)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var page = GetFolderPage(folder, i);
                page.Name = newName;
                var path = CreateFolderPageJsonPath(folder.Name, folder.Path, i);
                _ioHelper.WriteJson(path, page);
            }
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
            //Validate folder to copy to
            var folderToCopyTo = GetFolder(copyToName, copyToPath);
            UpdateNextPageToWrite(folderToCopyTo);
            if(folderToCopyTo == null) throw new Exception("The folder you are trying to copy to does not exists!");

            //Validate folder to copy
            Enum.TryParse(copyObjTypeStr, true, out FolderContentType copyObjType);
            var folderContentToCopy = GetFolderContent(copyObjName, copyObjPath, copyObjType);
            if(folderContentToCopy == null) throw new Exception("The folder content you are trying to copy does not exists!");

            //Add the new folder content and fix the path
            var pageOfFolderToCopyTo = GetFolderPage(folderToCopyTo, folderToCopyTo.NextPageToWrite);
            var contentList = pageOfFolderToCopyTo.Content.ToList();
            var folderContentToCopyOldPath = folderContentToCopy.Path;
            folderContentToCopy.Path = string.IsNullOrEmpty(folderToCopyTo.Path) ? folderToCopyTo.Name :
                                       $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";
            contentList.Add(folderContentToCopy);

            pageOfFolderToCopyTo.Content = contentList.ToArray();
            _ioHelper.WriteJson(CreateFolderPageJsonPath(pageOfFolderToCopyTo.Name, pageOfFolderToCopyTo.Path, folderToCopyTo.NextPageToWrite),
                                pageOfFolderToCopyTo);

            //UpdateNextPageToWrite(folderToCopyTo);

            //Fix the path in the folder content json file
            folderContentToCopy.Path = folderContentToCopyOldPath;
            folderContentToCopy = GetFolderIfFolderType(folderContentToCopy);
            folderContentToCopy.Path = string.IsNullOrEmpty(folderToCopyTo.Path) ? folderToCopyTo.Name :
                                                                                   $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";
            _ioHelper.WriteJson(CreateJsonPath(folderContentToCopy.Name, folderContentToCopy.Path, folderContentToCopy.Type),
                                folderContentToCopy);

            //Fix the children path and move the directory
            if (folderContentToCopy.Type == FolderContentType.Folder)
            {
                _ioHelper.DirectoryCopy(CreateFolderPath(folderContentToCopy.Name, folderContentToCopyOldPath),
                                        $"{CreateFolderPath(folderToCopyTo.Name, folderToCopyTo.Path)}\\{folderContentToCopy.Name}",
                                        true);

               
                UpdateChildrenPath(folderContentToCopy,
                                   $"{folderContentToCopy.Path}/{folderContentToCopy.Name}",
                                   $"{folderContentToCopyOldPath}/{folderContentToCopy.Name}");
            }

            if (folderContentToCopy.Type == FolderContentType.File)
            {
                File.Copy(CreateFilePath(folderContentToCopy.Name, folderContentToCopyOldPath),
                          CreateFilePath(folderContentToCopy.Name, folderContentToCopy.Path));
            }
        }

        public void CreateFile(string name, string path, string fileType, string[] value)
        {
            var file = new FolderContent(name, path, FolderContentType.File);

            ValidateName(file);

            _ioHelper.WriteJson(CreateJsonPath(name, path, FolderContentType.File),
                                file);

            _ioHelper.WriteFileContent(CreateFilePath(name, path),
                                       value);

            var parent = GetParentFolder(file);

            UpdateNextPageToWrite(parent);

            var page = GetFolderPage(parent, parent.NextPageToWrite);
            var parentContent = page.Content.ToList();
            parentContent.Add(file);
            page.Content = parentContent.ToArray();

            _ioHelper.WriteJson(CreateFolderPageJsonPath(page.Name, page.Path, parent.NextPageToWrite),
                                                         page);
        }

        public Stream GetFile(string name, string path)
        {
            return _ioHelper.GetFile(CreateFilePath(name, path));
        }

        public void DeleteFile(string name, string path, int page)
        {
            if (!IsFolderContentExist(name, path, FolderContentType.File)) return;

            var file = GetFolderContent(name, path, FolderContentType.File);
            
            var pathToFolderJson = CreateJsonPath(file.Name, file.Path, file.Type);
            File.Delete(pathToFolderJson);

            var pathToFile = CreateFilePath(file.Name, file.Path);
            File.Delete(pathToFile);

            UpdateDeleteFolderContentInParentData(file, page);
        }

        private void PerformPageCompression(IFolder folder)
        {
            for (var i = 1; i <= folder.NumOfPages; i++)
            {
                var page = GetFolderPage(folder, i);
                if(page.Content.Length == _maxFolderContentOnPage) continue;

                //Delete empty pages
                if (page.Content.Length == 0)
                {
                    var pageToDeletePath = CreateFolderPageJsonPath(page.Name, page.Path, i);
                    File.Delete(pageToDeletePath);
                    folder.NumOfPages--;
                    continue;
                }

                //This is the last page and it is not full -> its ok
                if(i == folder.NumOfPages) continue;

                var nextPage = GetFolderPage(folder, i + 1);
                var numOfElementsToMove = _maxFolderContentOnPage - page.Content.Length;

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
                var nextPagePath = CreateFolderPageJsonPath(nextPage.Name, nextPage.Path, i + 1);
                _ioHelper.WriteJson(nextPagePath, nextPage);

                //Save the current page
                var pagePath = CreateFolderPageJsonPath(page.Name, page.Path, i);
                _ioHelper.WriteJson(pagePath, page);
            }

            //Save changes in the num of pages
            var pathToFolder = CreateJsonPath(folder.Name, folder.Path, folder.Type);
            _ioHelper.WriteJson(pathToFolder, folder);
        }
    }
}
