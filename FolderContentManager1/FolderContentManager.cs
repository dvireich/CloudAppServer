using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Helpers.Result.InternalTypes;
using ContentManager.Model;
using ContentManager.Model.Enums;
using ContentManager.Model.FolderProviders;

namespace ContentManager
{
    public class FolderContentManager : ContentManager<Folder>
    {
        #region Ctor

        public FolderContentManager(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            IConfiguration configuration) :
            base(new FolderProvider(directoryManager, pathManager, fileManager, configuration), pathManager)
        {
            CreateFolderAsync(configuration.BaseFolderName, configuration.BaseFolderPath).Wait();
            CreateFolderAsync(configuration.HomeFolderName, configuration.HomeFolderPath).Wait();
            CreateFolderAsync(configuration.TemporaryFileFolderName, configuration.HomeFolderPath).Wait();
        }

        public FolderContentManager(IConfiguration configuration) :
            base(new FolderProvider(new DirectoryManagerAsync(), new PathManager(), new FileManagerAsync(), configuration), new PathManager())
        {
            CreateFolderAsync(configuration.BaseFolderName, configuration.BaseFolderPath).Wait();
            CreateFolderAsync(configuration.HomeFolderName, configuration.HomeFolderPath).Wait();
            CreateFolderAsync(configuration.TemporaryFileFolderName, configuration.HomeFolderPath).Wait();
        }

        #endregion

        #region Public methods

        public async Task<IResult<Void>> DeleteFolderAsync(string name, string path)
        {
            var parentFolder = FolderProvider.GetFolder(path);

            var folderToDeleteResult = await parentFolder.GetChildFolderAsync(name);

            if (!folderToDeleteResult.IsSuccess)
            {
                return new FailureResult(folderToDeleteResult.Exception);
            }

            var deleteFolderResult = await folderToDeleteResult.Data.DeleteAsync();

            if (!deleteFolderResult.IsSuccess)
            {
                return new FailureResult(deleteFolderResult.Exception);
            }

            return new SuccessResult();
        }

        public async Task<IResult<Void>> DeleteFileAsync(string name, string path)
        {
            var parentFolder = FolderProvider.GetFolder(path);

            var fileToDeleteResult = await parentFolder.GetChildFileAsync(name);

            if (!fileToDeleteResult.IsSuccess)
            {
                return new FailureResult(fileToDeleteResult.Exception);
            }

            var deleteFolderResult = await fileToDeleteResult.Data.DeleteAsync();

            if (!deleteFolderResult.IsSuccess)
            {
                return new FailureResult(deleteFolderResult.Exception);
            }

            return new SuccessResult();
        }

        public async Task<IResult<Stream>> GetFileStreamAsync(string name, string path)
        {
            var folder = FolderProvider.GetFolder(path);
            var fileResult = await folder.GetChildFileAsync(name);

            if (!fileResult.IsSuccess)
            {
                return new FailureResult<Stream>(fileResult.Exception);
            }

            return await fileResult.Data.GetStreamAsync();
        }

        public async Task<IResult<Folder>> GetFolderPageAsync(string name, string path, int pageNumber)
        {
            var relativePathResult = PathManager.Combine(path, name);

            if (!relativePathResult.IsSuccess)
            {
                return new FailureResult<Folder>(relativePathResult.Exception);
            }

            var folder = FolderProvider.GetFolder(relativePathResult.Data);
            var loadPageResult = await folder.LoadFolderPageAsync(pageNumber);

            if (!loadPageResult.IsSuccess)
            {
                return new FailureResult<Folder>(loadPageResult.Exception);
            }

            return new SuccessResult<Folder>(folder);

        }

        public IResult<SortType> GetSortType(string name, string path)
        {
            var relativePathResult = PathManager.Combine(path, name);

            if (!relativePathResult.IsSuccess)
            {
                return new FailureResult<SortType>(relativePathResult.Exception);
            }

            var folder = FolderProvider.GetFolder(relativePathResult.Data);

            return new SuccessResult<SortType>(folder.SortType);
        }

        public IResult<int> GetNumberOfElementToShowOnPage(string name, string path)
        {
            var relativePathResult = PathManager.Combine(path, name);

            if (!relativePathResult.IsSuccess)
            {
                return new FailureResult<int>(relativePathResult.Exception);
            }

            var folder = FolderProvider.GetFolder(relativePathResult.Data);

            return new SuccessResult<int>(folder.NumberOfElementToShowOnPage);
        }

