using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBManager
{
    public class User
    {
        public string Id = string.Empty;
        public string UserName = string.Empty;
        public string Password = string.Empty;
        public string SecurityQuestion = string.Empty;
        public string SecurityAnswer = string.Empty;

        public string[] ToStringArray()
        {
            return new string[] { Id,
                                  UserName,
                                  Password,
                                  SecurityQuestion,
                                  SecurityAnswer };
        }
    }
}
