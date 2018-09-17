using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentManager.Interfaces
{
    public interface IFolderContentFileManager
    {
        void CreateFile(string name, string path, string fileType, string[] value, long size);

        void DeleteFile(string name, string path, int page);

        string CreateFilePath(string name, string path);
    }
}
