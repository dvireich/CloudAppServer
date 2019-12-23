using System;
using System.IO;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.Enums;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Model
{
    public class SystemFile : SystemContentBase
    {
        #region Members

        public override string CreationTime => GetCreationTimeAsync().Result;
        public override string ModificationTime => GetModificationTimeAsync().Result;
        public override FolderContentType Type
        {
            get => FolderContentType.File;
            protected set { }
        }

        public override long Size => GetSizeAsync().Result;

        #endregion

        #region Ctor

        public SystemFile(
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

        public async Task<IResult<Void>> CreateAsync(Stream stream, bool leaveOpen = false)
        {
            var createFileResult = await FileManager.CreateFileAsync(FullPath, stream, leaveOpen);

            if (!createFileResult.IsSuccess)
            {
                return new FailureResult(createFileResult.Exception);
            }

            return new SuccessResult();
        }

        public async Task<IResult<Void>> DeleteAsync()
        {
            return await FileManager.DeleteAsync(FullPath);
        }

        public async Task<IResult<Void>> RenameAsync(string newFileName)
        {
            var basePath = PathManager.GetParent(FullPath);
            
            if (!basePath.IsSuccess)
            {
                return new FailureResult(basePath.Exception);
            }

            var oldName = FullPath.Substring(0, basePath.Data.Length);

            return await FileManager.RenameFileAsync(basePath.Data, oldName, newFileName);
        }

        public async Task<IResult<Stream>> GetStreamAsync()
        {
            return await FileManager.GetFileStreamAsync(FullPath);
        }

        public async Task<IResult<Void>> CopyToAsync(SystemFolder folder)
        {
            var streamResult = await GetStreamAsync();

            if (!streamResult.IsSuccess)
            {
                return new FailureResult(streamResult.Exception);
            }

            var createResult = await folder.AddFileAsync(streamResult.Data, Name);

            if (!createResult.IsSuccess)
            {
                return new FailureResult(createResult.Exception);
            }

            return new SuccessResult();
        }

        #endregion

        #region Private methods

        private async Task<string> GetCreationTimeAsync()
        {
            var creationTimeResult = await FileManager.GetCreationTimeAsync(FullPath);

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

        private async Task<long> GetSizeAsync()
        {
            var sizeResult = await FileManager.GetSizeAsync(FullPath);

            if (!sizeResult.IsSuccess)
            {
                return -1;
            }

            return sizeResult.Data;
        }

        #endregion

    }
}
