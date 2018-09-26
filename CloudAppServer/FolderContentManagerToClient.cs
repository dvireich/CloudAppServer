using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentHelper;
using FolderContentManager = FolderContentHelper.FolderContentManager;

namespace CloudAppServer
{
    public class FolderContentManagerToClient
    {
        #region Singelton

        private static FolderContentManagerToClient _instance = null;
        private static readonly object Padlock = new object();

        private FolderContentManagerToClient()
        {
            _clientToRemoveAction = new ConcurrentDictionary<string, Action>();
            _folderContentManagerToClient = new ConcurrentDictionary<string, FolderContentHelper.FolderContentManager>();
        }

        public static FolderContentManagerToClient Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new FolderContentManagerToClient());
                }
            }
        }

        #endregion Singelton

        private ConcurrentDictionary<string, FolderContentHelper.FolderContentManager> _folderContentManagerToClient;
        private ConcurrentDictionary<string, Action> _clientToRemoveAction;

        public void AddClient(string id)
        {
            if(_folderContentManagerToClient.ContainsKey(id)) throw new Exception("A client with the same id is already exists!");

            var folderContentManagerConstance = new Constance();
            folderContentManagerConstance.BaseFolderPath = $"{folderContentManagerConstance.BaseFolderPath}\\{id}";
            _folderContentManagerToClient[id] = new FolderContentHelper.FolderContentManager(folderContentManagerConstance);
        }

        public void AddOnRemoveCallBack(string id, Action onRemove)
        {
            if (!_folderContentManagerToClient.ContainsKey(id)) throw new Exception("No client with this id exists!");

            _clientToRemoveAction[id] = onRemove;
        }

        public void RemoveClient(string id)
        {
            _folderContentManagerToClient.TryRemove(id, out var folderContentManager);
            var onRemove = _clientToRemoveAction[id];
            onRemove?.Invoke();
        }

        public FolderContentHelper.FolderContentManager GetClient(string id)
        {
            return _folderContentManagerToClient[id];
        }
    }
}
