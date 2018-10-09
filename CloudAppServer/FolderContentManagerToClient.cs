using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderContentHelper;
using FolderContentManager;
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
            _fileServiceToClient = new ConcurrentDictionary<string, FileService>();
            _clientToNumberOfLogins = new ConcurrentDictionary<string, long>();
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

        private readonly ConcurrentDictionary<string, FolderContentHelper.FolderContentManager> _folderContentManagerToClient;
        private readonly ConcurrentDictionary<string, FileService> _fileServiceToClient;
        private readonly ConcurrentDictionary<string, Action> _clientToRemoveAction;
        private readonly ConcurrentDictionary<string, long> _clientToNumberOfLogins;

        public void AddClient(string id)
        {
            if (!_clientToNumberOfLogins.ContainsKey(id))
            {
                _clientToNumberOfLogins[id] = 0;
            }

            _clientToNumberOfLogins[id]++;;
            if (_clientToNumberOfLogins[id] > 1) return;

            var folderContentManagerConstance = new Constance();
            folderContentManagerConstance.BaseFolderPath = $"{folderContentManagerConstance.BaseFolderPath}\\{id}";
            var folderContentConcurrentManager = new FolderContentConcurrentManager(folderContentManagerConstance);
            _folderContentManagerToClient[id] = new FolderContentHelper.FolderContentManager(folderContentManagerConstance, folderContentConcurrentManager);
            _fileServiceToClient[id] = new FileService(folderContentConcurrentManager);
        }

        public void AddOnRemoveCallBack(string id, Action onRemove)
        {
            if (!_folderContentManagerToClient.ContainsKey(id)) throw new Exception("No client with this id exists!");

            _clientToRemoveAction[id] = onRemove;
        }

        public void RemoveClient(string id)
        {
            if (_clientToNumberOfLogins.ContainsKey(id))
            {
                _clientToNumberOfLogins[id]--;
                if (_clientToNumberOfLogins[id] > 0) return;
                _clientToNumberOfLogins.TryRemove(id, out var userId);
            }

            _folderContentManagerToClient.TryRemove(id, out var folderContentManager);
            _fileServiceToClient.TryRemove(id, out var fileService);
            var onRemove = _clientToRemoveAction[id];
            onRemove?.Invoke();
        }

        public FolderContentHelper.FolderContentManager GetFolderContentManager(string id)
        {
            return _folderContentManagerToClient[id];
        }

        public FileService GetFileService(string id)
        {
            return _fileServiceToClient[id];
        }

        public bool NeedToCreateService(string id)
        {
            if (_clientToNumberOfLogins.ContainsKey(id))
            {
                return _clientToNumberOfLogins[id] < 2;
            }

            return true;
        }
    }
}
