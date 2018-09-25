using CloudAppServer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentHelper.Interfaces;

namespace FolderContentHelper
{
    public class SearchCache : ISearchCache
    {
        private readonly ConcurrentDictionary<string, IFolderContent[]> _strSearchToResult;

        public SearchCache()
        {
            _strSearchToResult = new ConcurrentDictionary<string, IFolderContent[]>();
        }

        public void AddToCache(string strToSearch, IFolderContent[] result)
        {
            _strSearchToResult[strToSearch] = result;
        }

        public IFolderContent[] GetFromCache(string strToSearch)
        {
            return _strSearchToResult[strToSearch];
        }

        public void RemoveFromCache(string strToSearch)
        {
            _strSearchToResult.TryRemove(strToSearch,out var value);
        }

        public void ClearCache()
        {
            _strSearchToResult.Clear();
        }

        public bool Contains(string strToSearch)
        {
            return _strSearchToResult.ContainsKey(strToSearch);
        }
    }
}
