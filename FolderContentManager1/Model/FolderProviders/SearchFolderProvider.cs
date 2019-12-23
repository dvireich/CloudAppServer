using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;

namespace ContentManager.Model.FolderProviders
{
    public class SearchFolderProvider : IFolderProvider<SearchFolder>
    {
        #region Members

        private readonly IDirectoryManagerAsync _directoryManager;
        private readonly IPathManager _pathManager;
        private readonly IFileManagerAsync _fileManager;
        private readonly IConfiguration _configuration;

        #endregion

        #region Ctor

        public SearchFolderProvider(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            IConfiguration configuration)
        {
            _directoryManager = directoryManager;
            _pathManager = pathManager;
            _fileManager = fileManager;
            _configuration = configuration;
        }

        #endregion

        public SearchFolder GetFolder(string path)
        {
            return new SearchFolder(_directoryManager, _pathManager, _fileManager, path, _configuration);
        }
    }
}
