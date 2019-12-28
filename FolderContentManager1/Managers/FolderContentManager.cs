using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Model.FolderProviders;
using ContentManager.Model.Folders;

namespace ContentManager.Managers
{
    public class FolderContentManager : FolderContentManagerBase<Folder>
    {
        public FolderContentManager(
            IPathManager pathManager, 
            IConfiguration configuration, 
            IFolderProvider<Folder> folderProvider) : 
            base(pathManager, configuration, folderProvider)
        {
        }

        public FolderContentManager(
            IConfiguration configuration, 
            IFolderProvider<Folder> folderProvider) : 
            base(configuration, folderProvider)
        {
        }
    }
}
