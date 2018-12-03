using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentManager.Helpers;
using FolderContentManager.Model;
using FolderContentManager.Repositories;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentManager.Services
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class FolderContentFolderService : IFolderContentFolderService
    {
        private readonly IFolderContentFolderRepository _folderContentFolderRepository;
        private readonly IFolderContentPageService _folderContentPageService;
        private readonly IFolderContentFileService _folderContentFileService;
        private readonly IConstance _constance;

        public FolderContentFolderService(IConstance constance)
        {
            _folderContentFolderRepository = new FolderContentFolderRepository(constance);
            _folderContentPageService = new FolderContentPageService(constance);
            _folderContentFileService = new FolderContentFileService(constance, this);
            this._constance = constance;
        }

        public FolderContentFolderService(IConstance constance, IFolderContentFileService folderContentFileService)
        {
            _folderContentFolderRepository = new FolderContentFolderRepository(constance);
            _folderContentPageService = new FolderContentPageService(constance);
            _folderContentFileService = folderContentFileService;
            this._constance = constance;
        }

        public IFolder GetParentFolder(IFolderContent folder)
        {
            if (folder.Name == _constance.HomeFolderName && folder.Path == _constance.HomeFolderPath) return null;

            var parentName = GetParentName(folder);
            var parentPath = GetParentPath(folder);
            return _folderContentFolderRepository.GetFolder(parentName, parentPath);
        }

        public IFolder GetFolder(string name, string path)
        {
            return _folderContentFolderRepository.GetFolder(name, path);
        }

        public void CreateFolder(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower();
            if (!string.IsNullOrEmpty(path) && path.ToCharArray().Last() == '/')
            {
                var listOfChars = path.ToCharArray().ToList();
                listOfChars.RemoveAt(listOfChars.Count - 1);
                path = new string(listOfChars.ToArray());
            }
            ValidatePath(name, path);
            IFolder newFolder = new FolderObj(name, path, _constance.DefaultNumberOfElementOnPage);
            var parent = GetParentFolder(newFolder);
            _folderContentPageService.ValidateUniquenessOnAllFolderPages(parent, newFolder);
            CreateFolder(newFolder);
        }

        public void DeleteFolder(string name, string path, int page)
        {
            var folder = _folderContentFolderRepository.GetFolder(name, path);
            if (folder == null) return;

            for (var i = 1; i <= folder.NumOfPhysicalPages; i++)
            {
                var folderPage = _folderContentPageService.GetPhysicalFolderPage(folder, i);
                foreach (var folderContent in folderPage.Content)
                {
                    DeleteFolder(folderContent.Name, folderContent.Path, page);
                }
                _folderContentFolderRepository.DeleteFolder(folder.Name, folder.Path);
                _folderContentPageService.DeletePage(folder, i);
            }

            _folderContentFolderRepository.DeleteDirectory(folder.Name, folder.Path);

            var parent = GetParentFolder(folder);
            _folderContentPageService.RemoveFolderContentFromPage(parent, folder, page);
        }

        public void RenameDirectory(string path, string oldName, string newName)
        {
            _folderContentFolderRepository.CopyDirectory(newName, path, oldName, path);
            _folderContentFolderRepository.DeleteDirectory(oldName, path);
        }

        public void UpdateFolderChildrenPath(IFolderContent folderContent, string newPathPrefix, string oldPathPrefix)
        {
            if (folderContent.Type != FolderContentType.Folder) return;
            var folder = _folderContentFolderRepository.GetFolder(folderContent.Name, folderContent.Path);

            if (folder.Path.StartsWith(oldPathPrefix))
            {
                folder.Path = folder.Path.ReplacePrefixInString(oldPathPrefix, newPathPrefix);
            }

            _folderContentFolderRepository.CreateOrUpdateFolder(folder.Name, folder.Path, folder);

            for (var i = 1; i <= folder.NumOfPhysicalPages; i++)
            {
                _folderContentPageService.UpdatePathOnPage(folder, i, oldPathPrefix, newPathPrefix);
                var page = _folderContentPageService.GetPhysicalFolderPage(folder, i);

                foreach (var fc in page.Content)
                {
                    if (fc.Type == FolderContentType.File)
                    {
                        _folderContentFileService.UpdateFilePrefixPath(fc.Name, fc.Path, newPathPrefix, oldPathPrefix);
                    }
                    UpdateFolderChildrenPath(fc, newPathPrefix, oldPathPrefix);
                }
            }
        }

        public void UpdateFolderMetaData(FolderMetadata folderMetadata)
        {
            var folder = _folderContentFolderRepository.GetFolder(folderMetadata.Name, folderMetadata.Path);
            folder.SortType = folderMetadata.SortType;
            folder.NumberOfElementPerPage = folderMetadata.NumberOfPagesPerPage;
            _folderContentFolderRepository.CreateOrUpdateFolder(folderMetadata.Name, folderMetadata.Path, folder);
        }

        public void UpdateNextPageToWrite(IFolder folder)
        {
            folder.NextPhysicalPageToWrite = _folderContentPageService.GetNextAvailablePageToWrite(folder);
            if (folder.NextPhysicalPageToWrite > folder.NumOfPhysicalPages)
            {
                folder.NumOfPhysicalPages++;
                _folderContentPageService.AddNewFolderPageToFolder(folder.NextPhysicalPageToWrite, folder);
            }
            _folderContentFolderRepository.CreateOrUpdateFolder(folder.Name, folder.Path, folder);
        }

        public void CreateDirectory(string name, string path)
        {
            _folderContentFolderRepository.CreateDirectory(name, path);
        }

        public void UpdateFolder(IFolder folder)
        {
            _folderContentFolderRepository.CreateOrUpdateFolder(folder.Name, folder.Path, folder);
        }

        public void RenameFolder(string name, string newName, string path)
        {
            var folder = GetFolder(name, path);
            if (folder == null) throw new Exception("folder does not exists!");

            var oldName = folder.Name;
            var oldPath = $"{folder.Path}/{oldName}";
            var newPath = $"{folder.Path}/{newName}";
            _folderContentFolderRepository.CopyDirectory(newName, path, oldName, path);
            _folderContentPageService.MovePagesToNewLocation(folder, oldName, folder.Path, newName, folder.Path);
            folder.Name = newName;
            folder.ModificationTime = $"{DateTime.Now:G}";
            UpdateFolder(folder);
            _folderContentFolderRepository.DeleteFolder(name, path);
            _folderContentFolderRepository.DeleteDirectory(oldName, path);
            var parent = GetParentFolder(folder);
            _folderContentPageService.RenameFolderContentInFolderPages(parent, name, newName, folder);
            UpdateFolderChildrenPath(folder, newPath, oldPath);
        }

        public void Copy(string copyFromName, string copyFromPath, string copyToName, string copyToPath)
        {
            
            var folderToCopyTo = GetFolder(copyToName, copyToPath);
            if (folderToCopyTo == null)
            {
                throw new Exception("The folder you are trying to copy to does not exists!");
            }
            
            UpdateNextPageToWrite(folderToCopyTo);

            var folderToCopy = GetFolder(copyFromName, copyFromPath);
            if (folderToCopy == null)
            {
                throw new Exception("The folder you are trying to copy does not exists!");
            }
            
            _folderContentPageService.ValidateUniquenessOnAllFolderPages(folderToCopyTo, folderToCopy);
            var copyFromNewPath = string.IsNullOrEmpty(folderToCopyTo.Path) ? folderToCopyTo.Name : $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";
            var oldPath = $"{folderToCopy.Path}/{folderToCopy.Name}";
            var newPath = $"{copyFromNewPath}/{folderToCopy.Name}";
            _folderContentFolderRepository.CopyDirectory(copyFromName, copyFromNewPath, copyFromName, copyFromPath);
            _folderContentPageService.CopyPagesToNewLocation(folderToCopy, folderToCopy.Name , copyFromPath, folderToCopy.Name, copyFromNewPath);
            folderToCopy.Path = copyFromNewPath;
            UpdateFolder(folderToCopy);
            _folderContentPageService.AddToFolderPage(folderToCopyTo, folderToCopyTo.NextPhysicalPageToWrite, folderToCopy);
            UpdateFolderChildrenPath(folderToCopy, newPath, oldPath);
        }

        private void CreateFolder(IFolder folder)
        {
            _folderContentFolderRepository.CreateDirectory(folder.Name, folder.Path);
            _folderContentFolderRepository.CreateOrUpdateFolder(folder.Name, folder.Path, folder);
            _folderContentPageService.AddNewFolderPageToFolder(1, folder);
            UpdateNewFolderInParentData(folder);
        }

        private void UpdateNewFolderInParentData(IFolderContent folder)
        {
            var parent = GetParentFolder(folder);
            if (parent == null) return;
            UpdateNextPageToWrite(parent);
            _folderContentPageService.AddToFolderPage(parent, parent.NextPhysicalPageToWrite, folder);
        }

        private void ValidatePath(string name, string path)
        {
            path = path.ToLower();
            if (name == _constance.HomeFolderName && string.IsNullOrEmpty(path)) return;

            const string errorPathEmpty = "Path cannot be empty! it is at least must have 'home/'!";
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path)) throw new ArgumentException(errorPathEmpty);
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
    }
}
