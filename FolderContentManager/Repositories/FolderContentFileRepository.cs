using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentHelper.Interfaces;

namespace FolderContentManager.Repositories
{
    public class FolderContentFileRepository : GenericFolderContentRepository<IFile, MappableFile>, IFolderContentFileRepository
    {
        private readonly IConstance _constance;

        public FolderContentFileRepository(IConstance constance) : base(constance)
        {
            _constance = constance;
        }

        public IFile GetFolderContentFile(string name, string path)
        {
            return GetByFullPath(name, path, FolderContentType.File);
        }

        public void CreateOrUpdateFolderContentFile(string name, string path, IFile file)
        {
            CreateOrUpdate(file, name, path, FolderContentType.File);
        }

        public Stream GetFile(string name, string path)
        {
            return FileManager.GetFile(CreateFilePath(name, path));
        }

        public void Delete(string name, string path)
        {
            //Delete the JSON file
            Delete(name, path, FolderContentType.File);
            //Delete the actual file
            Delete(CreateFilePath(name, path));
        }

        public void Move(string moveToName, string moveToPath, string moveFromFullPath)
        {
            var moveToFullPath = CreateFilePath(moveToName, moveToPath);
            FileManager.Move(moveFromFullPath, moveToFullPath);
        }

        public void Move(string moveToName, string moveToPath, string moveFromName, string moveFromPath)
        {
            var moveToFullPath = CreateFilePath(moveToName, moveToPath);
            var moveFromFullPath = CreateFilePath(moveFromName, moveFromPath);
            FileManager.Move(moveFromFullPath, moveToFullPath);
        }

        public void Copy(string copyToName, string copyToPath, string copyFromName, string copyFromPath)
        {
            var copyToFullPath = CreateFilePath(copyToName, copyToPath);
            var copyFromFullPath = CreateFilePath(copyFromName, copyFromPath);
            FileManager.Copy(copyFromFullPath, copyToFullPath);
        }

        private string CreateFilePath(string name, string path)
        {
            name = name.ToLower();
            path = path.ToLower().Replace('/', '\\');
            return string.IsNullOrEmpty(path) ? $"{_constance.BaseFolderPath}\\{name}" : $"{_constance.BaseFolderPath}\\{path}\\{name}";
        }
    }
}
