using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentManager.Helpers.Path_helpers;
using ContentManager.Helpers.Result;
using ContentManager.Model;
using ContentManager.Model.FolderProviders;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager
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

        public async Task<IResult<Void>> CreateFolderAsync(string name, string path)
        {
            var folder = FolderProvider.GetFolder(path);

            var addChildFolderResult = await folder.AddChildFolderAsync(name);

            if (!addChildFolderResult.IsSuccess)
            {
                return new FailureResult(addChildFolderResult.Exception);
            }

            return new SuccessResult();
        }

        #endregion
    }
}
