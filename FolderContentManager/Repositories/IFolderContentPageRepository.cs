using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentManager.Model;

namespace FolderContentManager.Repositories
{
    public interface IFolderContentPageRepository
    {
        IFolderPage GetFolderPage(string name, string path, int page);

        void CreateOrUpdateFolderPage(string name, string path, int pageNumber, IFolderPage folderPage);

        void DeletePage(string name, string path, int pageNum);

        IEnumerable<IFolderPage> GetAllFolderPages(string name, string path, int numberOfPages);
    }
}
