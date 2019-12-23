using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;

namespace ContentManager.Model.FolderProviders
{
    class BufferedFolderProvider : IFolderProvider<BufferedFolder>
    {
        #region Members

        private readonly IDirectoryManagerAsync _directoryManager;
        private readonly IPathManager _pathManager;
        private readonly IFileManagerAsync _fileManager;
        private readonly IConfiguration _configuration;

        #endregion

        #region Ctor

        public BufferedFolderProvider(
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

        public BufferedFolder GetFolder(string path)
        {
            return new BufferedFolder(_directoryManager, _pathManager, _fileManager, path, _configuration);
        }
    }
}
