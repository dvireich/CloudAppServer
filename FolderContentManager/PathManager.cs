using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = Pri.LongPath.Path;
namespace FolderContentManager
{
    public class PathManager : IPathManager
    {
        public string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public string GetTempPath()
        {
            return Path.GetTempPath();
        }
    }
}
