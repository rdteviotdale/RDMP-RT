using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Repositories;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.Annotations;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// How are files cached within the cache (e.g. within a zip? tar? just uncompressed in a directory).
    /// </summary>
    public enum CacheArchiveType
    {
        /// <summary>
        /// Cached files are in a directory uncompressed
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Cached files are contained in a zip file
        /// </summary>
        Zip = 1,

        /// <summary>
        /// Cached files are contained in a tar file
        /// </summary>
        Tar = 2
    }



    /// <summary>
    /// Entrypoint to the loading metadata for one or more Catalogues. This includes name, description, scheduled start dates etc.  All other loading
    /// entities are attached to this entity for example each load Process (Unzip files called *.zip / Dowload all files from FTP server X) contains
    /// a reference to the LoadMetadata that it belongs in.
    /// 
    /// <para>A LoadMetadata also allows you to override various settings such as forcing a specific alternate server to load - for when you want to overule
    /// the location that TableInfo thinks data is on e.g. into a test environment mirror of live.</para>
    /// </summary>
    public class LoadMetadata : VersionedDatabaseEntity, ILoadMetadata, IHasDependencies, IHasQuerySyntaxHelper
    {

        #region Database Properties
        private string _locationOfFlatFiles;
        private string _anonymisationEngineClass;
        private string _name;
        private string _description;
        private CacheArchiveType _cacheArchiveType;
        private int? _overrideRawServerID;

        /// <inheritdoc/>
        [AdjustableLocation]
        public string LocationOfFlatFiles
        {
            get { return _locationOfFlatFiles; }
            set { SetField(ref _locationOfFlatFiles, value); }
        }

        /// <summary>
        /// Not used
        /// </summary>
        public string AnonymisationEngineClass
        {
            get { return _anonymisationEngineClass; }
            set { SetField(ref _anonymisationEngineClass, value); }
        }

        /// <inheritdoc/>
        [Unique]
        [NotNull]
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        /// <summary>
        /// Human readable description of the load, what it does etc
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { SetField(ref _description, value); }
        }

        /// <summary>
        /// The format for storing files in when reading/writing to a cache with a <see cref="CatalogueLibrary.Data.Cache.CacheProgress"/>.  This may not be respected
        /// depending on the implementation of the sepecific ICacheLayout
        /// </summary>
        public CacheArchiveType CacheArchiveType
        {
            get { return _cacheArchiveType; }
            set { SetField(ref _cacheArchiveType, value); }
        }

        /// <summary>
        /// Optional.  Indicates that when running the Data Load Engine, the specific <see cref="ExternalDatabaseServer"/> should be used for the RAW server (instead of 
        /// the system default - see <see cref="ServerDefaults"/>).
        /// </summary>
        public int? OverrideRAWServer_ID
        {
            get { return _overrideRawServerID; }
            set { SetField(ref _overrideRawServerID, value); }
        }

        #endregion

        
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int LocationOfFlatFiles_MaxLength = -1;

        #region Relationships

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public ExternalDatabaseServer OverrideRAWServer {
            get { return OverrideRAWServer_ID.HasValue? Repository.GetObjectByID<ExternalDatabaseServer>(OverrideRAWServer_ID.Value): null; }
        }

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public ILoadProgress[] LoadProgresses
        {
            get { return Repository.GetAllObjectsWithParent<LoadProgress>(this); }
        }

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IOrderedEnumerable<IProcessTask> ProcessTasks
        {
            get
            {
                return
                    Repository.GetAllObjectsWithParent<ProcessTask>(this).Cast<IProcessTask>().OrderBy(pt => pt.Order);
            }
        }
        #endregion

        /// <summary>
        /// Create a new DLE load.  This load will not have any <see cref="ProcessTask"/> and will not load any <see cref="TableInfo"/> yet.
        /// 
        /// <para>To set the loaded tables, set <see cref="Catalogue.LoadMetadata_ID"/> on some of your datasets</para>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="name"></param>
        public LoadMetadata(ICatalogueRepository repository, string name = null)
        {
            if (name == null)
                name = "NewLoadMetadata" + Guid.NewGuid();
            repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"Name",name}
            });
        }

        internal LoadMetadata(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            LocationOfFlatFiles = r["LocationOfFlatFiles"].ToString();
            Name = r["Name"] as string;
            AnonymisationEngineClass = r["AnonymisationEngineClass"].ToString();
            Name = r["Name"].ToString();
            Description = r["Description"] as string;//allows for nulls
            CacheArchiveType = (CacheArchiveType)r["CacheArchiveType"];
            OverrideRAWServer_ID = ObjectToNullableInt(r["OverrideRAWServer_ID"]);
        }
        
        /// <inheritdoc/>
        public override void DeleteInDatabase()
        {
            ICatalogue firstOrDefault = GetAllCatalogues().FirstOrDefault();

            if (firstOrDefault != null)
                throw new Exception("This load is used by " + firstOrDefault.Name + " so cannot be deleted (Disassociate it first)");

            base.DeleteInDatabase();
        }
        
        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public IEnumerable<ICatalogue> GetAllCatalogues()
        {
            return Repository.GetAllObjectsWithParent<Catalogue>(this);
        }

        /// <inheritdoc/>
        public DiscoveredServer GetDistinctLoggingDatabase(out IExternalDatabaseServer serverChosen)
        {
            var loggingServers = GetLoggingServers();

            var loggingServer = loggingServers.FirstOrDefault();

            //get distinct connection
            var toReturn = DataAccessPortal.GetInstance().ExpectDistinctServer(loggingServers, DataAccessContext.Logging, true);

            serverChosen = (IExternalDatabaseServer)loggingServer;
            return toReturn;
        }

        /// <inheritdoc/>
        public DiscoveredServer GetDistinctLoggingDatabase()
        {
            IExternalDatabaseServer whoCares;
            return GetDistinctLoggingDatabase(out whoCares);
        }

        private IDataAccessPoint[] GetLoggingServers()
        {
            ICatalogue[] catalogue = GetAllCatalogues().ToArray();

            if (!catalogue.Any())
                throw new NotSupportedException("LoadMetaData '" + ToString() + " (ID=" + ID + ") does not have any Catalogues associated with it so it is not possible to fetch it's LoggingDatabaseSettings");

            return catalogue.Select(c => c.LiveLoggingServer).ToArray();
        }

        /// <inheritdoc/>
        public string GetDistinctLoggingTask()
        {
            var catalogueMetadatas = GetAllCatalogues().ToArray();

            if(!catalogueMetadatas.Any())
                throw new Exception("There are no Catalogues associated with load metadata (ID=" +this.ID+")");

            var cataloguesWithoutLoggingTasks = catalogueMetadatas.Where(c => String.IsNullOrWhiteSpace(c.LoggingDataTask)).ToArray();

            if(cataloguesWithoutLoggingTasks.Any())
                throw new Exception("The following Catalogues do not have a LoggingDataTask specified:" + cataloguesWithoutLoggingTasks.Aggregate("",(s,n)=>s + n.ToString() + "(ID="+n.ID+"),"));
            
            string[] distinctLoggingTasks = catalogueMetadatas.Select(c => c.LoggingDataTask).Distinct().ToArray();
            if(distinctLoggingTasks.Count()>= 2)
                throw new Exception("There are " + distinctLoggingTasks.Length + " logging tasks in Catalogues belonging to this metadata (ID=" +this.ID+")");

            return distinctLoggingTasks[0];
        }

        /// <summary>
        /// Return all <see cref="TableInfo"/> underlying the <see cref="Catalogue"/>(s) which use this load (what tables will be loaded by the DLE).
        /// </summary>
        /// <param name="includeLookups">true to include lookup tables (e.g. z_sex etc) configured in the <see cref="Catalogue"/>(s)</param>
        /// <returns></returns>
        public List<TableInfo> GetDistinctTableInfoList(bool includeLookups)
        {
            List<TableInfo> toReturn = new List<TableInfo>();

            foreach (ICatalogue catalogueMetadata in GetAllCatalogues())
                foreach (TableInfo tableInfo in catalogueMetadata.GetTableInfoList(includeLookups))
                    if (!toReturn.Contains(tableInfo))
                        toReturn.Add(tableInfo);

            return toReturn;
        }
        
        /// <summary>
        /// Do not use, just assume true
        /// </summary>
        /// <returns></returns>
        [Obsolete("Test logging databases are a bad idea on a live Catalogue repository")]
        public bool AreLiveAndTestLoggingDifferent()
        {
            Catalogue[] catalogues = GetAllCatalogues().Cast<Catalogue>().ToArray();

            if (catalogues.Length == 0)
                return true;

            int? liveID = catalogues.Select(c => c.LiveLoggingServer_ID).Distinct().Single();
            int? testID = catalogues.Select(c => c.TestLoggingServer_ID).Distinct().Single();

            //theres a live configured but no test so we should just use the live one
            if (liveID != null && testID == null)
                return false;
            
            return liveID != testID ;
        }

        /// <inheritdoc/>
        public DiscoveredServer GetDistinctLiveDatabaseServer()
        {
            HashSet<ITableInfo> normalTables = new HashSet<ITableInfo>();
            HashSet<ITableInfo> lookupTables = new HashSet<ITableInfo>();

            foreach (ICatalogue catalogue in this.GetAllCatalogues())
            {
                List<ITableInfo> normal;
                List<ITableInfo> lookup;
                catalogue.GetTableInfos(out normal, out lookup);

                foreach (ITableInfo n in normal)
                    normalTables.Add(n);
                foreach (ITableInfo l in lookup)
                    lookupTables.Add(l);
            }

            if (normalTables.Any())
                return DataAccessPortal.GetInstance().ExpectDistinctServer(normalTables.ToArray(), DataAccessContext.DataLoad,true);
            
            if(lookupTables.Any())
                return DataAccessPortal.GetInstance().ExpectDistinctServer(lookupTables.ToArray(), DataAccessContext.DataLoad,true);
            
            throw new Exception("LoadMetadata " + this + " has no TableInfos configured (or possibly the tables have been deleted resulting in MISSING ColumnInfos?)");
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return null;
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return GetAllCatalogues().ToArray();
        }

        /// <summary>
        /// Tests that the logging database for the load is reachable and that it has an appropriate logging task for the load (if not a new task will be created 'Loading X')
        /// </summary>
        /// <param name="catalogue"></param>
        public void EnsureLoggingWorksFor(ICatalogue catalogue)
        {
            //if theres no logging task / logging server set them up with the same name as the lmd
            IExternalDatabaseServer loggingServer;

            if (catalogue.LiveLoggingServer_ID == null)
            {
                loggingServer = new ServerDefaults((CatalogueRepository) Repository).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

                if (loggingServer != null)
                    catalogue.LiveLoggingServer_ID = loggingServer.ID;
                else
                    throw new NotSupportedException("You do not yet have any logging servers configured so cannot create data loads");
            }
            else
                loggingServer = Repository.GetObjectByID<ExternalDatabaseServer>(catalogue.LiveLoggingServer_ID.Value);

            //if theres no logging task yet and theres a logging server
            if (string.IsNullOrWhiteSpace(catalogue.LoggingDataTask))
            {
                var lm = new LogManager(loggingServer);
                var loggingTaskName = Name;

                lm.CreateNewLoggingTaskIfNotExists(loggingTaskName);
                catalogue.LoggingDataTask = loggingTaskName;
                catalogue.SaveToDatabase();
            }
        }

        /// <inheritdoc/>
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            var syntax = GetAllCatalogues().Select(c => c.GetQuerySyntaxHelper()).Distinct().ToArray();
            if (syntax.Length > 1)
                throw new Exception("LoadMetadata '" + this + "' has multiple underlying Catalogue Live Database Type(s) - not allowed");

            return syntax.SingleOrDefault();
        }
    }
}
