using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAppServer.Model;
using FolderContentHelper.Interfaces;
using FolderContentHelper.Model;
using FolderContentHelper.Model.RestObject;

namespace FolderContentManager.Repositories
{
    public class FolderContentPageRepository : GenericFolderContentRepository<IFolderPage, MappableFolderPage>, IFolderContentPageRepository
    {
        private readonly IConstance _constance;

        public FolderContentPageRepository(IConstance constance) : base(constance)
        {
            _constance = constance;
        }

        public IFolderPage GetFolderPage(string name, string path, int pageNumber)
        {
            return GetByFullPath(CreateFolderPageJsonPath(name, path, pageNumber));
        }

        public void CreateOrUpdateFolderPage(string name, string path, int pageNumber, IFolderPage folderPage)
        {
            CreateOrUpdate(folderPage, CreateFolderPageJsonPath(name, path, pageNumber));
        }

        public void DeletePage(string name, string path, int pageNum)
        {
            Delete(CreateFolderPageJsonPath(name, path, pageNum));
        }

        private string CreateFolderPageJsonPath(string name, string path, int page)
        {
            ConvertNameAndPathToLower(name, path, out var lowerName, out var lowerPath);
            lowerPath = lowerPath.Replace('/', '\\');
            return string.IsNullOrEmpty(path) ?
                $"{_constance.BaseFolderPath}\\{lowerName}{FolderContentType.FolderPage.ToString()}.{page}.json" :
                $"{_constance.BaseFolderPath}\\{lowerPath}\\{lowerName}{FolderContentType.FolderPage.ToString()}.{page}.json";
        }
    }
}
