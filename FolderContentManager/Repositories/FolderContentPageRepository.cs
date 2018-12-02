using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentManager.Helpers;
using FolderContentManager.Model;
using FolderContentManager.Model.MappableObjects;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentManager.Repositories
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
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

        public IEnumerable<IFolderPage> GetAllFolderPages(string name, string path, int numberOfPages)
        {
            var result = new List<IFolderPage>();
            for (var i = 1; i <= numberOfPages; i++)
            {
                result.Add(GetFolderPage(name, path, i));
            }

            return result;
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
