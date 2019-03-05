using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace VizLaw_api.Data
{
    /// <summary>
    /// Basisklasse für Datenverbindung, welche gleichartige Inhalte haben
    /// </summary>
    public class GenericConnector
    {
        public bool LogSql { get; set; }

        public string DBUser { get; set; }

        public virtual string ConnectionString
        {
            get; set;
        }

        private SqlConnection _sqlConnection;

        public virtual SqlConnection Connection
        {
            get
            {
                try
                {
                    SqlConnection con = _sqlConnection == null ? new SqlConnection(ConnectionString) : _sqlConnection;
                    if (con.State != ConnectionState.Open) con.Open();
                    if (con.State == ConnectionState.Open)
                    {
                        _sqlConnection = con;
                        return con;
                    }

                }
                catch (Exception ex)
                {
                    throw;
                }

                return null;

            }
            set
            {
                _sqlConnection = value;
            }
        }

        public bool SqlConnectionAvailable
        {
            get
            {
                bool success = false;

                try
                {
                    SqlConnection con = new SqlConnection(ConnectionString);
                    con.Open();
                    success = con.State == ConnectionState.Open;
                }
                catch (Exception ex)
                {
                    try
                    {
                        success = Connection.State != ConnectionState.Closed;
                    }
                    catch
                    {

                    }
                }

                return success;
            }
        }

        /// <summary>
        /// Liste aller verfügbaren Datenbanken
        /// </summary>
        public List<String> Databases
        {

            get
            {
                if (Connection.State != ConnectionState.Open) Connection.Open();
                List<string> sources = new List<string>();
                foreach (DataRow item in Connection.GetSchema("Databases").Rows)
                    sources.Add(item[0].ToString());
                return sources;

            }
        }

        public virtual List<string> SqlHistoryLog { get; set; }

        /// <summary>
        /// Führt einen SQL Statement aus und gibt das Ergebnis als DataTable zurück
        /// </summary>
        /// <param name="sql">SQL Statement</param>
        /// <returns>Ergebnis als DataTable</returns>
        public virtual DataTable GetSqlAsDataTable(string sql, string TableName = null, int TimeOut = 0)
        {
            if (LogSql) SqlHistoryLog.Add(sql);

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandTimeout = TimeOut;
                cmd.Connection = Connection;
                cmd.CommandText = sql;

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                if (TableName != null)
                {
                    dt.TableName = TableName;
                }

                return dt;

            }
        }

        /// <summary>
        /// Führt das übergebene SQL Statement aus und schreibt das Schema (KEINE Datensätze!) in ein neues DataTable
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual DataTable GetSqlAsDataTableSchema(string tableName, string sql)
        {
            DataTable dtResult = new DataTable();
            using (SqlConnection sqlConn = Connection)
            {
                using (SqlCommand command = sqlConn.CreateCommand())
                {
                    command.CommandText = String.Format(sql, tableName);
                    command.CommandTimeout = 0;
                    command.CommandType = CommandType.Text;

                    SqlDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

                    dtResult.Load(reader);
                }
            }

            return dtResult;
        }

        /// <summary>
        /// Führt einen ExecuteScalar aus und gibt den Wert als String zurück
        /// </summary>
        /// <param name="sql">SQL Statement</param>
        /// <returns>Ergebnis als String</returns>
        public string GetSqlAsString(string sql)
        {
            if (LogSql) SqlHistoryLog.Add(sql);

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandTimeout = 0;
                cmd.Connection = Connection;
                cmd.CommandText = sql;
                object result = cmd.ExecuteScalar();
                return (result == null ? null : result.ToString());
            }
        }

        /// <summary>
        /// Führt einen ExecuteScalar aus und gibt den Wert als Integer zurück
        /// </summary>
        /// <param name="sql">SQL Statement</param>
        /// <returns>Ergebnis als Integer</returns>
        public int GetSqlAsInt(string sql)
        {
            if (LogSql) SqlHistoryLog.Add(sql);

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandTimeout = 0;
                cmd.Connection = Connection;
                cmd.CommandText = sql;
                object tmp = cmd.ExecuteScalar();
                if (tmp == null)
                    return 0;
                return tmp.GetType().Name == "DBNull" ? 0 : tmp.GetType().Name == "Int64" ? (int)((Int64)tmp) : (int)tmp;
            }
        }

        protected void NormalizeStringLengthInDataTable(DataTable tab)
        {
            try
            {
                string Database = "";
                if (tab.TableName.Split('.').Length == 3)
                {
                    Database = tab.TableName.Split('.')[0] + ".";
                }
                string sql = "select Column_Name, CHARACTER_MAXIMUM_LENGTH from " + Database + "information_schema.columns WHERE ('[' + Table_Catalog + '].' + Table_Schema + '.' + Table_Name) = '" + tab.TableName + "' AND CHARACTER_MAXIMUM_LENGTH > 0 AND DATA_TYPE LIKE 'nvarchar'";
                DataTable definition = GetSqlAsDataTable(sql, tab.TableName);
                for (int i = 0; i < tab.Rows.Count; i++)
                {
                    foreach (DataRow row in definition.Rows)
                    {
                        if (tab.Rows[i][row["Column_Name"].ToString()] as string != null && (tab.Rows[i][row["Column_Name"].ToString()].ToString().Length > ((int)row["CHARACTER_MAXIMUM_LENGTH"])))
                        {
                            tab.Rows[i][row["Column_Name"].ToString()] = tab.Rows[i][row["Column_Name"].ToString()].ToString().Substring(0, ((int)row["CHARACTER_MAXIMUM_LENGTH"]));
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public int insertTableBulk(DataTable tab, bool AutoTruncate = true, Action<string> logFunction = null)
        {
            ///keine DataTable übergeben -> keine weitere Verarbeitung nötig.
            if (tab == null)
            {
                return 0;
            }

            try
            {
                if (AutoTruncate)
                {
                    NormalizeStringLengthInDataTable(tab);
                }
                SqlBulkCopy bulkCopy = new SqlBulkCopy(
                                                        Connection,
                                                        SqlBulkCopyOptions.TableLock |
                                                        SqlBulkCopyOptions.FireTriggers |
                                                        SqlBulkCopyOptions.UseInternalTransaction,
                                                        null
                                                        );

                bulkCopy.DestinationTableName = tab.TableName;
                bulkCopy.BulkCopyTimeout = 0;
                bulkCopy.WriteToServer(tab);

                return tab.Rows.Count;
            }
            catch (Exception ex)
            {
                if (logFunction != null)
                    logFunction(tab.TableName + ": Abbruch Bulkimport -> Zeilenweise Übernahme gestartet (" + ex.Message + ")");
                //logger("Abbruch Bulkimport -> Zeilenweise Übernahme gestartet");
                return insertTableLineByLine(tab, logFunction);
            }
        }

        public int insertTableLineByLine(DataTable tab, Action<string> logFunction = null)
        {
            int count = 0;

            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM " + tab.TableName, Connection);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                // Create a dataset object
                DataSet ds = new DataSet(tab.TableName + "Set");
                adapter.Fill(ds, tab.TableName);

                // Create a data table object and add a new row
                DataTable tmpTable = ds.Tables[tab.TableName];

                foreach (DataRow row in tab.Rows)
                {
                    bool isAdded = false;
                    try
                    {
                        DataRow r = tmpTable.NewRow();
                        foreach (DataColumn col in tab.Columns)
                        {
                            if (tmpTable.Columns.Contains(col.ColumnName))
                            {
                                r[col.ColumnName] = row[col.ColumnName];
                            }
                        }
                        tmpTable.Rows.Add(r);
                        isAdded = true;
                        // Update data adapter
                        adapter.Update(ds, tab.TableName);
                        count++;
                        //refresher("Tabelle " + tab.TableName + " (" + count + " von " + tab.Rows.Count + " Sätzen übernommen)");
                    }
                    catch (Exception ex)
                    {
                        if (isAdded)
                        {
                            tmpTable.Rows.Remove(tmpTable.Rows[tmpTable.Rows.Count - 1]);
                        }
                        if (logFunction != null)
                            logFunction(tab.TableName + ": " + ex.Message);
                        //logger(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                if (logFunction != null)
                    logFunction(tab.TableName + ": " + ex.Message);
                //logger(ex.ToString());
            }

            return count;
        }

        public int insertSingleRow(DataRow row, Action<string> logFunction = null)
        {
            int count = 0;

            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM " + row.Table.TableName, Connection);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                // Create a dataset object
                DataSet ds = new DataSet(row.Table.TableName + "Set");
                adapter.Fill(ds, row.Table.TableName);

                // Create a data table object and add a new row
                DataTable tmpTable = ds.Tables[row.Table.TableName];


                bool isAdded = false;
                try
                {
                    DataRow r = tmpTable.NewRow();
                    foreach (DataColumn col in row.Table.Columns)
                    {
                        if (tmpTable.Columns.Contains(col.ColumnName))
                        {
                            r[col.ColumnName] = row[col.ColumnName];
                        }
                    }
                    tmpTable.Rows.Add(r);
                    isAdded = true;
                    // Update data adapter
                    adapter.Update(ds, row.Table.TableName);
                    count++;
                    //refresher("Tabelle " + tab.TableName + " (" + count + " von " + tab.Rows.Count + " Sätzen übernommen)");
                }
                catch (Exception ex)
                {
                    if (isAdded)
                    {
                        tmpTable.Rows.Remove(tmpTable.Rows[tmpTable.Rows.Count - 1]);
                    }
                    if (logFunction != null)
                        logFunction(row.Table.TableName + ": " + ex.Message);
                    //logger(ex.Message);
                }

            }
            catch (Exception ex)
            {
                if (logFunction != null)
                    logFunction(row.Table.TableName + ": " + ex.Message);
                //logger(ex.ToString());
            }

            return count;
        }


        public int ExecuteSql(string sql)
        {
            if (LogSql) SqlHistoryLog.Add(sql);

            int result = 0;

            //wir möchten SQL fehler SEHEN!
            /*try 
            {*/

            //bei Aufruf von PhDB.executeSQL(string sql) ist die Connection nicht immer geöffnet /RK
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            using (SqlCommand cmd = new SqlCommand(sql, Connection))
            {
                cmd.CommandTimeout = 0;
                result = cmd.ExecuteNonQuery();
            }
            /*}
            catch
            {
                return -2;
            }*/

            return result;
        }

        /// <summary>
        /// exucute list of sql queries, and rollback if exception occurs
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool ExecuteTransactionSql(List<string> sql)
        {
            bool returnValue = false;

            SqlTransaction trans = null;
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }

            string currentSQL = string.Empty;
            try
            {
                trans = Connection.BeginTransaction();
                foreach (string sqlstring in sql)
                {
                    currentSQL = sqlstring;
                    using (SqlCommand cmd = new SqlCommand(sqlstring, Connection, trans))
                    {
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                    }
                }
                trans.Commit();
                returnValue = true;
            }
            catch (Exception ex)
            {
                if (trans != null) trans.Rollback();
                returnValue = false;
            }

            return returnValue;
        }

        /// <summary>
        /// sql should be with output parameter to return data
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string ExecuteSQLwithOutput(string sql)
        {
            try
            {
                //bei Aufruf von PhDB.executeSQL(string sql) ist die Connection nicht immer geöffnet /RK
                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(sql, Connection))
                {
                    var result = cmd.ExecuteScalar();
                    return result.ToString();
                }
            }
            catch (Exception ex)
            {
                //fehlerbehaftetes Statement in der Exception hinterlegen
                ex.Data.Add("SQL", sql);
                throw;
            }
        }
        
        public virtual DataTable GetTable(string sTable)
        {
            return GetSqlAsDataTable("SELECT * FROM " + sTable, sTable);
        }

        /// <summary>
        /// Führt einen ExecuteScalar aus und gibt den Wert als Integer zurück
        /// </summary>
        /// <param name="sql">SQL Statement</param>
        /// <returns>Ergebnis als Integer</returns>
        public object GetSqlAsObject(string sql)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandTimeout = 0;
                cmd.Connection = Connection;
                cmd.CommandText = sql;
                object tmp = cmd.ExecuteScalar();

                return tmp;
            }
        }
    }


}
