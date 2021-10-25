﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataViewing;
using System.IO;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Fetches the private and release identifiers for a given <see cref="ExtractableCohort"/> optionally to file
    /// </summary>
    class ExecuteCommandViewCohortSample : BasicCommandExecution
    {
        public ExtractableCohort Cohort { get; }
        public int Sample { get; }
        public FileInfo ToFile { get; }

        public ExecuteCommandViewCohortSample(IBasicActivateItems activator,
            [DemandsInitialization("The cohort that you want to fetch records for")]
            ExtractableCohort cohort,
            [DemandsInitialization("Optional. The maximum number of records to retrieve")]
            int sample = 100,
            [DemandsInitialization("Optional. A file to write the records to instead of the console")]
            FileInfo toFile = null):base(activator)
        {
            Cohort = cohort;
            Sample = sample;
            ToFile = toFile;
        }
        public override void Execute()
        {
            base.Execute();

            var collection = new ViewCohortExtractionUICollection(Cohort) {
                Top = Sample
            };

            if(ToFile == null)
            {
                BasicActivator.ShowData(collection);
            }
            else
            {
                ExtractTableVerbatim.ExtractDataToFile(collection, ToFile);
            }
            
        }
    }
}