using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentHelper;
using FolderContentHelper.Interfaces;
using FolderContentManager.Helpers;

namespace FolderContentManager.Repositories
{
    public class FolderContentFolderRepository : GenericFolderContentRepository<IFolder, MappableFolder>, IFolderContentFolderRepository
    {
        private readonly IConstance _constance;
        private readonly IDirectoryManager _directoryManager;

        public FolderContentFolderRepository(IConstance constance) : base(constance)
        {
            _constance = constance;
            _directoryManager = new DirectoryManager();
        }

        public IFolder GetFolder(string name, string path)
        {
            return GetByFullPath(name, path, FolderContentType.Folder);
        }

        public void CreateOrUpdateFolder(string name, string path, IFolder folder)
        {
            CreateOrUpdate(folder, name, path, FolderContentType.Folder);
        }

        public void CreateDirectory(string name, string path)
        {
            var directoryPath = CreateDirectoryPath(name, path);
            if (!_directoryManager.Exists(directoryPath))
            {
                _directoryManager.CreateDirectory(directoryPath);
            }
        }

        public void DeleteFolder(string name, string path)
        {
           Delete(name, path, FolderContentType.Folder);
        }

        public void DeleteDirectory(string name, string path)
        {
            _directoryManager.Delete(CreateDirectoryPath(name, path), true);
        }

        public void CopyDirectory(string copyToName, string copyToPath, string copyFromName, string copyFromPath)
        {
            var pathToCopyFrom = CreateDirectoryPath(copyFromName, copyFromPath);
            var pathToCopyTo = CreateDirectoryPath(copyToName, copyToPath);
            _directoryManager.DirectoryCopy(pathToCopyFrom, pathToCopyTo, true);
        }

        public string CreateDirectoryPath(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/', '\\');

            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(name))
            {
                return _constance.BaseFolderPath;
            }

            if (string.IsNullOrEmpty(path))
            {
                return $"{_constance.BaseFolderPath}\\{name}";
            }

            return $"{_constance.BaseFolderPath}\\{path}\\{name}";
        }
    }
}
