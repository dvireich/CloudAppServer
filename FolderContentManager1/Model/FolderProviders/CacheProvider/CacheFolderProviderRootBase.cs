using ContentManager.Model.Folders;

namespace ContentManager.Model.FolderProviders.CacheProvider
{
    public abstract class CacheFolderProviderRootBase
    {
        #region Members

        private static volatile CacheFolder _root;
        private static volatile object _rootLock = new object();

        protected static CacheFolder Root
        {
            get => _root;
            set
            {
                if (_root == null)
                {
                    lock (_rootLock)
                    {
                        if(_root == null)
                        {
                            _root = value;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
