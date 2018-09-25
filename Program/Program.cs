using System;
using System.Threading;
using DBManager;
using ServiceLoader;
using ServiceLoadTaskQueue;

namespace Program
{

    public static class Program
    {
        private static void CreateDataBase()
        {
            using (var createDb = new CreateDBHandler())
            {
                createDb.CreateDataBase();
            }
        }

        private static void RunAsConsole()
        {
            try
            {
                var serviceLoadTaskQueue = TaskQueue.Instance;
                ServiceLoaderHelper.LoadBasicServices();
                while (true)
                {
                    if (!serviceLoadTaskQueue.Any())
                    {
                        Thread.Sleep(1000);
                        continue;
                    }


                    var id = serviceLoadTaskQueue.GetNextTask();
                    ServiceLoaderHelper.LoadCloudAppService(id);
                }
            }
            finally
            {
                ServiceLoaderHelper.CloseAllChannels();
            }
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (arg.ToLower() == @"/CreateDb".ToLower())
                    {
                        CreateDataBase();
                        return;
                    }

                    if (arg.ToLower() == @"/Console".ToLower())
                    {
                        RunAsConsole();
                        return;
                    }

                }
            }
        }
    }
}