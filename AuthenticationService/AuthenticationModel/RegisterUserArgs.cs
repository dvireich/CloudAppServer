using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService.AuthenticationModel
{
    [DataContract]
    public class RegisterUserArgs
    {
        [DataMember]
        public string UserName;
        [DataMember]
        public string Password;
    }
}
