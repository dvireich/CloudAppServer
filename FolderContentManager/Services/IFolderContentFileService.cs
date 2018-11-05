using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;

namespace FolderContentManager.Services
{
    public interface IFolderContentFileService
    {
        void CreateFile(string name, string path, string fileType, string tmpCreationPath, long size);

        void DeleteFile(string name, string path, int page);

        Stream GetFile(string name, string path);

        IFile GetFolderContentFile(string name, string path);

        void MoveFileToNewLocation(string sourceName, string sourcePath, string destName, string destPath);

        void UpdateFolderContentFile(IFile file);

        void RenameFile(string oldName, string newName, string path);

        void Copy(string copyFromName, string copyFromPath, string copyToName, string copyToPath);
    }
}
