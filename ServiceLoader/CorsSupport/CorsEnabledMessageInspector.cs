using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLoader
{
    class CorsEnabledMessageInspector : IDispatchMessageInspector
    {
        private List<string> corsEnabledOperationNames;

        public CorsEnabledMessageInspector(List<OperationDescription> corsEnabledOperations)
        {
            corsEnabledOperationNames = corsEnabledOperations.Select(o => o.Name).ToList();
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            HttpRequestMessageProperty httpProp = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
            object operationName;
            request.Properties.TryGetValue(WebHttpDispatchOperationSelector.HttpOperationNamePropertyName, out operationName);
            if (httpProp != null && operationName != null && this.corsEnabledOperationNames.Contains((string)operationName))
            {
                string origin = httpProp.Headers[CorsConstants.Origin];
                if (origin != null)
                {
                    return origin;
                }
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            HttpResponseMessageProperty httpProp = null;
            if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
            {
                httpProp = (HttpResponseMessageProperty) reply.Properties[HttpResponseMessageProperty.Name];
            }
            else
            {
                httpProp = new HttpResponseMessageProperty();
                reply.Properties.Add(HttpResponseMessageProperty.Name, httpProp);
            }

            httpProp.Headers.Add(CorsConstants.AccessControlAllowOrigin, CorsConstants.AllowedOriginAll);
            httpProp.Headers.Add(CorsConstants.AccessControlAllowMethods, CorsConstants.AllowedMethods);
            httpProp.Headers.Add(CorsConstants.AccessControlAllowHeaders, CorsConstants.AllowedHeaders);
            httpProp.Headers.Add(CorsConstants.AccessControlAllowCredentials, "True");
        }
    }
}
