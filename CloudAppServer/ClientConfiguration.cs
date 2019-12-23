using System;
using System.Collections.Concurrent;
using ContentManager.Helpers;
using ContentManager.Helpers.Configuration;
using ContentManager.Helpers.Directory_helpers;
using ContentManager.Helpers.File_helpers;
using ContentManager.Helpers.Path_helpers;
using PathManager = ContentManager.Helpers.Path_helpers.PathManager;

namespace CloudAppServer
{
    public class ClientConfiguration
    {
        #region Singelton

        private static ClientConfiguration _instance = null;
        private static readonly object Padlock = new object();

        private ClientConfiguration()
        {
            _clientToNumberOfLogins = new ConcurrentDictionary<string, long>();
            _clientToRemoveAction = new ConcurrentDictionary<string, Action>();
            _folderContentManagerToClient = new ConcurrentDictionary<string, IConfiguration>();
        }

        public static ClientConfiguration Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new ClientConfiguration());
                }
            }
        }

        #endregion Singelton

        private readonly ConcurrentDictionary<string, IConfiguration> _folderContentManagerToClient;
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

            var configuration = new Configuration {BaseFolderName = id};
            configuration.HomeFolderPath = $"{configuration.BaseFolderPath}\\{configuration.BaseFolderName}";
            _folderContentManagerToClient[id] = configuration;
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
            var onRemove = _clientToRemoveAction[id];
            onRemove?.Invoke();
        }

        public IConfiguration GetConfiguration(string id)
        {
            return _folderContentManagerToClient[id];
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
