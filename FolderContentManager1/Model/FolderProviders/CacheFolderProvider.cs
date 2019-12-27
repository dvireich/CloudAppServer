using System;
using System.Collections.Generic;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.Folders;

namespace ContentManager.Model.FolderProviders
{
    public class CacheFolderProvider : IFolderProvider<Folder>
    {
        #region Members

        private readonly CacheFolder _root;

        #endregion

        #region Ctor

        public CacheFolderProvider(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            IConfiguration configuration)
        {
            var pathManager1 = pathManager;
            var configuration1 = configuration;

            var homeFolderPathResult = pathManager1.Combine(configuration1.HomeFolderPath, configuration1.HomeFolderName);

            if (!homeFolderPathResult.IsSuccess)
            {
                throw homeFolderPathResult.Exception;
            }

            _root = new CacheFolder(
                directoryManager, 
                pathManager1, 
                fileManager, 
                homeFolderPathResult.Data,
                configuration1, 
                null);
        }

        #endregion

        #region Public

        public IResult<Folder> GetFolder(string path)
        {
            var parentFolderNames = path
                .Trim('\\')
                .Split('\\');

            return FindInCacheFolderTree(parentFolderNames);
        }

        #endregion

        #region Private

        private IResult<Folder> FindInCacheFolderTree(IEnumerable<string> parentFolderNames)
        {
            var current = _root;

            foreach (var parentFolderName in parentFolderNames)
            {
                var childFolderResult = current.GetChildFolderAsync(parentFolderName).Result;

                if (!childFolderResult.IsSuccess)
                {
                    return new FailureResult<Folder>(childFolderResult.Exception);
                }

                if (!(childFolderResult.Data is CacheFolder cacheFolder))
                {
                    return new FailureResult<Folder>(
                        new Exception(
                            $"Folder '{childFolderResult.Data.Name}' in path '{childFolderResult.Data.RelativePath}' is not CacheFolder"));
                }

                current = cacheFolder;
            }

            return new SuccessResult<Folder>(current);
        }

        #endregion
    }
}
