using System.Collections.Concurrent;
using FolderContentManager.Model;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;

namespace FolderContentManager.Helpers
{
    [Log(AttributeTargetElements = MulticastTargets.Method, AttributeTargetTypeAttributes = MulticastAttributes.Public, AttributeTargetMemberAttributes = MulticastAttributes.Public)]
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
