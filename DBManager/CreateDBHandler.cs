
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBManager
{
    public class CreateDBHandler : IDisposable
    {
        private SqlManager sql;

        public CreateDBHandler()
        {
            sql = new SqlManager();
            sql.Connect();
        }

        public void Exit()
        {
            if (sql != null)
                sql.Disconnect();
        }

        public void CreateDataBase()
        {
            sql.CreateDataBase();
        }

        public void Dispose()
        {
            Exit();
            sql = null;
        }
    }
}
