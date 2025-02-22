// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Attachers
{
    /// <summary>
    /// Data load component for loading a detatched database file into RAW.  This attacher does not load RAW tables normally (like AnySeparatorFileAttacher etc)
    /// instead it specifies that it is itself going to act as RAW.  Using this component requires that the computer running the data load has file system access
    /// to the RAW Sql Server data directory (and that the path is the same).
    /// 
    /// <para>The mdf file will be copied to the Sql Server data directory of the RAW server and attached with the expected name of RAW.  From this point on the load
    /// will function normally.  It is up to the user to ensure that the table names/columns in the attached MDF match expected LIVE tables on your server (or 
    /// write AdjustRAW scripts to harmonise).</para>
    /// </summary>
    public class MDFAttacher : Attacher,IPluginAttacher
    {
        private const string GetDefaultSQLServerDatabaseDirectory = @"SELECT LEFT(physical_name,LEN(physical_name)-CHARINDEX('\',REVERSE(physical_name))+1) 
            FROM sys.master_files mf   
            INNER JOIN sys.[databases] d   
            ON mf.[database_id] = d.[database_id]   
            WHERE d.[name] = 'master' AND type = 0";

        [DemandsInitialization("Set this only if your RAW server is NOT localhost.  This is the network path to the DATA directory of your RAW database server you can find the DATA directory by running 'select * FROM sys.master_files'")]
        public string OverrideMDFFileCopyDestination{ get; set; }

        [DemandsInitialization(@"There are multiple ways to attach a mdf files to an SQL server, the first stage is always to copy the mdf and ldf files to the DATA directory of your server but after that it get's flexible.  
1. AttachWithConnectionString attempts to do the attaching as part of connection by specifying the AttachDBFilename keyword in the connection string
2. ExecuteCreateDatabaseForAttachSql attempts to connect to 'master' and execute CREATE DATABASE sql with the FILENAME property set to your mdf file in the DATA directory of your database server")]
        public MdfAttachStrategy AttachStrategy { get; set; }

        [DemandsInitialization("Set this only if you encounter problems with the ATTACH stage path.  This is the local path to the .mdf file in the DATA directory from the perspective of SQL Server")]
        public string OverrideAttachMdfPath { get; set; }

        [DemandsInitialization("Set this only if you encounter problems with the ATTACH stage path.  This is the local path to the .ldf file in the DATA directory from the perspective of SQL Server")]
        public string OverrideAttachLdfPath { get; set; }
                
        public MDFAttacher():base(false)
        {
             
        }
        
        MdfFileAttachLocations _locations;

        public override ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            //The location of .mdf files from the perspective of the database server
            var databaseDirectory = FindDefaultSQLServerDatabaseDirectory(new FromDataLoadEventListenerToCheckNotifier(job));
            _locations = new MdfFileAttachLocations(LoadDirectory.ForLoading, databaseDirectory, OverrideMDFFileCopyDestination);

            if (!string.IsNullOrWhiteSpace(OverrideAttachMdfPath))
                _locations.AttachMdfPath = OverrideAttachMdfPath;

            if (!string.IsNullOrWhiteSpace(OverrideAttachLdfPath))
                _locations.AttachLdfPath = OverrideAttachLdfPath;

            job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Identified the MDF file:" +_locations.OriginLocationMdf + " and corresponding LDF file:" + _locations.OriginLocationLdf));

            AsyncCopyMDFFilesWithEvents(_locations.OriginLocationMdf,_locations.CopyToMdf,_locations.OriginLocationLdf,_locations.CopyToLdf,job);
         
            switch (AttachStrategy)
            {
                case MdfAttachStrategy.AttachWithConnectionString:
                    return AttachWithConnectionString(job);
                case MdfAttachStrategy.ExecuteCreateDatabaseForAttachSql:
                    return ExecuteCreateDatabaseForAttachSql(job);
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private ExitCodeType ExecuteCreateDatabaseForAttachSql(IDataLoadEventListener listener)
        {
            // connect to master
            var builder = new SqlConnectionStringBuilder(_dbInfo.Server.Builder.ConnectionString)
            {
                InitialCatalog = "master",
                ConnectTimeout = 600
            };

            using(SqlConnection con = new SqlConnection(builder.ConnectionString))
            {
                try
                {
                    if(_dbInfo.Exists())
                        throw new Exception("Database " + _dbInfo.GetRuntimeName()  + " already exists on server " + builder.DataSource);

                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,"About to connect to master on " + builder.DataSource));
                    con.Open();

                    var nameTheyWant = _dbInfo.GetRuntimeName();

                    SqlCommand cmd = new SqlCommand(string.Format(@"  CREATE DATABASE {0}   
   ON (FILENAME = '{1}'),   
   (FILENAME = '{2}')   
   FOR ATTACH;  ",nameTheyWant,_locations.AttachMdfPath,_locations.AttachLdfPath),con);
                    

                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to execute SQL" + cmd.CommandText));

                    cmd.ExecuteNonQuery();

                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "SQL completed successfully" + cmd.CommandText));

                    if(_dbInfo.Exists())
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Database " + _dbInfo.GetRuntimeName() + " now exists"));
                    else
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Database " + _dbInfo.GetRuntimeName() + " attach SQL worked but it is still showing up as not existing..."));
                        return ExitCodeType.Error;
                    }
                }
                catch (Exception e)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Could not attach file " + _locations.AttachMdfPath + " to database", e));
                    try
                    {
                        DeleteFilesIfExist();
                    }
                    catch (Exception exception)
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "File Deletion (in cleanup phase - post failure) did not succeed either ", exception));
                    }

                    return ExitCodeType.Error;
                }

                return ExitCodeType.Success;
                
            }
        }

        private ExitCodeType AttachWithConnectionString(IDataLoadEventListener listener)
        {
            // Attach database
            var builder = new SqlConnectionStringBuilder(_dbInfo.Server.Builder.ConnectionString)
            {

                AttachDBFilename = _locations.AttachMdfPath,
                InitialCatalog = _dbInfo.GetRuntimeName(),
                ConnectTimeout = 600
            };

            using (var attachConnection = new SqlConnection(builder.ConnectionString))
            {
                try
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to attach file " + _locations.AttachMdfPath + " as a database to server " + builder.DataSource));
                    attachConnection.Open();
                }
                catch (Exception e)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Could not attach file " + _locations.AttachMdfPath + " to database", e));
                    try
                    {
                        DeleteFilesIfExist();
                    }
                    catch (Exception exception)
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "File Deletion (in cleanup phase - post failure) did not succeed either ", exception));
                    }

                    return ExitCodeType.Error;
                }
            }

            

            return ExitCodeType.Success;

        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
         {
             //dont bother cleaning up if it bombed
             if (exitCode == ExitCodeType.Error)
                 return;
             
             //its Abort,Success or LoadNotRequired
            
             // Detach database
             try
             {
                 var dbToDropName = _dbInfo.GetRuntimeName();

                 if(!dbToDropName.EndsWith("_RAW"))
                     throw new Exception("We were in the cleanup phase and were about to drop the database that was created by MDFAttacher when we noticed its name didn't end with _RAW!, its name was:" + dbToDropName + " were we about to nuke your live database?");

                 _dbInfo.Drop();

                 DeleteFilesIfExist();
             }
             catch (Exception e)
             {
                 throw new Exception("Could not detach database '" + _dbInfo.GetRuntimeName() + "': " + e);
             }

         }

        private void DeleteFilesIfExist()
        {
            if (File.Exists(_locations.CopyToLdf))
                File.Delete(_locations.CopyToLdf);

            if (File.Exists(_locations.CopyToMdf))
                File.Delete(_locations.CopyToMdf);
        }

        private void AsyncCopyMDFFilesWithEvents(string MDFSource, string MDFDestination, string LDFSource, string LDFDestination,IDataLoadEventListener job)
        {
            if(File.Exists(MDFDestination))
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning,$"File {MDFDestination} already exists, an attempt will be made to overwrite it"));

            if (File.Exists(LDFDestination))
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, $"File {LDFDestination} already exists, an attempt will be made to overwrite it"));

            Stopwatch s = new Stopwatch();
            s.Start();
            
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Starting copy from " + MDFSource + " to " + MDFDestination));
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Starting copy from " + LDFSource + " to " + LDFDestination));

            CopyWithProgress copyMDF = new CopyWithProgress();
            copyMDF.Progress +=
                (size, transferred, streamSize, bytesTransferred, number, reason, file, destinationFile, data) =>
                {
                    job.OnProgress(this,new ProgressEventArgs(MDFDestination, new ProgressMeasurement((int)(transferred * 0.001),ProgressType.Kilobytes),s.Elapsed));
                    return CopyWithProgress.CopyProgressResult.PROGRESS_CONTINUE;
                };
            copyMDF.XCopy(MDFSource,MDFDestination);
            s.Reset();
            s.Start();
        
            CopyWithProgress copyLDF = new CopyWithProgress();
            copyLDF.Progress +=
                (size, transferred, streamSize, bytesTransferred, number, reason, file, destinationFile, data) =>
                {
                    job.OnProgress(this,new ProgressEventArgs(LDFDestination,new ProgressMeasurement((int)(transferred * 0.001),ProgressType.Kilobytes),s.Elapsed));
                    return CopyWithProgress.CopyProgressResult.PROGRESS_CONTINUE;
                };
            copyLDF.XCopy(LDFSource,LDFDestination);
            s.Stop();
        }

        public string FindDefaultSQLServerDatabaseDirectory(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("About to look up Sql Server DATA directory Path",CheckResult.Success));

            try
            {
                //connect to master to run the data directory discovery SQL
                var builder = new SqlConnectionStringBuilder(_dbInfo.Server.Builder.ConnectionString);
                builder.InitialCatalog = "master";

                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    notifier.OnCheckPerformed(new CheckEventArgs("About to run:\r\n" + GetDefaultSQLServerDatabaseDirectory,CheckResult.Success));

                    string result = new SqlCommand(GetDefaultSQLServerDatabaseDirectory, connection).ExecuteScalar() as string;

                    if(string.IsNullOrWhiteSpace(result))
                        throw new Exception("Looking up DATA directory on server returned null (user may not have permissions to read from relevant sys tables)");

                    return result;
                }
            }
            catch (SqlException e)
            {
                throw new Exception("Could not execute the command: " + GetDefaultSQLServerDatabaseDirectory, e);
            }
        }

        public override void Check(ICheckNotifier notifier)
        {
            string localSqlServerDataDirectory;

            if (!string.IsNullOrWhiteSpace(OverrideMDFFileCopyDestination))
                localSqlServerDataDirectory = OverrideMDFFileCopyDestination;
            else
                localSqlServerDataDirectory = FindDefaultSQLServerDatabaseDirectory(notifier);
            
            var mdfFilename = _dbInfo.GetRuntimeName() + ".mdf";
            var ldfFilename = _dbInfo.GetRuntimeName() + "_log.ldf";


            if (Directory.Exists(localSqlServerDataDirectory))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Found server DATA folder (that we will copy mdf/ldf files to at path:" +
                        localSqlServerDataDirectory, CheckResult.Success));
            else
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Proposed server DATA folder (that we will copy mdf/ldf files to) was not found, proposed path was:" + localSqlServerDataDirectory, CheckResult.Fail));

            if (File.Exists(Path.Combine(localSqlServerDataDirectory, mdfFilename)))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "The database file '" + mdfFilename + "' exists in the local SQL server data directory '" +
                    localSqlServerDataDirectory + "'. A database called '" + _dbInfo.GetRuntimeName() +
                    "' may already be attached, which will cause the load process to fail. Delete this file to continue.",
                    CheckResult.Fail, null, "Delete file"));
            
            if (File.Exists(Path.Combine(localSqlServerDataDirectory, ldfFilename)))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "The database log file '" + ldfFilename + "' exists in the local SQL server data directory '" +
                    localSqlServerDataDirectory + "'. A database called '" + _dbInfo.GetRuntimeName() +
                    "'may already be attached, which will cause the load process to fail. Delete this file to continue.",
                    CheckResult.Fail, null, "Delete file"));
        }
    }
}
