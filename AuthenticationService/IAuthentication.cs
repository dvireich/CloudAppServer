using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using AuthenticationService.AuthenticationModel;

namespace AuthenticationService
{
    [ServiceContract]
    public interface IAuthentication
    {
        [OperationContract]
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*")]
        void GetOptions();

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "/Authenticate/username={userName}&password={password}")]
        string Authenticate(string userName, string password);

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "/Authenticate/SecurityQuestion/username={userName}")]
        string GetSecurityQuestion(string userName);

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "/Authenticate/RestorePassword/username={userName}&securityAnswer={securityAnswer}")]
        string RestorePassword(string userName, string securityAnswer);


        [OperationContract]
        [WebInvoke(
            Method = "*",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/Register")]
        void Register(RegisterUserArgs args);

        [OperationContract]
        [WebGet(
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "/username={userName}")]
        bool IsUserNameTaken(string userName);
    }
}
