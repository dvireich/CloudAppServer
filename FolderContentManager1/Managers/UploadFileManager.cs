using System;
using System.IO;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.FolderProviders;
using ContentManager.Model.Folders;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Managers
{
    public class UploadFileManager : ContentManager<BufferedFolder>
    {
        #region Consts

        private const string FolderName = "Upload";

        #endregion

        #region Members

        private BufferedFolder _uploadFolder;

        #endregion

        #region Ctor

        public UploadFileManager(
            IDirectoryManagerAsync directoryManager, 
            IPathManager pathManager, 
            IFileManagerAsync fileManager, 
            IConfiguration configuration) : 
            base(new BufferedFolderProvider(directoryManager, pathManager, fileManager, configuration), pathManager)
        {
            var createUploadFolderResult = CreateUploadFolder(configuration);

            if (!createUploadFolderResult.IsSuccess)
            {
                throw createUploadFolderResult.Exception;
            }
        }

        public UploadFileManager(
            IConfiguration configuration) :
            base(new BufferedFolderProvider(new DirectoryManagerAsync(), new PathManager(), new FileManagerAsync(), configuration), new PathManager())
        {
            var createUploadFolderResult = CreateUploadFolder(configuration);

            if (!createUploadFolderResult.IsSuccess)
            {
                throw createUploadFolderResult.Exception;
            }
        }

        #endregion

        #region Public

        public async Task<IResult<Void>> CreateUploadFileAsync(string name, string path)
        {
            var addFileResult = await _uploadFolder.AddFileAsync(new MemoryStream(), GetFileName(name, path));

            if (!addFileResult.IsSuccess)
            {
                return new FailureResult(addFileResult.Exception);
            }

            return new SuccessResult();
        }

        public async Task<IResult<Void>> UpdateFileContentAsync(string name, string path, string base64StringContent)
        {
            var updateResult = await _uploadFolder.BufferToFileAsync(GetFileName(name, path), base64StringContent);

            if (!updateResult.IsSuccess)
            {
                return new FailureResult(updateResult.Exception);
            }

            return new SuccessResult();
        }

        public async Task<IResult<Void>> FinishUploadAsync(string name, string path)
        {
            var fileResult = await _uploadFolder.GetChildFileAsync(GetFileName(name, path));

            if (!fileResult.IsSuccess)
            {
                return new FailureResult(fileResult.Exception);
            }

            var streamResult = await fileResult.Data.GetStreamAsync();

            if (!streamResult.IsSuccess)
            {
                return new FailureResult(streamResult.Exception);
            }

            var destinationFolderResult = FolderProvider.GetFolder(path);

            if (!destinationFolderResult.IsSuccess)
            {
                return new FailureResult(destinationFolderResult.Exception);
            }

            var addFileResult = await destinationFolderResult.Data.AddFileAsync(streamResult.Data, name);

            if (!addFileResult.IsSuccess)
            {
                return new FailureResult(addFileResult.Exception);
            }

            return await fileResult.Data.DeleteAsync();
        }

        public async Task<IResult<Void>> CancelUploadAsync(string name, string path)
        {
            var fileResult = await _uploadFolder.GetChildFileAsync(GetFileName(name, path));

            if (!fileResult.IsSuccess)
            {
                return new FailureResult(fileResult.Exception);
            }

            return await fileResult.Data.DeleteAsync();
        }

        #endregion

        #region Private

        private IResult<Void> CreateUploadFolder(IConfiguration configuration)
        {
            var createFolderResult = CreateFolderAsync(configuration.TemporaryFileFolderName, configuration.HomeFolderPath).Result;

            if (!createFolderResult.IsSuccess)
            {
                return createFolderResult;
            }

            var uploadPathResult = PathManager.Combine(configuration.HomeFolderPath, configuration.TemporaryFileFolderName);

            if (!uploadPathResult.IsSuccess)
            {
                return new FailureResult(uploadPathResult.Exception);
            }

            createFolderResult = CreateFolderAsync(FolderName, uploadPathResult.Data).Result;

            if (!createFolderResult.IsSuccess)
            {
                return createFolderResult;
            }

            var pathResult = PathManager.Combine(uploadPathResult.Data, FolderName);

            if (!pathResult.IsSuccess)
            {
                return new FailureResult(pathResult.Exception);
            }

            var uploadFolderResult = FolderProvider.GetFolder(pathResult.Data);

            if (!uploadFolderResult.IsSuccess)
            {
                return new FailureResult(uploadFolderResult.Exception);
            }

            _uploadFolder = uploadFolderResult.Data;

            return new SuccessResult();
        }

        private string GetFileName(string name, string path)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{path}\\{name}");
            return Convert.ToBase64String(plainTextBytes);
        }

        #endregion

    }
}
