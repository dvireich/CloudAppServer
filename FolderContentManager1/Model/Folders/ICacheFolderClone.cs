using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManager.Model.Folders
{
    interface ICacheFolderClone
    {
        T Clone<T>() where T : CacheFolder;
    }
}
