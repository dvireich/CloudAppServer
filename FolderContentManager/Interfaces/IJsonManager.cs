using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentManager.Model;

namespace FolderContentManager.Interfaces
{
    public interface IJsonManager
    {
        string BaseFolderPath { get; }
        IFolder GetFolder(string name, string path);

        IFolderContent GetFolderContent(string name, string path, FolderContentType type);

        IFolderPage GetFolderPage(IFolder folder, int page);

        IFolderContent GetFolderIfFolderType(IFolderContent folderContent);

        bool IsFolderContentExist(string name, string path, FolderContentType type);

        bool IsFolderPageExist(string name, string path, int page);

        string CreateJsonPath(string name, string path, FolderContentType type);

        string CreateFolderPageJsonPath(string name, string path, int page);
    }
}
