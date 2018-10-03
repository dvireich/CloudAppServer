using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using AuthenticationService.AuthenticationModel;
using DBManager;
using DBManager.Interfaces;
using WcfLogger;

namespace AuthenticationService
{
    public class Authentication : IAuthentication
    {
        private readonly IAuthenticationManager _authenticationManager;

        public Authentication(IAuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        public Authentication()
        {
            _authenticationManager = new AuthenticationManager();
        }

        private T Perform<T>(Func<T> task)
        {
            try
            {
                return task();
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        private void Perform(Action task)
        {
            try
            {
                task();
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
        }

        public void GetOptions()
        {
           
        }

        [WcfLogging]
        public string Authenticate(string userName, string password)
        {
            return Perform(()=> _authenticationManager.Authenticate(userName, password));
        }

        [WcfLogging]
        public void Register(RegisterUserArgs args)
        {
            if (args == null) return;
            Perform(() => _authenticationManager.Register(args.UserName, args.Password));
        }

        [WcfLogging]
        public bool IsUserNameTaken(string userName)
        {
            return Perform(() => _authenticationManager.IsUserNameTaken(userName));
        }
    }
}
