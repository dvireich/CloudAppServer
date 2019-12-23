using ContentManager.Helpers.Result;

namespace ContentManager.Helpers.Path_helpers
{
    public interface IPathManager
    {
        IResult<string> Combine(params string[] paths);

        IResult<string> GetTempPath();

        IResult<string> GetFileExtension(string fileName);

        IResult<string> GetParent(string path);
    }
}