        public virtual async Task<IResult<long>> GetNumOfFolderPagesAsync(string name, string path)
        {
            var relativePathResult = PathManager.Combine(path, name);

            if (!relativePathResult.IsSuccess)
            {
                return new FailureResult<long>(relativePathResult.Exception);
            }

            var folder = FolderProvider.GetFolder(relativePathResult.Data);

            return await folder.GetNumOfFolderPagesAsync();
        }

        public async Task<IResult<Void>> RenameFileAsync(string name, string path, string newName)
        {
            var parentFolder = FolderProvider.GetFolder(path);

            var fileResult = await parentFolder.GetChildFileAsync(name);

            if (!fileResult.IsSuccess)
            {
                return new FailureResult(fileResult.Exception);
            }

            return await fileResult.Data.RenameAsync(newName);
        }

        public async Task<IResult<Void>> RenameFolderAsync(string name, string path, string newName)
        {
            var parentFolder = FolderProvider.GetFolder(path);

            var folderResult = await parentFolder.GetChildFolderAsync(name);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult(folderResult.Exception);
            }

            return await folderResult.Data.RenameAsync(newName);
        }

        public async Task<IResult<Void>> CreateFileAsync(string name, string path, Stream content)
        {
            var folder = FolderProvider.GetFolder(path);
            var fileResult = await folder.AddFileAsync(content, name);

            if (!fileResult.IsSuccess)
            {
                return new FailureResult<Void>(fileResult.Exception);
            }

            return new SuccessResult();
        }

        public async Task<IResult<Void>> CopyFolderAsync(
            string sourcePath,
            string sourceFolderName,
            string destPath,
            string destName)
        {
            var parentFolder = FolderProvider.GetFolder(sourcePath);
            var sourceFolderResult = await parentFolder.GetChildFolderAsync(sourceFolderName);

            if (!sourceFolderResult.IsSuccess)
            {
                return new FailureResult(sourceFolderResult.Exception);
            }

            var destParentFolder = FolderProvider.GetFolder(destPath);
            var destFolderResult = await destParentFolder.GetChildFolderAsync(destName);

            if (!destFolderResult.IsSuccess)
            {
                return new FailureResult(destFolderResult.Exception);
            }

            var copyResult = await sourceFolderResult.Data.CopyToAsync(destFolderResult.Data);

            if (!copyResult.IsSuccess)
            {
                return new FailureResult(copyResult.Exception);
            }

            return new SuccessResult();
        }

        public async Task<IResult<Void>> CopyFileAsync(
            string sourcePath,
            string sourceFileName,
            string destPath,
            string destName)
        {
            var parentFolder = FolderProvider.GetFolder(sourcePath);
            var sourceFileResult = await parentFolder.GetChildFileAsync(sourceFileName);

            if (!sourceFileResult.IsSuccess)
            {
                return new FailureResult(sourceFileResult.Exception);
            }

            var destParentFolder = FolderProvider.GetFolder(destPath);
            var destFolderResult = await destParentFolder.GetChildFolderAsync(destName);

            if (!destFolderResult.IsSuccess)
            {
                return new FailureResult(destFolderResult.Exception);
            }

            var sourceStreamResult = await sourceFileResult.Data.GetStreamAsync();

            if (!sourceStreamResult.IsSuccess)
            {
                return new FailureResult(sourceStreamResult.Exception);
            }

            var createResult = await destFolderResult.Data.AddFileAsync(sourceStreamResult.Data, sourceFileName);

            if (!createResult.IsSuccess)
            {
                return new FailureResult(createResult.Exception);
            }

            return new SuccessResult();
        }

        public async Task<IResult<Void>> UpdateFolderMetaData(FolderMetadata folderMetadata)
        {
            var pathResult = PathManager.Combine(folderMetadata.Path, folderMetadata.Name);

            if (!pathResult.IsSuccess)
            {
                return new FailureResult(pathResult.Exception);
            }

            var folder = FolderProvider.GetFolder(pathResult.Data);
            folder.SortType = folderMetadata.SortType;
            folder.NumberOfElementToShowOnPage = folderMetadata.NumberOfPagesPerPage;

            return await folder.SaveAsync();
        }

        #endregion
    }
}
