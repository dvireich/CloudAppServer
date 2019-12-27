using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.Folders;

namespace ContentManager.Model.FolderProviders
{
    public class FolderProvider : IFolderProvider<Folder>
    {
        #region Members

        private readonly IDirectoryManagerAsync _directoryManager;
        private readonly IPathManager _pathManager;
        private readonly IFileManagerAsync _fileManager;
        private readonly IConfiguration _configuration;

        #endregion

        #region Ctor

        public FolderProvider(
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

        #region Public methods

        public virtual IResult<Folder> GetFolder(string path)
        {
            return new SuccessResult<Folder>(
                new Folder(_directoryManager, _pathManager, _fileManager, path, _configuration));
        }

        #endregion
    }
}
