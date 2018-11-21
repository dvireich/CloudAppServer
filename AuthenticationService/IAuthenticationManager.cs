using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService
{
    public interface IAuthenticationManager
    {
        string Authenticate(string userName, string password);

        void Register(string userName, string password, string securityAnswer, string securityQuestion);

        bool IsUserNameTaken(string userName);
    }
}
