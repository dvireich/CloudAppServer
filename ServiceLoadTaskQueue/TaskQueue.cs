using System;
using System.Collections.Concurrent;

namespace ServiceLoadTaskQueue
{
    public class TaskQueue : ITaskQueue
    {
        private static volatile TaskQueue _instance;
        private static readonly object SyncRoot = new object();

        private readonly ConcurrentQueue<string> _taskQueue = new ConcurrentQueue<string>();

        public void AddToTaskQueue(string id)
        {
            _taskQueue.Enqueue(id);
        }

        public string GetNextTask()
        {
            _taskQueue.TryDequeue(out var task);
            return task;
        }

        public bool Any()
        {
            return _taskQueue.Count > 0;
        }

        private TaskQueue() { }

        public static TaskQueue Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new TaskQueue();
                }

                return _instance;
            }
        }
    }
}