using System;
using System.Collections.Generic;
using System.IO;
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
    public class FolderContentFileService : IFolderContentFileService
    {
        private readonly IFolderContentFileRepository _folderContentFileRepository;
        private readonly IFolderContentFolderService _folderContentFolderService;
        private readonly IFolderContentPageService _folderContentPageService;

        public FolderContentFileService(IConstance constance)
        {
            _folderContentPageService = new FolderContentPageService(constance);
            _folderContentFolderService = new FolderContentFolderService(constance, this);
            _folderContentFileRepository = new FolderContentFileRepository(constance);
        }

        public FolderContentFileService(IConstance constance, IFolderContentFolderService folderContentFolderService)
        {
            _folderContentFolderService = folderContentFolderService;
            _folderContentPageService = new FolderContentPageService(constance);
            _folderContentFileRepository = new FolderContentFileRepository(constance);
        }

        public void CreateFile(string name, string path, string fileType, string tmpCreationPath, long size)
        {
            var file = new FileObj(name, path, fileType, size);
            var parent = _folderContentFolderService.GetParentFolder(file);

            //Validate that the file is unique in all pages. If the file is unique then save it
            _folderContentPageService.ValidateUniquenessOnAllFolderPages(parent, file);
            _folderContentFileRepository.CreateOrUpdateFolderContentFile(name, path, file);
            
            _folderContentFileRepository.Move(name, path, tmpCreationPath);
            _folderContentFolderService.UpdateNextPageToWrite(parent);
            _folderContentPageService.AddToFolderPage(parent, parent.NextPhysicalPageToWrite, file);
        }

        public void DeleteFile(string name, string path, int page)
        {
            var file = _folderContentFileRepository.GetFolderContentFile(name, path);
            if(file == null) return;;

            _folderContentFileRepository.Delete(name, path);

            var parent = _folderContentFolderService.GetParentFolder(file);
            _folderContentPageService.RemoveFolderContentFromPage(parent, file, page);
        }

        public Stream GetFile(string name, string path)
        {
            return _folderContentFileRepository.GetFile(name, path);
        }

        public IFile GetFolderContentFile(string name, string path)
        {
            return _folderContentFileRepository.GetFolderContentFile(name, path);
        }

        public void MoveFileToNewLocation(string sourceName, string sourcePath, string destName, string destPath)
        {
            var sourceFolderContentFile = _folderContentFileRepository.GetFolderContentFile(sourceName, sourcePath);
            _folderContentFileRepository.CreateOrUpdateFolderContentFile(destName, destPath, sourceFolderContentFile);
            _folderContentFileRepository.Move(destName, destPath, sourceName, sourcePath);
            _folderContentFileRepository.Delete(sourceName, sourcePath);
        }

        public void UpdateFolderContentFile(IFile file)
        {
            _folderContentFileRepository.CreateOrUpdateFolderContentFile(file.Name, file.Path, file);
        }

        public void RenameFile(string oldName, string newName, string path)
        {
            var folderContentFile = GetFolderContentFile(oldName, path);
            if (folderContentFile == null) throw new Exception("file does not exists!");
            ValidateFileNewNameInParentData(folderContentFile, newName);
            folderContentFile.Name = newName;
            folderContentFile.ModificationTime = $"{DateTime.Now:G}";
            UpdateFolderContentFile(folderContentFile);
            MoveFileToNewLocation(oldName, path, newName, path);
            var folder = _folderContentFolderService.GetParentFolder(folderContentFile);
            _folderContentPageService.RenameFolderContentInFolderPages(folder, oldName, newName, folderContentFile);
        }

        public void Copy(string copyFromName, string copyFromPath, string copyToName, string copyToPath)
        {
            var folderToCopyTo = _folderContentFolderService.GetFolder(copyToName, copyToPath);
            if (folderToCopyTo == null)
            {
                throw new Exception("The folder you are trying to copy to does not exists!");
            }

            _folderContentFolderService.UpdateNextPageToWrite(folderToCopyTo);

            var fileToCopy = GetFolderContentFile(copyFromName, copyFromPath);
            if (fileToCopy == null)
            {
                throw new Exception("The file you are trying to copy does not exists!");
            }
            var copyFromNewPath = string.IsNullOrEmpty(folderToCopyTo.Path) ? folderToCopyTo.Name : $"{folderToCopyTo.Path}/{folderToCopyTo.Name}";
            fileToCopy.Path = copyFromNewPath;
            _folderContentPageService.AddToFolderPage(folderToCopyTo, folderToCopyTo.NextPhysicalPageToWrite, fileToCopy);
            UpdateFolderContentFile(fileToCopy);
            _folderContentFileRepository.Copy(copyFromName, copyFromNewPath, copyFromName, copyFromPath);
        }

        public void UpdateFilePrefixPath(string fileName, string filePath, string newPathPrefix, string oldPathPrefix)
        {
            var folderContentFile = GetFolderContentFile(fileName, filePath);
            if (!folderContentFile.Path.StartsWith(oldPathPrefix)) return;
            folderContentFile.Path = folderContentFile.Path.ReplacePrefixInString(oldPathPrefix, newPathPrefix);
            UpdateFolderContentFile(folderContentFile);
        }

        private void ValidateFileNewNameInParentData(IFolderContent file, string newName)
        {
            var parent = _folderContentFolderService.GetParentFolder(file);
            var oldName = file.Name;
            file.Name = newName;
            _folderContentPageService.ValidateUniquenessOnAllFolderPages(parent, file);
            file.Name = oldName;
        }
    }
}
