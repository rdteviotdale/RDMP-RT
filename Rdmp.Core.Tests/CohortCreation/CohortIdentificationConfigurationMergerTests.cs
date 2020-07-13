﻿using NUnit.Framework;
using Rdmp.Core.Curation.Data.Cohort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCreation
{
    class CohortIdentificationConfigurationMergerTests : CohortIdentificationTests
    {
        [Test]
        public void TestSimpleMerge()
        {
            var merger = new CohortIdentificationConfigurationMerger(CatalogueRepository);

            var cic1 = new CohortIdentificationConfiguration(CatalogueRepository,"cic1");
            var cic2 = new CohortIdentificationConfiguration(CatalogueRepository,"cic2");

            cic1.CreateRootContainerIfNotExists();
            var root1 = cic1.RootCohortAggregateContainer;
            root1.Name = "Root1";
            root1.SaveToDatabase();
            root1.AddChild(aggregate1,1);

            cic2.CreateRootContainerIfNotExists();
            var root2 = cic2.RootCohortAggregateContainer;
            root2.Name = "Root2";
            root2.SaveToDatabase();
            root2.AddChild(aggregate2,2);

            Assert.AreEqual(1,cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
            Assert.AreEqual(1,cic2.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
            
            int numberOfCicsBefore = CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Count();

            var result = merger.Merge(new []{cic1,cic2 },SetOperation.UNION);

            //original should still be in tact
            Assert.AreEqual(1,cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
            Assert.AreEqual(1,cic2.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);

            //the new merged set should contain both
            Assert.AreEqual(2,result.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);

            Assert.IsFalse(result.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Any(c=>c.Equals(aggregate1)),"Expected the merge to include clone aggregates not the originals! (aggregate1)");
            Assert.IsFalse(result.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Any(c=>c.Equals(aggregate2)),"Expected the merge to include clone aggregates not the originals! (aggregate2)");

            // Now should be a new one
            Assert.AreEqual(numberOfCicsBefore + 1,CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Count());

            var newCicId = result.ID;

            // Should have the root containers of the old configs
            Assert.AreEqual("Root2",result.RootCohortAggregateContainer.GetSubContainers()[0].Name);
            Assert.AreEqual("Root1",result.RootCohortAggregateContainer.GetSubContainers()[1].Name);

            // And should have
            Assert.AreEqual($"cic_{newCicId}_UnitTestAggregate2",result.RootCohortAggregateContainer.GetSubContainers()[0].GetAggregateConfigurations()[0].Name);
            Assert.AreEqual($"cic_{newCicId}_UnitTestAggregate1",result.RootCohortAggregateContainer.GetSubContainers()[1].GetAggregateConfigurations()[0].Name);

            Assert.AreEqual($"Merged cics (IDs {cic1.ID},{cic2.ID})",result.Name);

            Assert.IsTrue(cic1.Exists());
            Assert.IsTrue(cic2.Exists());

        }
    }
}
