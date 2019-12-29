using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.Folders;

namespace ContentManager.Model.FolderProviders.CacheProvider
{
    class EntryPointCacheFolderProvider : IFolderProvider<CacheFolder>
    {
        private readonly IConfiguration _configuration;

        public EntryPointCacheFolderProvider(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IResult<CacheFolder> GetFolder(string path)
        {
            return new SuccessResult<CacheFolder>(
                new CacheFolder(
                    new DirectoryManagerAsync(),
                    new PathManager(),
                    new FileManagerAsync(),
                    path,
                    _configuration,
                    null));
        }
    }
}
