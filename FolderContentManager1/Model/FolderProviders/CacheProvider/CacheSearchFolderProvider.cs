using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.Folders;

namespace ContentManager.Model.FolderProviders.CacheProvider
{
    public class CacheSearchFolderProvider : CacheFolderProviderBase<SearchFolder>
    {
        #region Ctor

        public CacheSearchFolderProvider(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            IConfiguration configuration) : 
            base(directoryManager, pathManager, fileManager, configuration)
        {
        }

        #endregion

        public override IResult<SearchFolder> GetFolder(string path)
        {
            var cacheFolderResult = base.GetFolder(path);

            return !cacheFolderResult.IsSuccess
                ? cacheFolderResult
                : new SuccessResult<SearchFolder>(cacheFolderResult.Data.Clone<SearchFolder>());
        }
    }
}
