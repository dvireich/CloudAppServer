using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model;
using ContentManager.Model.FolderProviders;

namespace ContentManager
{
    public class FolderContentSearchManager
    {
        #region Members

        private readonly IPathManager _pathManager;
        private readonly IConfiguration _configuration;
        private readonly IFolderProvider<SearchFolder> _folderProvider;

        #endregion

        #region Ctor

        public FolderContentSearchManager(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            IConfiguration configuration)
        {
            _pathManager = pathManager;
            _configuration = configuration;
            _folderProvider = new SearchFolderProvider(directoryManager, pathManager, fileManager, configuration);
        }

        public FolderContentSearchManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _pathManager = new PathManager();
            var directoryManager = new DirectoryManagerAsync();
            var fileManager = new FileManagerAsync();
            _folderProvider = new SearchFolderProvider(directoryManager, _pathManager, fileManager, configuration);
        }

        #endregion

        #region Public methods

        public async Task<IResult<Folder>> Search(string nameToSearch, int page)
        {
            var homePathResult = _pathManager.Combine(_configuration.HomeFolderPath, _configuration.HomeFolderName);

            if (!homePathResult.IsSuccess)
            {
                return new FailureResult<Folder>(homePathResult.Exception);
            }

            var homeFolder = _folderProvider.GetFolder(homePathResult.Data);

            var loadSearchResults = await homeFolder.LoadSearchPageAsync(nameToSearch, page);

            if (!loadSearchResults.IsSuccess)
            {
                return new FailureResult<Folder>(loadSearchResults.Exception);
            }

            return new SuccessResult<Folder>(homeFolder);
        }

        public async Task<IResult<long>> GetNumOfFolderPagesAsync(string name, string path)
        {
            var homePathResult = _pathManager.Combine(_configuration.HomeFolderPath, _configuration.HomeFolderName);

            if (!homePathResult.IsSuccess)
            {
                return new FailureResult<long>(homePathResult.Exception);
            }

            var homeFolder = _folderProvider.GetFolder(homePathResult.Data);
            var searchResult = await homeFolder.LoadSearchPageAsync(name, 1);

            if (!searchResult.IsSuccess)
            {
                return new FailureResult<long>(searchResult.Exception);
            }

            return await homeFolder.GetNumOfFolderPagesAsync();
        }

        #endregion
    }
}
