using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentManager.Interfaces
{
    public interface IDirectoryManager
    {
        void Delete(string path, bool recursive);

        void CreateDirectory(string path);

        bool Exists(string path);

        void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs);
    }
}
