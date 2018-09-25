using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using AuthenticationService;
using CloudAppServer;

namespace ServiceLoader
{
    public static class ServiceLoaderHelper
    {
        private static readonly List<ServiceHost> OpenChannels = new List<ServiceHost>();

        public static void LoadBasicServices()
        {
            InitializeCORESServiceReferences<Authentication, IAuthentication>("CloudAppServer/Authentication");
        }

        public static void LoadCloudAppService(string id)
        {
            FolderContentManagerToClient.Instance.AddClient(id);
            var sh =InitializeCORESServiceReferences<FolderContentService, IFolderContentService>($"CloudAppServer/{id}");
            FolderContentManagerToClient.Instance.AddOnRemoveCallBack(id, ()=> {sh.Close();});
        }

        private static void InitializeBasicHttpServiceReferences<TC, TI>(string endpointName)
        {
            //Configuring the Shell service
            var binding = new BasicHttpBinding
            {
                Security = {Mode = BasicHttpSecurityMode.None},
                CloseTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                SendTimeout = new TimeSpan(0, 0, 10, 0, 0),
                OpenTimeout = TimeSpan.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferPoolSize = int.MaxValue,
                MaxBufferSize = int.MaxValue
            };
            //Put Public ip of the server computer
            var endpoint = $"http://localhost:80/{endpointName}";
            var uri = new Uri(endpoint);

            var serviceHost = new ServiceHost(typeof(TC), uri);
            var smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
            serviceHost.Description.Behaviors.Add(smb);

            serviceHost.AddServiceEndpoint(typeof(TI), binding, endpoint);
            serviceHost.Open();
            OpenChannels.Add(serviceHost);
        }

        private static ServiceHost InitializeCORESServiceReferences<TC, TI>(string endpointName)
        {
            //Put Public ip of the server computer
            var endpoint = $"http://localhost:80/{endpointName}";
            var uri = new Uri(endpoint);

            var serviceHost = new CorsEnabledServiceHost(typeof(TC), new []{uri});
            serviceHost.Open();
            OpenChannels.Add(serviceHost);
            return serviceHost;
        }

        private static void InitializeWebHttpServiceReferences<TC, TI>(string endpointName)
        {
            //Configuring the Shell service
            var binding = new WebHttpBinding
            {
                Security = {Mode = WebHttpSecurityMode.None},
                CloseTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                SendTimeout = new TimeSpan(0, 0, 10, 0, 0),
                OpenTimeout = TimeSpan.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferPoolSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                
            };
            //Put Public ip of the server computer
            var endpointUri = $"http://localhost:80/{endpointName}";
            var uri = new Uri(endpointUri);

            var serviceHost = new ServiceHost(typeof(TC), uri);
            var smb = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpsGetEnabled = true };
            serviceHost.Description.Behaviors.Add(smb);
            ServiceEndpoint endpoint = serviceHost.AddServiceEndpoint(typeof(TI), binding, endpointUri);
            WebHttpBehavior behavior = new WebHttpBehavior();
            endpoint.Behaviors.Add(behavior);
            serviceHost.Open();
            OpenChannels.Add(serviceHost);
        }

        public static void InitializeTcpServiceReferences<TC, TI>(string endpoint)
        {
            Uri endPointAdress = new Uri($"net.tcp://localhost/{endpoint}");
            NetTcpBinding wsd = new NetTcpBinding();
            wsd.Security.Mode = SecurityMode.None;
            wsd.CloseTimeout = TimeSpan.MaxValue;
            wsd.ReceiveTimeout = TimeSpan.MaxValue;
            wsd.OpenTimeout = TimeSpan.MaxValue;
            wsd.SendTimeout = TimeSpan.MaxValue;
            EndpointAddress ea = new EndpointAddress(endPointAdress);

            var serviceHost = new ServiceHost(typeof(TC), endPointAdress);
            var smb = new ServiceMetadataBehavior();
            serviceHost.Description.Behaviors.Add(smb);
            serviceHost.AddServiceEndpoint(typeof(TI), wsd, endPointAdress);
            Uri mexEndPointAddress = new Uri($"net.tcp://localhost/ShellTrasferServer/{endpoint}/mex");
            serviceHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), mexEndPointAddress);
            serviceHost.Open();
        }

        public static void CloseAllChannels()
        {
            foreach (var channel in OpenChannels)
            {
                channel.Close();
            }
        }
    }
}
