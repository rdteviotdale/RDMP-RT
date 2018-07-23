﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Triggers;
using DataLoadEngine.Checks;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using HIC.Logging;
using LoadModules.Generic.Attachers;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataLoadEngineTests.Integration.CrossDatabaseTypeTests
{

    /*
     Test currently requires for LowPrivilegeLoaderAccount (e.g. minion)
     ---------------------------------------------------
     
        create database DLE_STAGING

        use DLE_STAGING

        CREATE USER [minion] FOR LOGIN [minion]

        ALTER ROLE [db_datareader] ADD MEMBER [minion]
        ALTER ROLE [db_ddladmin] ADD MEMBER [minion]
        ALTER ROLE [db_datawriter] ADD MEMBER [minion]
    */

    public class CrossDatabaseDataLoadTests : DatabaseTests
    {
        public enum TestCase
        {
            Normal,
            LowPrivilegeLoaderAccount,
            ForeignKeyOrphans,
            DodgyCollation,
            AllPrimaryKeys,
            WithNonPrimaryKeyIdentityColumn
        }

        [TestCase(DatabaseType.Oracle,TestCase.Normal)]
        [TestCase(DatabaseType.MicrosoftSQLServer,TestCase.Normal)]
        [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.WithNonPrimaryKeyIdentityColumn)]
        [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.DodgyCollation)]
        [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.LowPrivilegeLoaderAccount)]
        [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.AllPrimaryKeys)]
        [TestCase(DatabaseType.MYSQLServer,TestCase.Normal)]
        //[TestCase(DatabaseType.MYSQLServer, TestCase.WithNonPrimaryKeyIdentityColumn)] //Not supported by MySql:Incorrect table definition; there can be only one auto column and it must be defined as a key
        [TestCase(DatabaseType.MYSQLServer, TestCase.DodgyCollation)]
        [TestCase(DatabaseType.MYSQLServer, TestCase.LowPrivilegeLoaderAccount)]
        [TestCase(DatabaseType.MYSQLServer, TestCase.AllPrimaryKeys)]
        public void Load(DatabaseType databaseType, TestCase testCase)
        {
            var defaults = new ServerDefaults(CatalogueRepository);
            var logServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);
            var logManager = new LogManager(logServer);
            
            var db = GetCleanedServer(databaseType);

            var raw = db.Server.ExpectDatabase(db.GetRuntimeName() + "_RAW");
            if(raw.Exists())
                raw.ForceDrop();
            
            var dt = new DataTable("MyTable");
            dt.Columns.Add("Name");
            dt.Columns.Add("DateOfBirth");
            dt.Columns.Add("FavouriteColour");
            dt.Rows.Add("Bob", "2001-01-01","Pink");
            dt.Rows.Add("Frank", "2001-01-01","Orange");

            var nameCol = new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof (string), 20), false){IsPrimaryKey = true};

            if (testCase == TestCase.DodgyCollation)
                if(databaseType == DatabaseType.MicrosoftSQLServer)
                    nameCol.Collation = "Latin1_General_CS_AS_KS_WS";
                else if (databaseType == DatabaseType.MYSQLServer)
                    nameCol.Collation = "latin1_german1_ci";


            DiscoveredTable tbl;
            if (testCase == TestCase.WithNonPrimaryKeyIdentityColumn)
            {
                tbl = db.CreateTable("MyTable",new []
                {
                    new DatabaseColumnRequest("ID",new DatabaseTypeRequest(typeof(int)),false){IsPrimaryKey = false,IsAutoIncrement = true}, 
                    nameCol, 
                    new DatabaseColumnRequest("DateOfBirth",new DatabaseTypeRequest(typeof(DateTime)),false){IsPrimaryKey = true}, 
                    new DatabaseColumnRequest("FavouriteColour",new DatabaseTypeRequest(typeof(string))), 
                });
                
                using (var blk = tbl.BeginBulkInsert())
                    blk.Upload(dt);

                Assert.AreEqual(1,tbl.DiscoverColumns().Count(c=>c.GetRuntimeName().Equals("ID",StringComparison.CurrentCultureIgnoreCase)),"Table created did not contain ID column");
                
            }
            else
            if (testCase == TestCase.AllPrimaryKeys)
            {
                dt.PrimaryKey = dt.Columns.Cast<DataColumn>().ToArray();
                tbl = db.CreateTable("MyTable",dt,new []{nameCol}); //upload the column as is 
                Assert.IsTrue(tbl.DiscoverColumns().All(c => c.IsPrimaryKey));
            }
            else
            {
                tbl = db.CreateTable("MyTable", dt, new[]
                {
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth",new DatabaseTypeRequest(typeof(DateTime)),false){IsPrimaryKey = true}
                });
            }

            Assert.AreEqual(2, tbl.GetRowCount());
            
            //define a new load configuration
            var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");

            TableInfo ti = Import(tbl, lmd,logManager);
            
            var projectDirectory = SetupLoadDirectory(lmd);

            CreateCSVProcessTask(lmd,ti,"*.csv");
            
            //create a text file to load where we update Frank's favourite colour (it's a pk field) and we insert a new record (MrMurder)
            File.WriteAllText(
                Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
@"Name,DateOfBirth,FavouriteColour
Frank,2001-01-01,Neon
MrMurder,2001-01-01,Yella");

            
            //the checks will probably need to be run as ddl admin because it involves creating _Archive table and trigger the first time

            //clean up RAW / STAGING etc and generally accept proposed cleanup operations
            var checker = new CheckEntireDataLoadProcess(lmd, new HICDatabaseConfiguration(lmd), new HICLoadConfigurationFlags(),CatalogueRepository.MEF);
            checker.Check(new AcceptAllCheckNotifier());

            //create a reader
            if (testCase == TestCase.LowPrivilegeLoaderAccount)
            {
                SetupLowPrivilegeUserRightsFor(ti, TestLowPrivilegePermissions.Reader|TestLowPrivilegePermissions.Writer);
                SetupLowPrivilegeUserRightsFor(db.Server.ExpectDatabase("DLE_STAGING"),TestLowPrivilegePermissions.All);
            }
            var loadFactory = new HICDataLoadFactory(
                lmd,
                new HICDatabaseConfiguration(lmd),
                new HICLoadConfigurationFlags(),
                CatalogueRepository,
                logManager
                );

            try
            {
                var exe = loadFactory.Create(new ThrowImmediatelyDataLoadEventListener());
            
                var exitCode = exe.Run(
                    new DataLoadJob("Go go go!", logManager, lmd, projectDirectory,new ThrowImmediatelyDataLoadEventListener()),
                    new GracefulCancellationToken());

                Assert.AreEqual(ExitCodeType.Success,exitCode);

                if(testCase == TestCase.AllPrimaryKeys)
                {
                    Assert.AreEqual(4, tbl.GetRowCount()); //Bob, Frank, Frank (with also pk Neon) & MrMurder
                    Assert.Pass();
                }

                //frank should be updated to like Neon instead of Orange
                Assert.AreEqual(3,tbl.GetRowCount());
                var result = tbl.GetDataTable();
                var frank = result.Rows.Cast<DataRow>().Single(r => (string) r["Name"] == "Frank");
                Assert.AreEqual("Neon",frank["FavouriteColour"]);
                AssertHasDataLoadRunId(frank);

                //MrMurder is a new person who likes Yella
                var mrmurder = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "MrMurder");
                Assert.AreEqual("Yella", mrmurder["FavouriteColour"]);
                Assert.AreEqual(new DateTime(2001,01,01), mrmurder["DateOfBirth"]);
                AssertHasDataLoadRunId(mrmurder);

                //bob should be untouched (same values as before and no dataloadrunID)
                var bob = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "Bob");
                Assert.AreEqual("Pink", bob["FavouriteColour"]);
                Assert.AreEqual(new DateTime(2001, 01, 01), bob["DateOfBirth"]);
                Assert.AreEqual(DBNull.Value,bob[SpecialFieldNames.DataLoadRunID]);

                //MySql add default of now() on a table will auto populate all the column values with the the now() date while Sql Server will leave them as nulls
                if(databaseType != DatabaseType.MYSQLServer)
                    Assert.AreEqual(DBNull.Value, bob[SpecialFieldNames.ValidFrom]);
            }
            finally
            {
                Directory.Delete(lmd.LocationOfFlatFiles, true);

                foreach (Catalogue c in RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>())
                    c.DeleteInDatabase();

                foreach (TableInfo t in RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>())
                    t.DeleteInDatabase();

                foreach (LoadMetadata l in RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                    l.DeleteInDatabase();
            }
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void DLELoadTwoTables(DatabaseType databaseType)
        {
            //setup the data tables
            var defaults = new ServerDefaults(CatalogueRepository);
            var logServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);
            var logManager = new LogManager(logServer);

            var db = GetCleanedServer(databaseType);

            var dtParent = new DataTable();
            dtParent.Columns.Add("ID");
            dtParent.Columns.Add("Name");
            dtParent.Columns.Add("Height");
            dtParent.PrimaryKey = new[] {dtParent.Columns[0]};

            dtParent.Rows.Add("1", "Dave", "3.5");
            
            var dtChild = new DataTable();
            dtChild.Columns.Add("Parent_ID");
            dtChild.Columns.Add("ChildNumber");
            dtChild.Columns.Add("Name");
            dtChild.Columns.Add("DateOfBirth");
            dtChild.Columns.Add("Age");
            dtChild.Columns.Add("Height");

            dtChild.Rows.Add("1","1","Child1","2001-01-01","20","3.5");
            dtChild.Rows.Add("1","2","Child2","2002-01-01","19","3.4");
            
            dtChild.PrimaryKey = new[] {dtChild.Columns[0], dtChild.Columns[1]};

            //create the parent table based on the DataTable
            var parentTbl = db.CreateTable("Parent",dtParent);

            //go find the primary key column created
            var pkParentID = parentTbl.DiscoverColumn("ID");

            //forward declare this column as part of pk (will be used to specify foreign key
            var fkParentID = new DatabaseColumnRequest("Parent_ID", "int"){IsPrimaryKey = true};

            var childTbl = db.CreateTable("Child", dtChild, new[]
            {
                fkParentID
            }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn>()
            {
                {fkParentID, pkParentID}
            }, true);

            Assert.AreEqual(1, parentTbl.GetRowCount());
            Assert.AreEqual(2, childTbl.GetRowCount());

            //create a new load
            var lmd = new LoadMetadata(CatalogueRepository, "MyLoading2");

            TableInfo parentTableInfo = Import(parentTbl,lmd,logManager);
            TableInfo childTableInfo = Import(childTbl, lmd,logManager);

            var projectDirectory = SetupLoadDirectory(lmd);

            CreateCSVProcessTask(lmd,parentTableInfo,"parent.csv");
            CreateCSVProcessTask(lmd, childTableInfo, "child.csv");

            //create a text file to load where we update Frank's favourite colour (it's a pk field) and we insert a new record (MrMurder)
            File.WriteAllText(
                Path.Combine(projectDirectory.ForLoading.FullName, "parent.csv"),
@"ID,Name,Height
2,Man2,3.1
1,Dave,3.2");

            File.WriteAllText(
                Path.Combine(projectDirectory.ForLoading.FullName, "child.csv"),
@"Parent_ID,ChildNumber,Name,DateOfBirth,Age,Height
1,1,UpdC1,2001-01-01,20,3.5
2,1,NewC1,2000-01-01,19,null");
            
            
            //clean up RAW / STAGING etc and generally accept proposed cleanup operations
            var checker = new CheckEntireDataLoadProcess(lmd, new HICDatabaseConfiguration(lmd), new HICLoadConfigurationFlags(), CatalogueRepository.MEF);
            checker.Check(new AcceptAllCheckNotifier());

            var loadFactory = new HICDataLoadFactory(
                lmd,
                new HICDatabaseConfiguration(lmd),
                new HICLoadConfigurationFlags(),
                CatalogueRepository,
                logManager
                );
            try
            {
                var exe = loadFactory.Create(new ThrowImmediatelyDataLoadEventListener());

                var exitCode = exe.Run(
                    new DataLoadJob("Go go go!", logManager, lmd, projectDirectory, new ThrowImmediatelyDataLoadEventListener()),
                    new GracefulCancellationToken());

                Assert.AreEqual(ExitCodeType.Success, exitCode);

                //should now be 2 parents (the original - who was updated) + 1 new one (Man2)
                Assert.AreEqual(2, parentTbl.GetRowCount());
                var result = parentTbl.GetDataTable();
                var dave = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "Dave");
                Assert.AreEqual(3.2f, dave["Height"]); //should now be only 3.2 inches high
                AssertHasDataLoadRunId(dave);

                //should be 3 children (Child1 who gets updated to be called UpdC1) and NewC1
                Assert.AreEqual(3, childTbl.GetRowCount());
                result = childTbl.GetDataTable();

                var updC1 = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "UpdC1");
                Assert.AreEqual(1, updC1["Parent_ID"]);
                Assert.AreEqual(1, updC1["ChildNumber"]);
                AssertHasDataLoadRunId(updC1);

                var newC1 = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "NewC1");
                Assert.AreEqual(2, newC1["Parent_ID"]);
                Assert.AreEqual(1, newC1["ChildNumber"]);
                Assert.AreEqual(DBNull.Value, newC1["Height"]); //the "null" in the input file should be DBNull.Value in the final database
                AssertHasDataLoadRunId(newC1);

            }
            finally
            {
                Directory.Delete(lmd.LocationOfFlatFiles,true);

                foreach (Catalogue c in RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>())
                    c.DeleteInDatabase();

                foreach (TableInfo t in RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>())
                    t.DeleteInDatabase();

                foreach (LoadMetadata l in RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                    l.DeleteInDatabase();
            }
        }

        private void AssertHasDataLoadRunId(DataRow row)
        {
            var o = row[SpecialFieldNames.DataLoadRunID];
            
            Assert.IsNotNull(o,"A row which was expected to have a hic_dataLoadRunID had null instead");
            Assert.AreNotEqual(DBNull.Value,o,"A row which was expected to have a hic_dataLoadRunID had DBNull.Value instead");
            Assert.GreaterOrEqual((int)o, 0);

            var d = row[SpecialFieldNames.ValidFrom];
            Assert.IsNotNull(d, "A row which was expected to have a hic_validFrom had null instead");
            Assert.AreNotEqual(DBNull.Value,d, "A row which was expected to have a hic_validFrom had DBNull.Value instead");
            
            //expect validFrom to be after 2 hours ago (to handle UTC / BST nonesense)
            Assert.GreaterOrEqual((DateTime)d, DateTime.Now.Subtract(new TimeSpan(2,0,0)));

        }

        private void CreateCSVProcessTask(LoadMetadata lmd, TableInfo ti, string regex)
        {
            var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.Mounting);
            pt.Path = typeof(AnySeparatorFileAttacher).FullName;
            pt.ProcessTaskType = ProcessTaskType.Attacher;
            pt.Name = "Load " + ti.GetRuntimeName();
            pt.SaveToDatabase();

            pt.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>();
            pt.SetArgumentValue("FilePattern", regex);
            pt.SetArgumentValue("Separator", ",");
            pt.SetArgumentValue("TableToLoad", ti);

            pt.Check(new ThrowImmediatelyCheckNotifier());
        }

        private HICProjectDirectory SetupLoadDirectory(LoadMetadata lmd)
        {
            var projectDirectory = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo(Environment.CurrentDirectory), "MyLoadDir", true);
            lmd.LocationOfFlatFiles = projectDirectory.RootPath.FullName;
            lmd.SaveToDatabase();

            return projectDirectory;
        }

        private TableInfo Import(DiscoveredTable tbl, LoadMetadata lmd, LogManager logManager)
        {
            logManager.CreateNewLoggingTaskIfNotExists(lmd.Name);

            //import TableInfos
            var importer = new TableInfoImporter(CatalogueRepository, tbl);
            TableInfo ti;
            ColumnInfo[] cis;
            importer.DoImport(out ti, out cis);

            //create Catalogue
            var forwardEngineer = new ForwardEngineerCatalogue(ti, cis, true);

            Catalogue cata;
            CatalogueItem[] cataItems;
            ExtractionInformation[] eis;
            forwardEngineer.ExecuteForwardEngineering(out cata, out cataItems, out eis);

            //make the catalogue use the load configuration
            cata.LoadMetadata_ID = lmd.ID;
            cata.LoggingDataTask = lmd.Name;
            Assert.IsNotNull(cata.LiveLoggingServer_ID); //catalogue should have one of these because of system defaults
            cata.SaveToDatabase();

            return ti;
        }
    }
}
