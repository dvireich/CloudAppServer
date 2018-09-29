using System;
using System.Threading;
using DBManager;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Diagnostics.Backends.Log4Net;
using ServiceLoader;
using ServiceLoadTaskQueue;

namespace Program
{

    public static class Program
    {
        [Log]
        private static void CreateDataBase()
        {
            using (var createDb = new CreateDBHandler())
            {
                createDb.CreateDataBase();
            }
        }

        [Log]
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


                    var userData = serviceLoadTaskQueue.GetNextTask();
                    ServiceLoaderHelper.LoadCloudAppService(userData.Id, userData.OnRemove);
                }
            }
            finally
            {
                ServiceLoaderHelper.CloseAllChannels();
            }
        }

        [Log(AttributeExclude = true)]
        static void Main(string[] args)
        {
            InitializeLoggingBackend();
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

        [Log(AttributeExclude = true)]
        public static void InitializeLoggingBackend()
        {
            log4net.Config.XmlConfigurator.Configure();
            var log4NetLoggingBackend = new Log4NetLoggingBackend();
            LoggingServices.DefaultBackend = log4NetLoggingBackend;
        }
    }
}