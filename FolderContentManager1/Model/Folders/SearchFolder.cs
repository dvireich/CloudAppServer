using System;
using System.Linq;
using System.Threading.Tasks;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.Enums;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Model.Folders
{
    public class SearchFolder : Folder
    {
        #region Consts

        private const string FolderName = "search";

        #endregion

        #region Members

        public override string CreationTime { get; }
        public override string ModificationTime { get; }

        #endregion

        #region Ctor

        public SearchFolder(
            IDirectoryManagerAsync directoryManager,
            IPathManager pathManager,
            IFileManagerAsync fileManager,
            string relativePath,
            IConfiguration configuration)
            : base(directoryManager, pathManager, fileManager, relativePath, configuration)
        {
            CreationTime = ModificationTime = $"{DateTime.Now:G}";
            Name = FolderName;
            Type = FolderContentType.FolderPageResult;
        }

        #endregion

        #region Public methods

        public async Task<IResult<Void>> LoadSearchPageAsync(string nameToSearch, int pageNumber)
        {
            RelativePath = nameToSearch;

            var searchResults = await Search(nameToSearch);

            if (!searchResults.IsSuccess)
            {
                return new FailureResult(searchResults.Exception);
            }

            try
            {
                var searchPage = searchResults.Data
                    .Skip(Configuration.DefaultNumberOfElementToShowOnPage * (pageNumber - 1))
                    .Take(Configuration.DefaultNumberOfElementToShowOnPage).ToList();

                CurrentPage = new ContentPage(pageNumber);
                CurrentPage.AddRange(searchPage);

                return new SuccessResult();
            }
            catch (Exception e)
            {
                return new FailureResult(e);
            }
        }

        #endregion

        #region Override

        public override async Task<IResult<long>> GetNumOfFolderPagesAsync()
        {
            var searchResults = await Search(RelativePath);

            if (!searchResults.IsSuccess)
            {
                return new FailureResult<long>(searchResults.Exception);
            }

            try
            {

                var numberOfPages = (long)Math.Ceiling(searchResults.Data.Count() / (double)Configuration.DefaultNumberOfElementToShowOnPage);
                numberOfPages = Math.Max(1, numberOfPages);

                return new SuccessResult<long>(numberOfPages);
            }
            catch (Exception e)
            {
                return new FailureResult<long>(e);
            }
        }

        #endregion
    }
}
