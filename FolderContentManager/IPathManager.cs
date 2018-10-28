using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentManager
{
    public interface IPathManager
    {
        string Combine(params string[] paths);

        string GetTempPath();
    }
}
