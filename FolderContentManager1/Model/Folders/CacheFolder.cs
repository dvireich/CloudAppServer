using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.Exceptions;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Model.Folders
{
    public class CacheFolder : Folder
    {
        #region Members

        private readonly ConcurrentDictionary<int, ContentPage> _pageNumberToContentPage = new ConcurrentDictionary<int, ContentPage>();
        private readonly ConcurrentDictionary<string, CacheFolder> _childFolderNameToCacheFolder;
        private readonly ConcurrentDictionary<string, SystemFile> _childFileNameToFile;
        private long _numberOfPages = -1;
        private readonly CacheFolder _parent;

        #endregion

        #region Ctor

        public CacheFolder(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            string relativePath,
            IConfiguration configuration, 
            CacheFolder parent) :
            base(directoryManager, pathManager, fileManager, relativePath, configuration)
        {
            _parent = parent;

            var allChildFilesResult = base.GetAllChildFilesAsync().Result;

            if (!allChildFilesResult.IsSuccess)
            {
                throw allChildFilesResult.Exception;
            }

            var allChildFoldersResult = base.GetAllChildFolderAsync().Result;

            if (!allChildFoldersResult.IsSuccess)
            {
                throw allChildFoldersResult.Exception;
            }

            _childFileNameToFile = new ConcurrentDictionary<string, SystemFile>(allChildFilesResult.Data
                .ToDictionary(file => file.Name, file => file));

            _childFolderNameToCacheFolder = new ConcurrentDictionary<string, CacheFolder>(allChildFoldersResult.Data
                .ToDictionary(
                    folder => folder.Name, 
                    folder => new CacheFolder(
                        directoryManager,
                        pathManager,
                        fileManager,
                        folder.RelativePath,
                        configuration,
                        this)));
        }

        #endregion

        #region Public

        public override Task<IResult<Void>> LoadFolderPageAsync(int pageNumber)
        {
            if (_pageNumberToContentPage.ContainsKey(pageNumber))
            {
                return VoidSuccess();
            }
            
            Task.Run(async () =>
            {
                var loadFolderPageResult = await base.LoadFolderPageAsync(pageNumber);

                if (loadFolderPageResult.IsSuccess)
                {
                    _pageNumberToContentPage.AddOrUpdate(
                        CurrentContentPage.PageNumber,
                        CurrentContentPage,
                        (pageNum, oldPage) => CurrentContentPage);
                }
            });

            return VoidFailure($"Could not find {pageNumber}. Please try again...");
        }

        public override Task<IResult<long>> GetNumOfFolderPagesAsync()
        {
            if (_numberOfPages > -1)
            {
                return Success(_numberOfPages);
            }

            Task.Run(async () =>
            {
                var numberOfPagesResult = await base.GetNumOfFolderPagesAsync();

                if (numberOfPagesResult.IsSuccess)
                {
                    Interlocked.Exchange(ref _numberOfPages, numberOfPagesResult.Data);
                }
            });

            return Failure<long>($"Could not get number of pages. Please try again...");
        }

        public override Task<IResult<SystemFolder>> AddChildFolderAsync(string name)
        {
            if (_childFolderNameToCacheFolder.TryGetValue(name, out var childFolder))
            {
                return Success<SystemFolder>(childFolder);
            }

            Task.Run(async () =>
            {
                var childFolderResult = await base.AddChildFolderAsync(name);

                if (childFolderResult.IsSuccess)
                {
                    var childCacheFolder = new CacheFolder(
                        DirectoryManager, 
                        PathManager, 
                        FileManager,
                        childFolderResult.Data.RelativePath,
                        Configuration,
                        this);

                    _childFolderNameToCacheFolder.TryAdd(name, childCacheFolder);
                }
            });

            return Failure<SystemFolder>($"Could not add child folder with name {name} to {RelativePath}. Please try again...");
        }

        public override Task<IResult<Void>> DeleteAsync()
        {
            _parent._childFolderNameToCacheFolder.TryRemove(this.Name, out var child);

            ClearCache();

            return base.DeleteAsync();
        }

        public override Task<IResult<SystemFile>> AddFileAsync(Stream stream, string name, bool leaveOpen = false)
        {
            if (_childFileNameToFile.TryGetValue(name, out var file))
            {
                return Success(file);
            }

            Task.Run(async () =>
            {
                var addFileResult = await base.AddFileAsync(stream, name, leaveOpen);

                if (addFileResult.IsSuccess)
                {
                    _childFileNameToFile.TryAdd(name, addFileResult.Data);
                }
            });

            return Failure<SystemFile>($"Could not add child file with name {name} to {RelativePath}. Please try again...");
        }

        public override Task<IResult<SystemFolder>> GetChildFolderAsync(string name)
        {
            if (_childFolderNameToCacheFolder.TryGetValue(name, out var childFolder))
            {
                return Success<SystemFolder>(childFolder);
            }

            Task.Run(async () =>
            {
                var childFolderResult = await base.GetChildFolderAsync(name);

                var childCacheFolder = new CacheFolder(
                    DirectoryManager,
                    PathManager,
                    FileManager,
                    childFolderResult.Data.RelativePath,
                    Configuration,
                    this);

                if (childFolderResult.IsSuccess)
                {
                    _childFolderNameToCacheFolder.TryAdd(name, childCacheFolder);
                }
            });

            return Failure<SystemFolder>($"Could not get child folder with name {name} from {RelativePath}. Please try again...");
        }

        public override Task<IResult<SystemFile>> GetChildFileAsync(string name)
        {
            if (_childFileNameToFile.TryGetValue(name, out var childFile))
            {
                return Success(childFile);
            }

            Task.Run(async () =>
            {
                var childFileResult = await base.GetChildFileAsync(name);

                if (childFileResult.IsSuccess)
                {
                    _childFileNameToFile.TryAdd(name, childFileResult.Data);
                }
            });

            return Failure<SystemFile>($"Could not get child file with name {name} from {RelativePath}. Please try again...");
        }

        public override Task<IResult<IEnumerable<SystemFile>>> GetAllChildFilesAsync()
        {
            return Success<IEnumerable<SystemFile>>(_childFileNameToFile.Values);
        }

        public override Task<IResult<IEnumerable<SystemFolder>>> GetAllChildFolderAsync()
        {
            return Success<IEnumerable<SystemFolder>>(_childFolderNameToCacheFolder.Values);
        }

        public override Task<IResult<IEnumerable<ContentBase>>> GetAllChildrenAsync()
        {
            var childFolders = GetAllChildFilesAsync().Result.Data;
            var childFiles = GetAllChildFilesAsync().Result.Data;

            return Success<IEnumerable<ContentBase>>(childFolders.Union(childFiles));
        }

        public override Task<IResult<Void>> CopyToAsync(SystemFolder destFolder)
        {
            var destCacheFolder = GetMatchedCacheFolder(destFolder, GetRootCacheFolder());

            if (destCacheFolder._childFolderNameToCacheFolder.ContainsKey(Name))
            {
                return VoidSuccess();
            }

            Task.Run(async () =>
            {
                var copyToResult = await base.CopyToAsync(destFolder);

                if (copyToResult.IsSuccess)
                {
                    destCacheFolder._childFolderNameToCacheFolder.TryAdd(Name, this);
                }
            });

            return VoidFailure($"Could not copy folder with name {Name} to {destFolder.RelativePath}. Please try again...");
        }

        public IResult<Void> ClearCache()
        {
            _pageNumberToContentPage.Clear();
            _childFolderNameToCacheFolder.Clear();
            _childFileNameToFile.Clear();

            Interlocked.Exchange(ref _numberOfPages, -1);

            return new SuccessResult();
        }

        #endregion

        #region Private

        private Task<IResult<T>> Success<T>(T returnObject)
        {
            return Task.FromResult<IResult<T>>(new SuccessResult<T>(returnObject));
        }

        private Task<IResult<Void>> VoidSuccess()
        {
            return Task.FromResult<IResult<Void>>(new SuccessResult<Void>(null));
        }

        private Task<IResult<T>> Failure<T>(string message)
        {
            return Task.FromResult<IResult<T>>(
                new FailureResult<T>(
                    new CacheMissException(message)));
        }

        private Task<IResult<Void>> VoidFailure(string message)
        {
            return Task.FromResult<IResult<Void>>(
                new FailureResult<Void>(
                    new CacheMissException(message)));
        }

        private CacheFolder GetRootCacheFolder()
        {
            CacheFolder current = this;

            while (current._parent != null)
            {
                current = current._parent;
            }

            return current;
        }

        private CacheFolder GetMatchedCacheFolder(SystemFolder folderToFind, CacheFolder folderToLookIn)
        {
            if (folderToLookIn._childFolderNameToCacheFolder.TryGetValue(folderToFind.Name, out var cacheFolder) &&
                cacheFolder.Equals(folderToFind))
            {
                return cacheFolder;
            }

            return folderToLookIn._childFolderNameToCacheFolder.Values
                .Select(childFolder => GetMatchedCacheFolder(folderToFind, childFolder))
                .FirstOrDefault();
        }

        #endregion

    }
}
