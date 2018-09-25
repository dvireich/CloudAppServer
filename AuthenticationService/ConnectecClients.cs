using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService
{
    public class ConnectecClients : IConnectecClients
    {
        private HashSet<string> _connectecClientIds;
        #region Singelton

        private static ConnectecClients _instance = null;
        private static readonly object Padlock = new object();

        private ConnectecClients()
        {
            _connectecClientIds = new HashSet<string>();
        }

        public static ConnectecClients Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new ConnectecClients());
                }
            }
        }

        #endregion Singelton

        public bool Contains(string id)
        {
            return _connectecClientIds.Contains(id);
        }

        public void Add(string id)
        {
            _connectecClientIds.Add(id);
        }

        public void Remove(string id)
        {
            _connectecClientIds.Remove(id);
        }
    }
}
