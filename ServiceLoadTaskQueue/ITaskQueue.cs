using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLoadTaskQueue
{
    public interface ITaskQueue
    {
        void AddToTaskQueue(string id);

        string GetNextTask();

        bool Any();
    }
}
