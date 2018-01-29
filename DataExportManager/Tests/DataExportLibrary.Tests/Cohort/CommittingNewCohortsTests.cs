﻿using System;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.CohortCreationPipeline.Destinations;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using DataLoadEngine.DataFlowPipeline.Destinations;
using LoadModules.Generic.DataFlowSources;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.Tests.Cohort
{
    public class CommittingNewCohortsTests : TestsRequiringACohort
    {
        private string filename = "CommittingNewCohorts.csv";
        private string projName = "MyProj";

        [SetUp]
        public void GenerateFileToLoad()
        {
            StreamWriter sw = new StreamWriter(filename);    
            sw.WriteLine("PrivateID,ReleaseID,SomeHeader");
            sw.WriteLine("Priv_1111,Pub_1111,Smile buddy");
            sw.WriteLine("Priv_2222,Pub_2222,Your on tv");
            sw.WriteLine("Priv_3333,Pub_3333,Smile buddy");
            sw.Close();
        }

        [TearDown]
        public void CleanupProjects()
        {
            foreach (var c in DataExportRepository.GetAllObjects<ExtractableCohort>().Where(c => c.GetExternalData().ExternalDescription.Equals("CommittingNewCohorts")))
                c.DeleteInDatabase();

            foreach (Project p in DataExportRepository.GetAllObjects<Project>().Where(p => p.Name.Equals(projName)))
                p.DeleteInDatabase();
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Expected the cohort definition CommittingNewCohorts(Version 1, ID=511) to have a null ID - we are trying to create this, why would it already exist?")]
        public void CommittingNewCohortFile_IDPopulated_Throws()
        {
            var proj = new Project(DataExportRepository, projName);

            CohortCreationRequest request = new CohortCreationRequest(proj, new CohortDefinition(511, "CommittingNewCohorts",1,999,_externalCohortTable), (DataExportRepository)DataExportRepository, "fish");
            request.Check(new ThrowImmediatelyCheckNotifier());
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Project MyProj does not have a ProjectNumber specified, it should have the same number as the CohortCreationRequest (999)")]
        public void CommittingNewCohortFile_ProjectNumberNumberMissing()
        {
            var proj = new Project(DataExportRepository, projName);

            CohortCreationRequest request = new CohortCreationRequest(proj, new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable), (DataExportRepository)DataExportRepository, "fish");
            request.Check(new ThrowImmediatelyCheckNotifier());
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Project MyProj has ProjectNumber=321 but the CohortCreationRequest.ProjectNumber is 999")]
        public void CommittingNewCohortFile_ProjectNumberMismatch()
        {
            var proj = new Project(DataExportRepository, projName) {ProjectNumber = 321};
            proj.SaveToDatabase();

            CohortCreationRequest request = new CohortCreationRequest(proj, new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable), (DataExportRepository)DataExportRepository, "fish");
            request.Check(new ThrowImmediatelyCheckNotifier());
        }

        [Test]
        public void BasicCohortDestination_NullReleaseIdentifiersTest()
        {
            var dest = new BasicCohortDestination();

            var dt = new DataTable();
            dt.TableName = "beforeName";
            dt.Columns.Add("priv");
            dt.Columns.Add("release");

            dt.Rows.Add("123", 23);

            var result1 = dest.GetDistinctNotNullTable(dt);
            Assert.AreEqual(dt.TableName,result1.TableName);
            Assert.AreEqual(1,result1.Rows.Count);

            dt.Rows.Add("123", 23);
            dt.Rows.Add("123", 23);

            Assert.AreEqual(1,dest.GetDistinctNotNullTable(dt).Rows.Count);


            dt.Rows.Add("123", null);
            var ex = Assert.Throws<Exception>(()=>dest.GetDistinctNotNullTable(dt));
            Assert.AreEqual("Cohort DataTable contained null row:123,<null>",ex.Message);

            dt.Rows.Clear();
            dt.Rows.Add("123", 23);
            dt.Rows.Add(null, DBNull.Value); //row is fully null so gets ignored

            Assert.AreEqual(1, dest.GetDistinctNotNullTable(dt).Rows.Count);
        }

        [Test]
        public void CommittingNewCohortFile_CallPipeline()
        {
            var listener = new ThrowImmediatelyDataLoadEventListener();

            var proj = new Project(DataExportRepository, projName);
            proj.ProjectNumber = 999;
            proj.SaveToDatabase();

            CohortCreationRequest request = new CohortCreationRequest(proj, new CohortDefinition(null, "CommittingNewCohorts", 1, 999, _externalCohortTable), (DataExportRepository)DataExportRepository, "fish");
            request.Check(new ThrowImmediatelyCheckNotifier());


            DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
            BasicCohortDestination destination = new BasicCohortDestination();
            
            source.Separator = ",";
            source.StronglyTypeInput = true;
            
            DataFlowPipelineEngine<DataTable> pipeline = new DataFlowPipelineEngine<DataTable>((DataFlowPipelineContext<DataTable>) request.GetContext(),source,destination,listener);
            pipeline.Initialize(new FlatFileToLoad(new FileInfo(filename)),request);
            pipeline.ExecutePipeline(new GracefulCancellationToken());

            //there should be a new ExtractableCohort now
            Assert.NotNull(request.NewCohortDefinition.ID);

            var ec = DataExportRepository.GetAllObjects<ExtractableCohort>().Single(c => c.OriginID == request.NewCohortDefinition.ID);

            //with the data in it from the test file
            Assert.AreEqual(ec.Count,3);

        }
    }
}
