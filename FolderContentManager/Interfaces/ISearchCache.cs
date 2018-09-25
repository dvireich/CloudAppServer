using CloudAppServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderContentHelper.Interfaces
{
    public interface ISearchCache
    {
        void AddToCache(string strToSearch, IFolderContent[] result);

        void RemoveFromCache(string strToSearch);

        void ClearCache();

        IFolderContent[] GetFromCache(string strToSearch);

        bool Contains(string strToSearch);
    }
}
