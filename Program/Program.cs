using System;
using System.Threading;
using ServiceLoader;

namespace Program
{

    public static class Program
    {

        private static void RunAsConsole()
        {
            try
            {
                ServiceLoaderHelper.LoadBasicServices();
                Console.ReadLine();
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