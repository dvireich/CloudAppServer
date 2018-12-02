using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentManager.Model;

namespace FolderContentManager.Repositories
{
    public interface IFolderContentFolderRepository
    {
        IFolder GetFolder(string name, string path);

        void CreateOrUpdateFolder(string name, string path, IFolder folder);

        void CreateDirectory(string name, string path);

        void DeleteFolder(string name, string path);

        void DeleteDirectory(string name, string path);

        void CopyDirectory(string copyToName, string copyToPath, string copyFromName, string copyFromPath);
    }
}
