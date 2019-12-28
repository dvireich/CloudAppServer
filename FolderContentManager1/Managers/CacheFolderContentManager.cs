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
        public CacheFolderContentManager(
            IPathManager pathManager,
            IConfiguration configuration,
            IFolderProvider<CacheFolder> folderProvider) :
            base(pathManager, configuration, folderProvider)
        {
        }

        public CacheFolderContentManager(
            IConfiguration configuration) :
            base(
                configuration,
                new CacheFolderProvider(
                    new DirectoryManagerAsync(), 
                    new PathManager(), 
                    new FileManagerAsync(), 
                    configuration))
        {
        }
    }
}
