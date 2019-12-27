using ContentManager.Helpers.Result;

namespace ContentManager.Model.FolderProviders
{
    public interface IFolderProvider<T>
    {
        IResult<T> GetFolder(string path);
    }
}
