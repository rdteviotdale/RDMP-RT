// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode;
using Tests.Common;

namespace Rdmp.UI.Tests.DesignPatternTests
{
    public class AllObjectsHaveImages:DatabaseTests
    {
        [Test]
        public void AllIHasDependenciesHaveIcons()
        {
            List<string> missingConcepts = new List<string>();


            string[] ExceptionsAllowed = new[]
            {

                "IHasDependencies", //base interface of which nobody is who isn't otherwise on this list
                "ITableInfo", //handled because they are all secretly TableInfos
                "AggregateConfiguration", //handled by AggregateConfigurationStateBasedIconProvider
                "AnyTableSqlParameter", //handled by CatalogueIconProvider looking for ISqlParameter classes
                "ICatalogue", //handled because they are all secretly Catalogues
                "ExtractionFilter", //handled by CatalogueIconProvider looking for IFilter
                "ExtractionFilterParameter",
                //handled by CatalogueIconProvider just like AnyTableSqlParameter by looking for the ISqlParameter classes

                "StackFrame"
                //not required , it's only ever dependent on itself and it doesn't have any visualisation on Catalogue / Export

            };

            foreach (Type type in RepositoryLocator.CatalogueRepository.MEF.GetAllTypes().Where(t => typeof (IHasDependencies).IsAssignableFrom(t) && !t.IsInterface))
            {
                //skip masqueraders
                if(typeof(IMasqueradeAs).IsAssignableFrom(type))
                    continue;

                var typeName = type.Name;
                if (ExceptionsAllowed.Any(s=>s.Equals(typeName)))
                    continue;

                try
                {
                    var c = Enum.Parse(typeof (RDMPConcept), typeName);
                }
                catch (Exception)
                {
                    missingConcepts.Add(typeName);
                }
            }

            Console.WriteLine("The following Database Object Types are missing concepts (and therefore images) in CatalogueManager.exe" + Environment.NewLine + string.Join("," + Environment.NewLine , missingConcepts));

            Assert.AreEqual(0,missingConcepts.Count);
        }
    }
}