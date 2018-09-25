
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DBManager
{
    internal class SqlManager : IDisposable
    {
        private SqlConnection cnn;

        private readonly string dbName = "CloudApp";

        public string TableNameFormat(string tableName)
        {
            return string.Format("[{0}].[dbo].[{1}]", dbName, tableName);
        }

        public void Connect()
        {
            ValidateNoConnectionBeforeConnect();
            var connectionString = ConfigurationManager.ConnectionStrings["SQLExpress"].ConnectionString;
            cnn = new SqlConnection(connectionString);
            try
            {
                cnn.Open();

            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Could not open SQL connection with the connection string {0}", connectionString), e);
            }
        }

        public void Disconnect()
        {
            if (cnn.State == ConnectionState.Open)
                cnn.Close();
        }

        public void CreateDataBase(string path = null)
        {
            ValidateDBConnection();

            var createDatabaseStr = string.IsNullOrEmpty(path) ?
                  string.Format("CREATE DATABASE {0}", dbName) :
                  string.Format("CREATE DATABASE {0} ON PRIMARY ", dbName) +
                  string.Format("(NAME = {0}_Data, ", dbName) +
                  string.Format("FILENAME = '{0}\\{1}.mdf', ", path, dbName) +
                  "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%) " +
                  string.Format("LOG ON (NAME = {0}_Log, ", dbName) +
                  string.Format("FILENAME = '{0}\\{1}.ldf', ", path, dbName) +
                  "SIZE = 1MB, " +
                  "MAXSIZE = 5MB, " +
                  "FILEGROWTH = 10%)";

            var deleteBatabaseStr = string.Format("DROP DATABASE {0}", dbName);

            if (CheckDatabaseExists(dbName))
            {
                using (var createDatabaseCommand = new SqlCommand(deleteBatabaseStr, cnn))
                {
                    createDatabaseCommand.ExecuteNonQuery();
                }
            }

            using (var createDatabaseCommand = new SqlCommand(createDatabaseStr, cnn))
            {
                createDatabaseCommand.ExecuteNonQuery();
            }

            foreach (var schema in Schema.TableNameToSchema)
            {
                StringBuilder command = new StringBuilder();
                command.AppendFormat("CREATE TABLE {0}", TableNameFormat(schema.Key));
                command.AppendFormat("(", schema.Key);
                var lastElement = schema.Value.Last();
                foreach (var col in schema.Value)
                {
                    command.AppendFormat("{0} {1} ({2}) NOT NULL ", col.ColumnName, col.ColumnType, col.ColumnLength);
                    if (col != lastElement)
                        command.Append(",");
                }
                command.Append(") ");

                if (CheckIfTableExsits(TableNameFormat(schema.Key)))
                {
                    var dropTableStr = string.Format("DROP TABLE {0}", TableNameFormat(schema.Key));

                    using (var deleteTableCommand = new SqlCommand(dropTableStr, cnn))
                    {
                        var res = deleteTableCommand.ExecuteNonQuery();
                    }
                }
                using (var createTableCommand = new SqlCommand(command.ToString(), cnn))
                {
                    var res = createTableCommand.ExecuteNonQuery();
                }

                MakeColumnPrimeryKey(schema.Key, Schema.Columns.Id.ToString());
            }
        }

        public void MakeColumnPrimeryKey(string tableName, string column)
        {
            ValidateDBConnection();
            ValidateTableName(tableName);

            var addPrimery = string.Format("ALTER TABLE  {0} ", TableNameFormat(tableName)) +
                             string.Format("ADD PRIMARY KEY ({0}) ", column);

            using (var saveCommand = new SqlCommand(addPrimery, cnn))
            {
                saveCommand.ExecuteNonQuery();
            }
        }

        public void InsertIntoDataBase(string tableName, string[] values)
        {
            ValidateDBConnection();
            ValidateTableName(tableName);
            ValidateNumberOfValsMatchColNum(values.Length, tableName);
            ValidateValsLengthByCol(values, tableName);

            var tableCols = Schema.TableNameToSchema[tableName].Select(se => se.ColumnName).ToArray();
            values = values.Select(val => "'" + val + "'").ToArray();
            var saveValuestr = string.Format("INSERT INTO {0} ", TableNameFormat(tableName)) +
                               string.Format("({0}) ", ToSeperatedCommaList(tableCols)) +
                               string.Format("VALUES ({0})", ToSeperatedCommaList(values));

            using (var saveCommand = new SqlCommand(saveValuestr, cnn))
            {
                saveCommand.ExecuteNonQuery();
            }

        }

        public void UpdateDataBaseRecord(string tableName, string id, string[] columns, string[] values)
        {
            ValidateDBConnection();
            ValidateNumberModifiedColEqualToNumberOfVals(columns, values);
            ValidateNumOfColsOrValsNotZero(columns, values);
            ValidateTableName(tableName);

            StringBuilder command = new StringBuilder();
            command.AppendFormat("UPDATE {0} ", TableNameFormat(tableName));
            command.Append("SET ");
            var zippedColVals = columns.Zip(values, (col, val) => new Tuple<string, string>(col, val)).ToList();
            var last = zippedColVals.Last();
            foreach (var colVal in zippedColVals)
            {
                command.AppendFormat("{0} = '{1}' ", colVal.Item1, colVal.Item2);
                if (colVal != last)
                    command.Append(",");
            };
            command.AppendFormat(" WHERE Id = '{0}';", id);

            using (var updateCommand = new SqlCommand(command.ToString(), cnn))
            {
                updateCommand.ExecuteNonQuery();
            }
        }

        public string[] GetRecord(string tableName, string colName, string colVal)
        {
            ValidateDBConnection();
            ValidateTableName(tableName);

            var SelectRecord = string.Format("SELECT * FROM {0} ", TableNameFormat(tableName)) +
                               string.Format("WHERE {0} = '{1}'", colName, colVal);


            using (var updateCommand = new SqlCommand(SelectRecord, cnn))
            {
                using (var reader = updateCommand.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    var readList = new List<string>();
                    for (int i = 0; i < Schema.TableNameToSchema[tableName].Count; i++)
                    {
                        readList.Add(reader.GetString(i));
                    }

                    if (colName == "Id" && reader.Read())
                        throw new InvalidOperationException("Multiple records were returned. Id is not unique!");

                    return readList.ToArray();
                }
            }
        }

        public void DeleteRecord(string tableName, string id)
        {
            ValidateDBConnection();
            ValidateTableName(tableName);

            var deleteStr = string.Format("DELETE FROM {0} ", TableNameFormat(tableName)) +
                            string.Format("WHERE Id = '{0}'", id);

            using (var updateCommand = new SqlCommand(deleteStr, cnn))
            {
                updateCommand.ExecuteNonQuery();
            }

        }

        private bool CheckDatabaseExists(string databaseName)
        {
            ValidateDBConnection();

            bool result = false;

            try
            {
                var sqlCreateDBQuery = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName);

                using (SqlCommand sqlCmd = new SqlCommand(sqlCreateDBQuery, cnn))
                {

                    object resultObj = sqlCmd.ExecuteScalar();

                    int databaseID = 0;

                    if (resultObj != null)
                    {
                        int.TryParse(resultObj.ToString(), out databaseID);
                    }
                    result = (databaseID > 0);
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private bool CheckIfTableExsits(string tableName)
        {
            ValidateDBConnection();

            bool exists;

            try
            {
                // ANSI SQL way.  Works in PostgreSQL, MSSQL, MySQL.  
                var sqlSelectDBQuery = string.Format(
                  "select case when exists((select * from information_schema.tables where table_name = '" + tableName + "')) then 1 else 0 end");

                using (SqlCommand sqlCmd = new SqlCommand(sqlSelectDBQuery, cnn))
                {
                    exists = (int)sqlCmd.ExecuteScalar() == 1;
                }
            }
            catch
            {
                try
                {
                    // Other RDBMS.  Graceful degradation
                    exists = true;
                    var sqlSelectDBQueryOthers = string.Format("select 1 from " + tableName + " where 1 = 0");
                    using (SqlCommand sqlCmd = new SqlCommand(sqlSelectDBQueryOthers, cnn))
                    {
                        sqlCmd.ExecuteScalar();
                    }
                }
                catch
                {
                    exists = false;
                }
            }
            return exists;
        }

        public string ToSeperatedCommaList(string[] values)
        {
            return string.Join(", ", values);
        }

        //Validations

        private void ValidateNumberModifiedColEqualToNumberOfVals(string[] columns, string[] values)
        {
            if (columns.Length != values.Length)
                throw new Exception("Columns number and values number are not the same!");
        }

        private void ValidateNumOfColsOrValsNotZero(string[] columns, string[] values)
        {
            if (columns.Length == 0 || values.Length == 0)
                throw new Exception("Cannot update 0 columns or 0 values!");
        }

        private void ValidateTableName(string tableName)
        {
            if (!Schema.TableNameToSchema.ContainsKey(tableName))
                throw new Exception(string.Format("No sutch table!. Table: {0}", tableName));
        }

        private void ValidateDBConnection()
        {
            if (cnn == null)
                throw new Exception("Trying to do operation on database but need to connect to database first!");
        }

        private void ValidateNumberOfValsMatchColNum(int valLen, string tableName)
        {
            if (valLen > Schema.TableNameToSchema[tableName].Count)
                throw new Exception(string.Format("Too less values to this table!. Table: {0} Got: {1} Need: {2}",
                    tableName, valLen, Schema.TableNameToSchema[tableName].Count));
        }

        private void ValidateValsLengthByCol(string[] values, string tableName)
        {
            var colLength = Schema.TableNameToSchema[tableName].Select(se => se.ColumnLength).ToArray();
            for (int i = 0; i < colLength.Length; i++)
            {
                if (int.Parse(colLength[i]) < values[i].Length)
                    throw new Exception(string.Format("Value for this column is too long. Value: {0} Value Length: {1} Table: {2} Column Number: {3}",
                        values[i], values[i].Length, tableName, i));
            }
        }

        private void ValidateNoConnectionBeforeConnect()
        {
            if (cnn != null)
                throw new Exception("Trying open connection to database but a connection is already open!");
        }

        public void Dispose()
        {
            Disconnect();
            cnn = null;
        }
        }
    }

