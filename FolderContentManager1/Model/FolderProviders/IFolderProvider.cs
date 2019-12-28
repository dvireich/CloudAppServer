using ContentManager.Helpers.Result;
using ContentManager.Model.Folders;

namespace ContentManager.Model.FolderProviders
{
    public interface IFolderProvider<T> where T : Folder
    {
        IResult<T> GetFolder(string path);
    }
}
