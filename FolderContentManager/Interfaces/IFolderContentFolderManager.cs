using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Model;

namespace FolderContentHelper.Interfaces
{
    public interface IFolderContentFolderManager
    {
        void DeleteFolder(string name, string path, int page);

        void RenameDirectory(string path, string oldName, string newName);

        void UpdateChildrenPath(IFolderContent folderContent, string newPathPrefix, string oldPathPrefix);

        IFolder GetParentFolder(IFolderContent folder);

        string CreateFolderPath(string name, string path);

        void UpdateNextPageToWrite(IFolder folder);

        void CreateFolder(string name, string path);

        void UpdateFolderMetaData(FolderMetadata folderMetadata);
    }
}
