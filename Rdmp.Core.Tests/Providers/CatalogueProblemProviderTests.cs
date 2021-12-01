﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Providers;
using ReusableLibraryCode.Checks;
using Tests.Common;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.Tests.Providers
{
    class CatalogueProblemProviderTests : UnitTests
    {
        #region ROOT CONTAINERS
        [Test]
        public void TestRootOrderCohortContainer_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
            var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();
            
            container.Operation = SetOperation.UNION;
            container.AddChild(childAggregateConfiguration, 1);
            container.AddChild(childAggregateConfiguration2, 1);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNotNull(problem);
            Assert.AreEqual("Child order is ambiguous, show the Order column and reorder contents", problem);
        }

        [Test]
        public void TestEmptyRootUNIONCohortContainer_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();

            container.Operation = SetOperation.UNION;

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNotNull(problem);
            Assert.AreEqual("You must have at least one element in the root container", problem);
        }

        [Test]
        public void TestEmptyRootEXCEPTCohortContainer_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();

            container.Operation = SetOperation.EXCEPT;

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNotNull(problem);
            Assert.AreEqual("EXCEPT and INTERSECT container operations must have at least two elements within", problem);
        }

        [Test]
        public void TestEmptyRootINTERSECTCohortContainer_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();

            container.Operation = SetOperation.INTERSECT;

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNotNull(problem);
            Assert.AreEqual("EXCEPT and INTERSECT container operations must have at least two elements within", problem);
        }

        [Test]
        public void Test1ChildRootUNIONCohortContainer_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

            container.Operation = SetOperation.UNION;
            container.AddChild(childAggregateConfiguration, 0);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNull(problem);
        }

        [Test]
        public void Test2ChildRootUNIONCohortContainer_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
            var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

            container.Operation = SetOperation.UNION;
            container.AddChild(childAggregateConfiguration, 1);
            container.AddChild(childAggregateConfiguration2, 2);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNull(problem);
        }

        [Test]
        public void Test1ChildRootEXCEPTCohortContainer_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

            container.Operation = SetOperation.EXCEPT;
            container.AddChild(childAggregateConfiguration, 0);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNotNull(problem);
            Assert.AreEqual("EXCEPT and INTERSECT container operations must have at least two elements within", problem);
        }

        [Test]
        public void Test2ChildRootEXCEPTCohortContainer_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
            var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

            container.Operation = SetOperation.EXCEPT;
            container.AddChild(childAggregateConfiguration, 1);
            container.AddChild(childAggregateConfiguration2, 2);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNull(problem);
        }

        [Test]
        public void Test1ChildRootINTERSECTCohortContainer_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

            container.Operation = SetOperation.EXCEPT;
            container.AddChild(childAggregateConfiguration, 0);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNotNull(problem);
            Assert.AreEqual("EXCEPT and INTERSECT container operations must have at least two elements within", problem);
        }

        [Test]
        public void Test2ChildRootINTERSECTCohortContainer_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
            var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

            container.Operation = SetOperation.INTERSECT;
            container.AddChild(childAggregateConfiguration, 1);
            container.AddChild(childAggregateConfiguration2, 2);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(container);

            Assert.IsNull(problem);
        }
        #endregion

        #region SET containers
        [Test]
        public void TestSetContainerUNION_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = new CohortAggregateContainer(Repository, SetOperation.UNION);

            container.AddChild(childContainer);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(childContainer);

            Assert.IsNotNull(problem);
            Assert.AreEqual("SET containers cannot be empty", problem);
        }

        [Test]
        public void TestSetContainer1ChildUNION_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = new CohortAggregateContainer(Repository, SetOperation.UNION);
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

            childContainer.AddChild(childAggregateConfiguration, 0);
            container.AddChild(childContainer);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(childContainer);

            Assert.IsNotNull(problem);
            Assert.AreEqual("SET container operations have no effect if there is only one child within", problem);
        }

        [Test]
        public void TestSetContainer2ChildUNION_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = new CohortAggregateContainer(Repository, SetOperation.UNION);
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
            var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

            childContainer.AddChild(childAggregateConfiguration, 0);
            childContainer.AddChild(childAggregateConfiguration2, 0);
            container.AddChild(childContainer);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(childContainer);

            Assert.IsNull(problem);
        }

        [Test]
        public void TestSetContainerEXCEPT_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = new CohortAggregateContainer(Repository, SetOperation.EXCEPT);

            container.AddChild(childContainer);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(childContainer);

            Assert.IsNotNull(problem);
            Assert.AreEqual("SET containers cannot be empty", problem);
        }

        [Test]
        public void TestSetContainer1ChildEXCEPT_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = new CohortAggregateContainer(Repository, SetOperation.EXCEPT);
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

            childContainer.AddChild(childAggregateConfiguration, 0);
            container.AddChild(childContainer);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(childContainer);

            Assert.IsNotNull(problem);
            Assert.AreEqual("SET container operations have no effect if there is only one child within", problem);
        }

        [Test]
        public void TestSetContainer2ChildEXCEPT_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = new CohortAggregateContainer(Repository, SetOperation.EXCEPT);
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
            var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

            childContainer.AddChild(childAggregateConfiguration, 0);
            childContainer.AddChild(childAggregateConfiguration2, 0);
            container.AddChild(childContainer);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(childContainer);

            Assert.IsNull(problem);
        }

        [Test]
        public void TestSetContainerINTERSECT_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = new CohortAggregateContainer(Repository, SetOperation.INTERSECT);

            container.AddChild(childContainer);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(childContainer);

            Assert.IsNotNull(problem);
            Assert.AreEqual("SET containers cannot be empty", problem);
        }

        [Test]
        public void TestSetContainer1ChildINTERSECT_IsProblem()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = new CohortAggregateContainer(Repository, SetOperation.INTERSECT);
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();

            childContainer.AddChild(childAggregateConfiguration, 0);
            container.AddChild(childContainer);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(childContainer);

            Assert.IsNotNull(problem);
            Assert.AreEqual("SET container operations have no effect if there is only one child within", problem);
        }

        [Test]
        public void TestSetContainer2ChildINTERSECT_IsOk()
        {
            var container = WhenIHaveA<CohortAggregateContainer>();
            var childContainer = new CohortAggregateContainer(Repository, SetOperation.INTERSECT);
            var childAggregateConfiguration = WhenIHaveA<AggregateConfiguration>();
            var childAggregateConfiguration2 = WhenIHaveA<AggregateConfiguration>();

            childContainer.AddChild(childAggregateConfiguration, 0);
            childContainer.AddChild(childAggregateConfiguration2, 0);
            container.AddChild(childContainer);

            var pp = new CatalogueProblemProvider();
            pp.RefreshProblems(new CatalogueChildProvider(Repository, null, new ThrowImmediatelyCheckNotifier(), null));
            var problem = pp.DescribeProblem(childContainer);

            Assert.IsNull(problem);
        }


        #endregion

    }
}