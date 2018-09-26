﻿using System;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataLoadEngine.Attachers;
using DataLoadEngine.DataFlowPipeline.Destinations;
using DataLoadEngine.DataFlowPipeline.Sources;
using DataLoadEngine.Job;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using DataTable = System.Data.DataTable;

namespace LoadModules.Generic.Attachers
{
    /// <summary>
    /// Data load component for loading RAW tables with records read from a remote database server. 
    /// Fetches all table from the specified database to load all catalogues specified.
    /// </summary>
    public class RemoteDatabaseAttacher: Attacher, IPluginAttacher
    {
        public RemoteDatabaseAttacher(): base(true)
        {
            
        }

        [DemandsInitialization("The DataSource to connect to in order to read data.", Mandatory=true)]
        public ExternalDatabaseServer RemoteSource { get; set; }

        [DemandsInitialization("The length of time in seconds to allow for data to be completely read from the destination before giving up (0 for no timeout)")]
        public int Timeout { get; set; }

        public override void Check(ICheckNotifier notifier)
        {
            if (!RemoteSource.Discover(DataAccessContext.DataLoad).Exists())
                throw new Exception("Database " + RemoteSource.Database + " did not exist on the remote server");
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
            
        }

        public override ExitCodeType Attach(IDataLoadJob job)
        {
            base.Attach(job);

            if (job == null)
                throw new Exception("Job is Null, we require to know the job to build a DataFlowPipeline");
      
            string sql;

            var dbFrom = RemoteSource.Discover(DataAccessContext.DataLoad);

            var remoteTables = dbFrom.DiscoverTables(true).Select(t => t.GetRuntimeName()).ToArray();
            var loadables = job.RegularTablesToLoad.Union(job.LookupTablesToLoad).Select(t => t.GetRuntimeName()).ToArray();

            foreach (var table in loadables)
            {
                if (!remoteTables.Contains(table))
                    throw new Exception("Loadable table " + table + " was NOT found on the remote DB!");

                sql = "Select * from " + table;

                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to execute SQL:" + Environment.NewLine + sql));

                var source = new DbDataCommandDataFlowSource(sql, "Fetch data from " + dbFrom + " to populate RAW table " + table, dbFrom.Server.Builder, Timeout == 0 ? 50000 : Timeout);

                var destination = new SqlBulkInsertDestination(_dbInfo, table, Enumerable.Empty<string>());
                
                var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
                var context = contextFactory.Create(PipelineUsage.LogsToTableLoadInfo | PipelineUsage.FixedDestination);

                var engine = new DataFlowPipelineEngine<DataTable>(context, source, destination, job);

                ITableLoadInfo loadInfo = job.DataLoadInfo.CreateTableLoadInfo("Truncate RAW table " + table,
                    _dbInfo.Server.Name + "." + _dbInfo.GetRuntimeName(),
                    new[]
                    {
                        new DataSource(
                            "Remote SqlServer Servername=" + dbFrom.Server + ";Database=" + _dbInfo.GetRuntimeName() +

                            //Either list the table or the query depending on what is populated
                            (table != null ? " Table=" + table : " Query = " + sql), DateTime.Now)
                    }, -1);

                engine.Initialize(loadInfo);
                engine.ExecutePipeline(new GracefulCancellationToken());

                if (source.TotalRowsRead == 0)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No rows were read from the remote table " + table + "."));
                }

                job.OnNotify(this, new NotifyEventArgs(source.TotalRowsRead > 0 ? ProgressEventType.Information : ProgressEventType.Warning, "Finished after reading " + source.TotalRowsRead + " rows"));
            }

            return ExitCodeType.Success;
        }
    }
}