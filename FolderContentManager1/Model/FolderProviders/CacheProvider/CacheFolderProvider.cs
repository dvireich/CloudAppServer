using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Model.Folders;

namespace ContentManager.Model.FolderProviders.CacheProvider
{
    public class CacheFolderProvider : CacheFolderProviderBase<CacheFolder>
    {
        public CacheFolderProvider(
            IDirectoryManagerAsync directoryManager, 
            IPathManager pathManager, 
            IFileManagerAsync fileManager, 
            IConfiguration configuration) : 
            base(directoryManager, pathManager, fileManager, configuration)
        {
        }
    }
}
