using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService
{
    public interface IConnectecClients
    {
        bool Contains(string id);

        void Add(string id);

        void Remove(string id);
    }
}
