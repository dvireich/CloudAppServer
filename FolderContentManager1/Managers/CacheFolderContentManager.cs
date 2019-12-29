using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Model.FolderProviders;
using ContentManager.Model.FolderProviders.CacheProvider;
using ContentManager.Model.Folders;

namespace ContentManager.Managers
{
    public class CacheFolderContentManager : FolderContentManagerBase<CacheFolder>
    {
        #region Ctor

        public CacheFolderContentManager(
            IPathManager pathManager,
            IConfiguration configuration,
            IFolderProvider<CacheFolder> cacheFolderProvider) :
            base(pathManager, cacheFolderProvider)
        {
            CreateFolders(configuration);
        }

        public CacheFolderContentManager(
            IConfiguration configuration) :
            base(new CacheFolderProvider(
                new DirectoryManagerAsync(),
                new PathManager(),
                new FileManagerAsync(),
                configuration))
        {
            CreateFolders(configuration);
        }

        #endregion

        #region Private

        private void CreateFolders(IConfiguration configuration)
        {
            var folderProvider = new EntryPointCacheFolderProvider(configuration);

            CreateFolderAsync(configuration.BaseFolderName, configuration.BaseFolderPath, folderProvider).Wait();
            CreateFolderAsync(configuration.HomeFolderName, configuration.HomeFolderPath, folderProvider).Wait();
            CreateFolderAsync(configuration.TemporaryFileFolderName, configuration.HomeFolderPath, folderProvider).Wait();
        }

        #endregion
    }

    
}
