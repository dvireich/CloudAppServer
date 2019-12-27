using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.Enums;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Model.Folders
{
    public class Folder : SystemFolder
    {
        #region Consts

        private const string MetaDataFile = nameof(MetaDataFile);

        #endregion

        #region Members

        public SortType SortType
        {
            get => _sortType;
            set
            {
                _sortType = value;
                _isModified = true;
            }
        }

        public int NumberOfElementToShowOnPage
        {
            get => _numberOfElementToShowOnPage;
            set
            {
                _numberOfElementToShowOnPage = value;
                _isModified = true;
            }
        }

        public ContentPage CurrentContentPage => CurrentPage;

        private bool _isModified;
        private SortType _sortType;
        private int _numberOfElementToShowOnPage;
        protected ContentPage CurrentPage;

        #endregion

        #region Ctor

        public Folder(
            IDirectoryManagerAsync directoryManager, 
            IPathManager pathManager, 
            IFileManagerAsync fileManager,
            string relativePath,
            IConfiguration configuration)
            : base(directoryManager, pathManager, fileManager, relativePath, configuration)
        {
            var metaDataResult = LoadMetaDataAsync().Result;

            if (!metaDataResult.IsSuccess)
            {
                Trace.TraceError($"Could not load folder metadata for folder with path '{FullPath}' with the following exception: {metaDataResult.Exception}");
                NumberOfElementToShowOnPage = configuration.DefaultNumberOfElementToShowOnPage;

                return;
            }

            var folderMetaData = metaDataResult.Data;
            NumberOfElementToShowOnPage = folderMetaData.NumberOfPagesPerPage;
            SortType = folderMetaData.SortType;
        }

        #endregion

        #region Public methods

        public virtual async Task<IResult<Void>> LoadFolderPageAsync(int pageNumber)
        {
            var allChildrenResult = await GetAllChildrenAsync();

            if (!allChildrenResult.IsSuccess)
            {
                return new FailureResult(allChildrenResult.Exception);
            }

            try
            {
                var sortedChildren = allChildrenResult.Data.ToList();
                sortedChildren.RemoveAll(child => child.Name == MetaDataFile);
                sortedChildren.Sort((child1, child2) => string.Compare(child1.RelativePath, child2.RelativePath, StringComparison.Ordinal));

                var page = sortedChildren
                    .Skip(NumberOfElementToShowOnPage * (pageNumber - 1))
                    .Take(NumberOfElementToShowOnPage).ToList();

                CurrentPage = new ContentPage(pageNumber);
                CurrentPage.AddRange(page);

                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new FailureResult(e);
            }
        }

        public virtual async Task<IResult<long>> GetNumOfFolderPagesAsync()
        {
            var allChildrenResult = await GetAllChildrenAsync();

            if (!allChildrenResult.IsSuccess)
            {
                return new FailureResult<long>(allChildrenResult.Exception);
            }

            try
            {
                var numOfPages = (long)Math.Ceiling(allChildrenResult.Data.Count() / (double)NumberOfElementToShowOnPage);
                numOfPages = Math.Max(1, numOfPages);

                return new SuccessResult<long>(numOfPages);
            }
            catch (Exception e)
            {
                return new FailureResult<long>(e);
            }
        }

        public async Task<IResult<Void>> SaveAsync()
        {
            if (!_isModified) return new SuccessResult();

            var saveResult = await SaveMetaDataAsync();

            if (!saveResult.IsSuccess)
            {
                return new FailureResult(saveResult.Exception);
            }

            return new SuccessResult();
        }

        #endregion

        #region Private methods

        private async Task<IResult<FolderMetadata>> LoadMetaDataAsync()
        {
            var getMetaDataResult = await GetChildFileAsync(MetaDataFile);

            if (!getMetaDataResult.IsSuccess)
            {
                return new FailureResult<FolderMetadata>(getMetaDataResult.Exception);
            }

            var metaDataFile = getMetaDataResult.Data;
            var streamResult = await metaDataFile.GetStreamAsync();

            if (!streamResult.IsSuccess)
            {
                return new FailureResult<FolderMetadata>(streamResult.Exception);
            }

            StreamReader streamReader = null;

            try
            {
                streamResult.Data.Position = 0;
                streamReader = new StreamReader(streamResult.Data);
                var serializeFolderMetaData = streamReader.ReadToEnd();
                var serializer = new JavaScriptSerializer();
                var folderMetaData = serializer.Deserialize<FolderMetadata>(serializeFolderMetaData);

                return new SuccessResult<FolderMetadata>(folderMetaData);
            }
            catch (Exception e)
            {
                return new FailureResult<FolderMetadata>(e);
            }
            finally
            {
                streamResult.Data.Dispose();
                streamReader?.Dispose();
            }
        }

        private async Task<IResult<Void>> SaveMetaDataAsync()
        {
            var serializer = new JavaScriptSerializer();
            var folderMetaData = new FolderMetadata()
            {
                SortType = SortType,
                NumberOfPagesPerPage = NumberOfElementToShowOnPage,
                Path = RelativePath,
                Name = Name
            };

            var serializeFolderMetaData = serializer.Serialize(folderMetaData);
            var byteArray = Encoding.ASCII.GetBytes(serializeFolderMetaData);
            var stream = new MemoryStream(byteArray);

            var addFileResult =  await AddFileAsync(stream, MetaDataFile);

            if (!addFileResult.IsSuccess)
            {
                return new FailureResult(addFileResult.Exception);
            }

            return new SuccessResult();
        }

        #endregion
    }
}
