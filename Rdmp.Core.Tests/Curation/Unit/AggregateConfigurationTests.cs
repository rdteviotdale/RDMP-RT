﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data.Aggregation;
using ReusableLibraryCode.Settings;
using System.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Unit
{
    class AggregateConfigurationTests : UnitTests
    {
        [Test]
        public void TestStripZeroSeries_EmptyTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("col1");
            dt.Columns.Add("col2");

            UserSettings.IncludeZeroSeriesInGraphs = false;

            // empty tables should not get nuked
            AggregateConfiguration.AdjustGraphDataTable(dt);
            Assert.AreEqual(2, dt.Columns.Count);

            dt.Dispose();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestStripZeroSeries_Nulls(bool includeZeroSeries)
        {
            var dt = new DataTable();
            dt.Columns.Add("date");
            dt.Columns.Add("col1");
            dt.Columns.Add("col2");

            dt.Rows.Add("2001",0, 12);
            dt.Rows.Add("2002",null, 333);

            UserSettings.IncludeZeroSeriesInGraphs = includeZeroSeries;

            AggregateConfiguration.AdjustGraphDataTable(dt);

            if(includeZeroSeries)
            {
                Assert.AreEqual(3,dt.Columns.Count);
            }
            else
            {
                // col1 should have been gotten rid of
                Assert.AreEqual(2, dt.Columns.Count);
                dt.Columns.Contains("date");
                dt.Columns.Contains("col2");
            }

            dt.Dispose();
        }
    }
}
