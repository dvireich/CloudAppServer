using System;
using System.Collections.Generic;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.Folders;

namespace ContentManager.Model.FolderProviders.CacheProvider
{
    public abstract class CacheFolderProviderBase<T> : CacheFolderProviderRootBase,  IFolderProvider<T> where T : CacheFolder
    {
        #region Ctor

        protected CacheFolderProviderBase(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            IConfiguration configuration)
        {
            //var homeFolderPathResult = pathManager.Combine(configuration.HomeFolderPath, configuration.HomeFolderName);

            //if (!homeFolderPathResult.IsSuccess)
            //{
            //    throw homeFolderPathResult.Exception;
            //}

            Root = Root ?? new CacheFolder(
                        directoryManager,
                        pathManager,
                        fileManager,
                        configuration.HomeFolderPath,
                        configuration,
                        null);
        }

        #endregion

        #region Public

        public virtual IResult<T> GetFolder(string path)
        {
            var parentFolderNames = path
                .Trim('\\')
                .Split('\\');

            return FindInCacheFolderTree(parentFolderNames);
        }

        #endregion

        #region Private

        private IResult<T> FindInCacheFolderTree(IEnumerable<string> parentFolderNames)
        {
            var current = Root;

            foreach (var parentFolderName in parentFolderNames)
            {
                var childFolderResult = current.GetChildFolderAsync(parentFolderName).Result;

                if (!childFolderResult.IsSuccess)
                {
                    return new FailureResult<T>(childFolderResult.Exception);
                }

                if (!(childFolderResult.Data is CacheFolder cacheFolder))
                {
                    return new FailureResult<T>(
                        new Exception(
                            $"Folder '{childFolderResult.Data.Name}' in path '{childFolderResult.Data.RelativePath}' is not CacheFolder"));
                }

                current = cacheFolder;
            }

            return new SuccessResult<T>((T) current);
        }

        #endregion
    }
}
