﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{

    public class MicrosoftSQLServerHelper : DiscoveredServerHelper
    {

        //the name of the properties on DbConnectionStringBuilder that correspond to server and database
        public MicrosoftSQLServerHelper() : base(DatabaseType.MicrosoftSQLServer)
        {
        }

        protected override string ServerKeyName { get { return "Data Source"; }}
        protected override string DatabaseKeyName { get { return "Initial Catalog"; }}

        #region Up Typing
        public override DbCommand GetCommand(string s, DbConnection con, DbTransaction transaction = null)
        {
            return new SqlCommand(s, (SqlConnection)con, transaction as SqlTransaction);
        }

        public override DbDataAdapter GetDataAdapter(DbCommand cmd)
        {
            return new SqlDataAdapter((SqlCommand) cmd);
        }

        public override DbCommandBuilder GetCommandBuilder(DbCommand cmd)
        {
            return new SqlCommandBuilder((SqlDataAdapter) GetDataAdapter(cmd));
        }

        public override DbParameter GetParameter(string parameterName)
        {
            return new SqlParameter(parameterName,null);
        }

        public override DbConnection GetConnection(DbConnectionStringBuilder builder)
        {
            return new SqlConnection(builder.ConnectionString);
        }

        public override DbConnectionStringBuilder GetConnectionStringBuilder(string connectionString)
        {
            return new SqlConnectionStringBuilder(connectionString);
        }

        public string GetDatabaseNameFrom(DbConnectionStringBuilder builder)
        {
            return ((SqlConnectionStringBuilder) builder).InitialCatalog;
        }
        #endregion

        /// <summary>
        /// Changes the database that the DbConnectionStringBuilder points at. This does not modify the original DbConnectionStringBuilder.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="newDatabase"></param>
        /// <returns></returns>
        public override DbConnectionStringBuilder ChangeDatabase(DbConnectionStringBuilder builder, string newDatabase)
        {
            return new SqlConnectionStringBuilder(builder.ConnectionString) {InitialCatalog = newDatabase};
        }

        public override string[] ListDatabases(DbConnectionStringBuilder builder)
        {
            //create a copy so as not to corrupt the original
            var b = new SqlConnectionStringBuilder(builder.ConnectionString);
            b.InitialCatalog = "master";

            using (var con = new SqlConnection(b.ConnectionString))
            {
                con.Open();
                return ListDatabases(con);
            }
        }

        public override string[] ListDatabases(DbConnection con)
        {
            var cmd = GetCommand("select name [Database] from master..sysdatabases", con);
            
            DbDataReader r = cmd.ExecuteReader();

            List<string> databases = new List<string>();

            while (r.Read())
                databases.Add((string) r["Database"]);

            con.Close();
            return databases.ToArray();
        }
       
        public override DbConnectionStringBuilder EnableAsync(DbConnectionStringBuilder builder)
        {
            var b = (SqlConnectionStringBuilder) builder;

            b.MultipleActiveResultSets = true;
            b.AsynchronousProcessing = true;

            return b;
        }

        public override IDiscoveredDatabaseHelper GetDatabaseHelper()
        {
            return new MicrosoftSQLDatabaseHelper();
        }

        public override IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return new MicrosoftQuerySyntaxHelper();
        }

        public override void CreateDatabase(DbConnectionStringBuilder builder, IHasRuntimeName newDatabaseName)
        {
            var b = new SqlConnectionStringBuilder(builder.ConnectionString);
            b.InitialCatalog = "master";

            using (var con = new SqlConnection(b.ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("CREATE DATABASE [" + newDatabaseName.GetRuntimeName() + "]", (SqlConnection)con);
                cmd.ExecuteNonQuery();                
            }
        }

        public override Dictionary<string,string> DescribeServer(DbConnectionStringBuilder builder)
        {
            Dictionary<string,string> toReturn = new Dictionary<string, string>();
          
            using (SqlConnection con = new SqlConnection(builder.ConnectionString))
            {
                con.Open();

                Server s = new Server(new ServerConnection(con));
                try
                {

                    toReturn.Add("Backup Locations",s.BackupDirectory);
                 
                    string backupDevices = string.Join(",",
                        s.BackupDevices.Cast<BackupDevice>()
                            .Select(b => b.PhysicalLocation + "(" + b.BackupDeviceType + ")"));

                    toReturn.Add("Backup Devices",backupDevices);
                }
                catch (Exception )
                {
                    toReturn.Add("Backups","Unknown");
                }

                try
                {
                    toReturn.Add("CPUs", s.AffinityInfo.Cpus.Count.ToString());
                }
                catch (Exception)
                {
                    toReturn.Add("CPUs", "Unknown");
                }
                try
                {
                    toReturn.Add("OS Version", s.OSVersion);
                }
                catch (Exception)
                {
                    toReturn.Add("OS Version","Unknown");
                }
                
                var properties = s.Information.GetType().GetProperties();

                foreach (var property in properties)
                {
                    if(property.Name.Equals("Properties"))
                        continue;

                    var value = property.GetValue(s.Information);
                    toReturn.Add(property.Name,value == null ? "" : value.ToString());
                }

                try
                {
                    DataTable dt = new DataTable();
                    new SqlDataAdapter(new SqlCommand("EXEC master..xp_fixeddrives",con)).Fill(dt);
                    foreach (DataRow row in dt.Rows)
                        toReturn.Add("Free Space Drive" + row[0], "" + row[1]);
                }
                catch (Exception)
                {
                    toReturn.Add("Free Space ", "Unknown");
                }
            }
            

            return toReturn;
        }

        public override bool RespondsWithinTime(DbConnectionStringBuilder builder, int timeoutInSeconds, out Exception exception)
        {
            try
            {
                var copyBuilder = new SqlConnectionStringBuilder(builder.ConnectionString);
                copyBuilder.ConnectTimeout = timeoutInSeconds;

                using (var con = GetConnection(copyBuilder))
                {
                    con.Open();

                    con.Close();

                    exception = null;
                    return true;
                }
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }

        public override DbConnectionStringBuilder GetConnectionStringBuilder(string server, string database, string username, string password)
        {
            var toReturn = new SqlConnectionStringBuilder() {DataSource = server, InitialCatalog = database};
            if (!string.IsNullOrWhiteSpace(username))
            {
                toReturn.UserID = username;
                toReturn.Password = password;
            }
            else
                toReturn.IntegratedSecurity = true;

            return toReturn;
        }
    }
}
