using System;
using System.Linq;
using ContentManager.Helpers.Result;
using Path = Pri.LongPath.Path;
namespace ContentManager.Helpers.Path_helpers
{
    public class PathManager : IPathManager
    {
        public IResult<string> Combine(params string[] paths)
        {
            try
            {
                return new SuccessResult<string>(Path.Combine(paths));
            }
            catch (Exception e)
            {
                return new FailureResult<string>(e);
            }
        }

        public IResult<string> GetTempPath()
        {
            try
            {
                return new SuccessResult<string>(Path.GetTempPath());
            }
            catch (Exception e)
            {
                return new FailureResult<string>(e);
            }
        }

        public IResult<string> GetFileExtension(string fileName)
        {
            try
            {
                return new SuccessResult<string>(Path.GetExtension(fileName));
            }
            catch (Exception e)
            {
                return new FailureResult<string>(e);
            }
        }

        public IResult<string> GetParent(string path)
        {
            try
            {
                string lastPart = path.TrimEnd('\\').Split('\\').Last();
                path = path.Substring(0, path.Length -lastPart.Length);
                path = path.TrimEnd('\\');

                return new SuccessResult<string>(path);
            }
            catch (Exception e)
            {
                return new FailureResult<string>(e);
            }
        }
    }
}
