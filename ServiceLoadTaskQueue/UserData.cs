using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLoadTaskQueue
{
    public class UserData
    {
        public string Id;
        public Action OnRemove;
    }
}
