using FolderContentManager.Model;

namespace FolderContentManager.Helpers
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
