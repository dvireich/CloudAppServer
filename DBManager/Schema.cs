using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBManager
{
    static class Schema
    {
        public enum Tables
        {
            Users,
        }

        public enum Columns
        {
            Id,
            UserName,
            Password,
            SecurityQuestion,
            SecurityAnswer
        }

        public enum ColumnType
        {
            Char
        }

        private static List<SchemaEntry> UserSchema = new List<SchemaEntry>()
        {
            new SchemaEntry{TableName = Tables.Users.ToString(), ColumnName = Columns.Id.ToString(), ColumnType = ColumnType.Char.ToString() ,ColumnLength = "50"},
            new SchemaEntry{TableName = Tables.Users.ToString(), ColumnName = Columns.UserName.ToString(), ColumnType = ColumnType.Char.ToString() ,ColumnLength = "50"},
            new SchemaEntry{TableName = Tables.Users.ToString(), ColumnName = Columns.Password.ToString(), ColumnType = ColumnType.Char.ToString() ,ColumnLength = "50"},
            new SchemaEntry{TableName = Tables.Users.ToString(), ColumnName = Columns.SecurityQuestion.ToString(), ColumnType = ColumnType.Char.ToString() ,ColumnLength = "50"},
            new SchemaEntry{TableName = Tables.Users.ToString(), ColumnName = Columns.SecurityAnswer.ToString(), ColumnType = ColumnType.Char.ToString() ,ColumnLength = "50"},
        };

        public static Dictionary<string, List<SchemaEntry>> TableNameToSchema = new Dictionary<string, List<SchemaEntry>>()
        {
            {Tables.Users.ToString() , UserSchema}
        };
    }
}