using System.IO;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Helpers.Result.InternalTypes;
using ContentManager.Model;
using ContentManager.Model.Enums;
using ContentManager.Model.FolderProviders;
using ContentManager.Model.Folders;

namespace ContentManager.Managers
{
    public abstract class FolderContentManagerBase<T> : ContentManager<T> where T : Folder
    {
        #region Ctor

        protected FolderContentManagerBase(
            IPathManager pathManager,
            IFolderProvider<T> folderProvider) :
            base(folderProvider, pathManager)
        {
        }

        protected FolderContentManagerBase(IFolderProvider<T> folderProvider) :
            base(folderProvider, new PathManager())
        {
        }

        #endregion

        #region Public methods

        public async Task<IResult<Void>> DeleteFolderAsync(string name, string path)
        {
            var parentFolderResult = FolderProvider.GetFolder(path);

            if (!parentFolderResult.IsSuccess)
            {
                return new FailureResult(parentFolderResult.Exception);
            }

            var folderToDeleteResult = await parentFolderResult.Data.GetChildFolderAsync(name);

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
            var parentFolderResult = FolderProvider.GetFolder(path);

            if (!parentFolderResult.IsSuccess)
            {
                return new FailureResult(parentFolderResult.Exception);
            }

            var fileToDeleteResult = await parentFolderResult.Data.GetChildFileAsync(name);

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
            var folderResult = FolderProvider.GetFolder(path);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult<Stream>(folderResult.Exception);
            }

            var fileResult = await folderResult.Data.GetChildFileAsync(name);

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

            var folderResult = FolderProvider.GetFolder(relativePathResult.Data);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult<Folder>(folderResult.Exception);
            }

            var loadPageResult = await folderResult.Data.LoadFolderPageAsync(pageNumber);

            if (!loadPageResult.IsSuccess)
            {
                return new FailureResult<Folder>(loadPageResult.Exception);
            }

            return new SuccessResult<Folder>(folderResult.Data);

        }

        public IResult<SortType> GetSortType(string name, string path)
        {
            var relativePathResult = PathManager.Combine(path, name);

            if (!relativePathResult.IsSuccess)
            {
                return new FailureResult<SortType>(relativePathResult.Exception);
            }

            var folderResult = FolderProvider.GetFolder(relativePathResult.Data);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult<SortType>(folderResult.Exception);
            }

            return new SuccessResult<SortType>(folderResult.Data.SortType);
        }

        public IResult<int> GetNumberOfElementToShowOnPage(string name, string path)
        {
            var relativePathResult = PathManager.Combine(path, name);

            if (!relativePathResult.IsSuccess)
            {
                return new FailureResult<int>(relativePathResult.Exception);
            }

            var folderResult = FolderProvider.GetFolder(relativePathResult.Data);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult<int>(folderResult.Exception);
            }

            return new SuccessResult<int>(folderResult.Data.NumberOfElementToShowOnPage);
        }

        public virtual async Task<IResult<long>> GetNumOfFolderPagesAsync(string name, string path)
        {
            var relativePathResult = PathManager.Combine(path, name);

            if (!relativePathResult.IsSuccess)
            {
                return new FailureResult<long>(relativePathResult.Exception);
            }

            var folderResult = FolderProvider.GetFolder(relativePathResult.Data);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult<long>(folderResult.Exception);
            }

            return await folderResult.Data.GetNumOfFolderPagesAsync();
        }

        public async Task<IResult<Void>> RenameFileAsync(string name, string path, string newName)
        {
            var parentFolderResult = FolderProvider.GetFolder(path);

            if (!parentFolderResult.IsSuccess)
            {
                return new FailureResult(parentFolderResult.Exception);
            }

            var fileResult = await parentFolderResult.Data.GetChildFileAsync(name);

            if (!fileResult.IsSuccess)
            {
                return new FailureResult(fileResult.Exception);
            }

            return await fileResult.Data.RenameAsync(newName);
        }

        public async Task<IResult<Void>> RenameFolderAsync(string name, string path, string newName)
        {
            var parentFolderResult = FolderProvider.GetFolder(path);

            if (!parentFolderResult.IsSuccess)
            {
                return new FailureResult(parentFolderResult.Exception);
            }

            var folderResult = await parentFolderResult.Data.GetChildFolderAsync(name);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult(folderResult.Exception);
            }

            return await folderResult.Data.RenameAsync(newName);
        }

        public async Task<IResult<Void>> CreateFileAsync(string name, string path, Stream content)
        {
            var folderResult = FolderProvider.GetFolder(path);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult(folderResult.Exception);
            }

            var fileResult = await folderResult.Data.AddFileAsync(content, name);

            if (!fileResult.IsSuccess)
            {
                return new FailureResult(fileResult.Exception);
            }

            return new SuccessResult();
        }

        public async Task<IResult<Void>> CopyFolderAsync(
            string sourcePath,
            string sourceFolderName,
            string destPath,
            string destName)
        {
            var parentFolderResult = FolderProvider.GetFolder(sourcePath);

            if (!parentFolderResult.IsSuccess)
            {
                return new FailureResult(parentFolderResult.Exception);
            }

            var sourceFolderResult = await parentFolderResult.Data.GetChildFolderAsync(sourceFolderName);

            if (!sourceFolderResult.IsSuccess)
            {
                return new FailureResult(sourceFolderResult.Exception);
            }

            var destParentFolderResult = FolderProvider.GetFolder(destPath);

            if (!destParentFolderResult.IsSuccess)
            {
                return new FailureResult(destParentFolderResult.Exception);
            }

            var destFolderResult = await destParentFolderResult.Data.GetChildFolderAsync(destName);

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
            var parentFolderResult = FolderProvider.GetFolder(sourcePath);

            if (!parentFolderResult.IsSuccess)
            {
                return new FailureResult(parentFolderResult.Exception);
            }

            var sourceFileResult = await parentFolderResult.Data.GetChildFileAsync(sourceFileName);

            if (!sourceFileResult.IsSuccess)
            {
                return new FailureResult(sourceFileResult.Exception);
            }

            var destParentFolderResult = FolderProvider.GetFolder(destPath);

            if (!destParentFolderResult.IsSuccess)
            {
                return new FailureResult(destParentFolderResult.Exception);
            }

            var destFolderResult = await destParentFolderResult.Data.GetChildFolderAsync(destName);

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

            var folderResult = FolderProvider.GetFolder(pathResult.Data);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult(folderResult.Exception);
            }

            folderResult.Data.SortType = folderMetadata.SortType;
            folderResult.Data.NumberOfElementToShowOnPage = folderMetadata.NumberOfPagesPerPage;

            return await folderResult.Data.SaveAsync();
        }

        #endregion
    }
}
