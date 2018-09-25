
using DBManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBManager
{
     public class UserRepository : IUserRepository
    {
        SqlManager sql;

        readonly string[] userCol = new string[]
        {
            Schema.Columns.Id.ToString(),
            Schema.Columns.UserName.ToString(),
            Schema.Columns.Password.ToString(),
            Schema.Columns.SecurityQuestion.ToString(),
            Schema.Columns.SecurityAnswer.ToString(),
        };

        public UserRepository()
        {
            sql = new SqlManager();
            sql.Connect();
        }

        public void Exit()
        {
            if (sql != null)
                sql.Dispose();
        }

        public bool CheckUserNameExists(string userName)
        {
            var resp = sql.GetRecord(Schema.Tables.Users.ToString(), Schema.Columns.UserName.ToString(), userName);
            return resp != null && resp.Length > 0;
        }

        public bool CheckUserExists(User user)
        {
            var resp = sql.GetRecord(Schema.Tables.Users.ToString(), Schema.Columns.Id.ToString(), user.Id);
            return resp != null && resp.Length > 0;
        }

        public bool CheckUserNameAndPassword(string userName, string password, out User user)
        {
            user = null;
            if (!CheckUserNameExists(userName)) return false;
            var userList = new List<User>();
            var resp = sql.GetRecord(Schema.Tables.Users.ToString(), Schema.Columns.UserName.ToString(), userName);
            for(int i = 0; i < resp.Length; i = i + 5)
            {
                userList.Add(new User()
                {
                    Id = resp[i].Trim(),
                    UserName = resp[i + 1].Trim(),
                    Password = resp[i + 2].Trim(),
                    SecurityAnswer = resp[i + 3].Trim(),
                    SecurityQuestion = resp[i + 4].Trim(),
                });
            }

            user = userList.FirstOrDefault(u => u.Password == password);

            return user != null;
        }

        public void SaveUserInDb(User user)
        {
            if(CheckUserExists(user))
            {
                sql.UpdateDataBaseRecord(Schema.Tables.Users.ToString(), user.Id, userCol, user.ToStringArray());
            }
            else
            {
                sql.InsertIntoDataBase(Schema.Tables.Users.ToString(), user.ToStringArray());
            }
        }

        public void DeleteUserFromDb(User user)
        {
            if (!CheckUserExists(user)) return;

            sql.DeleteRecord(Schema.Tables.Users.ToString(), user.Id);
        }

        public User GetUserByUserName(string userName)
        {
            if (!CheckUserNameExists(userName)) return null;

            var resp = sql.GetRecord(Schema.Tables.Users.ToString(), Schema.Columns.UserName.ToString(), userName);

            return new User()
            {
                Id = resp[0].Trim(),
                UserName = resp[1].Trim(),
                Password = resp[2].Trim(),
                SecurityQuestion = resp[3].Trim(),
                SecurityAnswer = resp[4].Trim()
            };
        }

        public void Dispose()
        {
            Exit();
            sql = null;
        }
    }
}
