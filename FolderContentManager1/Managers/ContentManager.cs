using System.Threading.Tasks;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model.FolderProviders;
using ContentManager.Model.Folders;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Managers
{
    public class ContentManager<T> where T : Folder
    {
        #region Members

        protected readonly IFolderProvider<T> FolderProvider;
        protected readonly IPathManager PathManager;

        #endregion

        #region Ctor

        public ContentManager(IFolderProvider<T> folderProvider, IPathManager pathManager)
        {
            FolderProvider = folderProvider;
            PathManager = pathManager;
        }

        #endregion

        #region Public

        public async Task<IResult<Void>> CreateFolderAsync(
            string name, 
            string path, 
            IFolderProvider<T> folderProvider = null)
        {
            folderProvider = folderProvider ?? FolderProvider;

            var folderResult = folderProvider.GetFolder(path);

            if (!folderResult.IsSuccess)
            {
                return new FailureResult(folderResult.Exception);
            }

            var addChildFolderResult = await folderResult.Data.AddChildFolderAsync(name);

            if (!addChildFolderResult.IsSuccess)
            {
                return new FailureResult(addChildFolderResult.Exception);
            }

            return new SuccessResult();
        }

        #endregion
    }
}
