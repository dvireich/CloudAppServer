using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.Enums;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Model.Folders
{
    public class SystemFolder : SystemContentBase
    {
        #region Members

        public override string CreationTime => GetCreationTimeAsync().Result;
        public override string ModificationTime => GetModificationTimeAsync().Result;
        public sealed override FolderContentType Type
        {
            get => FolderContentType.Folder;
            protected set { }
        }

        public override long Size => long.MinValue;

        #endregion

        #region Ctor

        public SystemFolder(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            string relativePath,
            IConfiguration configuration)
            : base(directoryManager, pathManager, fileManager, relativePath, configuration)
        {
        }

        #endregion

        #region Public methods

        #region SystemFolder methods

        public async Task<IResult<Void>> CreateAsync()
        {
            var isExistsResult = await FileManager.ExistsAsync(FullPath);

            if (!isExistsResult.IsSuccess)
            {
                return new FailureResult(isExistsResult.Exception);
            }

            if (!isExistsResult.Data)
            {
                return await DirectoryManager.CreateDirectoryAsync(FullPath);
            }

            return new SuccessResult();
        }

        public virtual async Task<IResult<SystemFolder>> GetChildFolderAsync(string name)
        {
            var pathResult = PathManager.Combine(FullPath, name);

            if (!pathResult.IsSuccess)
            {
                return new FailureResult<SystemFolder>(pathResult.Exception);
            }

            var isExistsResult = await DirectoryManager.ExistsAsync(pathResult.Data);

            if (!isExistsResult.IsSuccess || !isExistsResult.Data)
            {
                return new FailureResult<SystemFolder>(isExistsResult.Exception ?? new Exception($"SystemFolder '{name}' doesn't exists on relativePath '{pathResult.Data}'"));
            }

            var folder = new SystemFolder(DirectoryManager, PathManager, FileManager, pathResult.Data, Configuration);

            return new SuccessResult<SystemFolder>(folder);
        }

        public virtual async Task<IResult<SystemFolder>> AddChildFolderAsync(string name)
        {
            var pathResult = PathManager.Combine(FullPath, name);

            if (!pathResult.IsSuccess)
            {
                return new FailureResult<SystemFolder>(pathResult.Exception);
            }

            var folder = new SystemFolder(DirectoryManager, PathManager, FileManager, pathResult.Data, Configuration);

            var createResult = await folder.CreateAsync();

            if (!createResult.IsSuccess)
            {
                return new FailureResult<SystemFolder>(createResult.Exception);
            }

            return new SuccessResult<SystemFolder>(folder);
        }

        public virtual async Task<IResult<IEnumerable<SystemFolder>>> GetAllChildFolderAsync()
        {
            var directoriesResult = await DirectoryManager.GetAllDirectoriesAsync(FullPath);

            if (!directoriesResult.IsSuccess)
            {
                return new FailureResult<IEnumerable<SystemFolder>>(directoriesResult.Exception);
            }

            var allChildren = new List<SystemFolder>();

            foreach (var folderName in directoriesResult.Data)
            {
                var folderResult = await GetChildFolderAsync(folderName);

                if (!folderResult.IsSuccess)
                {
                    return new FailureResult<IEnumerable<SystemFolder>>(folderResult.Exception);
                }

                allChildren.Add(folderResult.Data);
            }

            return new SuccessResult<IEnumerable<SystemFolder>>(allChildren);
        }

        public virtual async Task<IResult<IEnumerable<SystemFile>>> GetAllChildFilesAsync()
        {
            var filesResult = await DirectoryManager.GetAllFilesAsync(FullPath);

            if (!filesResult.IsSuccess)
            {
                return new FailureResult<IEnumerable<SystemFile>>(filesResult.Exception);
            }

            var allChildren = new List<SystemFile>();

            foreach (var fileName in filesResult.Data)
            {
                var fileResult = await GetChildFileAsync(fileName);

                if (!fileResult.IsSuccess)
                {
                    return new FailureResult<IEnumerable<SystemFile>>(fileResult.Exception);
                }

                allChildren.Add(fileResult.Data);
            }

            return new SuccessResult<IEnumerable<SystemFile>>(allChildren);
        }

        public virtual async Task<IResult<IEnumerable<ContentBase>>> GetAllChildrenAsync()
        {
            
            var allChildren = new List<ContentBase>();

            var allChildFolderResult = await GetAllChildFolderAsync();

            if (!allChildFolderResult.IsSuccess)
            {
                return new FailureResult<IEnumerable<ContentBase>>(allChildFolderResult.Exception);
            }

            var allChildFilesResult = await GetAllChildFilesAsync();

            if (!allChildFilesResult.IsSuccess)
            {
                return new FailureResult<IEnumerable<ContentBase>>(allChildFilesResult.Exception);
            }

            allChildren.AddRange(allChildFilesResult.Data);
            allChildren.AddRange(allChildFolderResult.Data);
            
            return new SuccessResult<IEnumerable<ContentBase>>(allChildren);

        }

        public virtual async Task<IResult<Void>> DeleteAsync()
        {
            return await DirectoryManager.DeleteDirectoryAsync(FullPath, true);
        }

        public virtual async Task<IResult<Void>> RenameAsync(string newFolderName)
        {
            var basePath = PathManager.GetParent(FullPath);

            if (!basePath.IsSuccess)
            {
                return new FailureResult(basePath.Exception);
            }

            var oldName = FullPath.Replace(basePath.Data, string.Empty).Trim('\\');

            return await DirectoryManager.RenameDirectoryAsync(basePath.Data, oldName, newFolderName);
        }

        public virtual async Task<IResult<Void>> CopyToAsync(SystemFolder destFolder)
        {
            // Copy folder
            var sourceFolderInDestFolderResult = await destFolder.AddChildFolderAsync(Name);

            if (!sourceFolderInDestFolderResult.IsSuccess)
            {
                return new FailureResult(sourceFolderInDestFolderResult.Exception);
            }

            // Copy sub folders
            var allChildFolderResult = await GetAllChildFolderAsync();

            if (!allChildFolderResult.IsSuccess)
            {
                return new FailureResult(allChildFolderResult.Exception);
            }

            foreach (var childFolder in allChildFolderResult.Data)
            {
                var copyResult = await childFolder.CopyToAsync(sourceFolderInDestFolderResult.Data);

                if (!copyResult.IsSuccess)
                {
                    return copyResult;
                }
            }

            // Copy sub files
            var allChildFilesResult = await GetAllChildFilesAsync();

            if (!allChildFilesResult.IsSuccess)
            {
                return new FailureResult(allChildFilesResult.Exception);
            }

            foreach (var file in allChildFilesResult.Data)
            {
                var copyFileResult = await file.CopyToAsync(sourceFolderInDestFolderResult.Data);

                if (!copyFileResult.IsSuccess)
                {
                    return copyFileResult;
                }
            }

            return new SuccessResult();
        }

        #endregion

        #region SystemFile methods

        public virtual async Task<IResult<SystemFile>> AddFileAsync(Stream stream, string name, bool leaveOpen = false)
        {
            var pathResult = PathManager.Combine(FullPath, name);

            if (!pathResult.IsSuccess)
            {
                return new FailureResult<SystemFile>(pathResult.Exception);
            }

            var file = new SystemFile(DirectoryManager, PathManager, FileManager, pathResult.Data, Configuration);

            var createFileResult = await file.CreateAsync(stream, leaveOpen);

            if (!createFileResult.IsSuccess)
            {
                return new FailureResult<SystemFile>(createFileResult.Exception);
            }

            return new SuccessResult<SystemFile>(file);

        }

        public virtual async Task<IResult<SystemFile>> GetChildFileAsync(string name)
        {
            var pathResult = PathManager.Combine(FullPath, name);

            if (!pathResult.IsSuccess)
            {
                return new FailureResult<SystemFile>(pathResult.Exception);
            }

            var isExistsResult = await FileManager.ExistsAsync(pathResult.Data);

            if (!isExistsResult.IsSuccess || !isExistsResult.Data)
            {
                return new FailureResult<SystemFile>(isExistsResult.Exception ?? new Exception($"SystemFile '{name}' doesn't exists on relativePath '{pathResult.Data}'"));
            }

            var file = new SystemFile(DirectoryManager, PathManager, FileManager, pathResult.Data, Configuration);

            return new SuccessResult<SystemFile>(file);
        }

        #endregion

        #endregion

        #region Protected

        protected async Task<IResult<IEnumerable<ContentBase>>> Search(string name)
        {
            var searchResults = new List<ContentBase>();
            var filesResult = await GetAllChildFilesAsync();

            if (!filesResult.IsSuccess)
            {
                return new FailureResult<IEnumerable<ContentBase>>(filesResult.Exception);
            }

            searchResults.AddRange(filesResult.Data.Where(file => file.Name == name));

            var folderResult = await GetAllChildFolderAsync();

            if (!folderResult.IsSuccess)
            {
                return new FailureResult<IEnumerable<ContentBase>>(folderResult.Exception);
            }

            searchResults.AddRange(folderResult.Data.Where(file => file.Name == name));

            foreach (var folder in folderResult.Data)
            {
                var searchResult = await folder.Search(name);

                if (!searchResult.IsSuccess)
                {
                    return searchResult;
                }

                searchResults.AddRange(searchResult.Data);
            }

            return new SuccessResult<IEnumerable<ContentBase>>(searchResults);
        }

        #endregion

        #region Private

        private async Task<string> GetCreationTimeAsync()
        {
            var creationTimeResult = await DirectoryManager.GetCreationTimeAsync(FullPath);

            if (!creationTimeResult.IsSuccess)
            {
                return null;
            }

            return creationTimeResult.Data.ToLongDateString();
        }

        private async Task<string> GetModificationTimeAsync()
        {
            var modificationTimeResult = await FileManager.GetModificationTimeAsync(FullPath);

            if (!modificationTimeResult.IsSuccess)
            {
                return null;
            }

            return modificationTimeResult.Data.ToLongDateString();
        }

        #endregion
    }
}
