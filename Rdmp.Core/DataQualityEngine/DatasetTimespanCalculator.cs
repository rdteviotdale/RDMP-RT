// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Reports;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataQualityEngine
{
    /// <summary>
    /// Calculates the date range of data held in a dataset (Catalogue).  Optionally you can 'discardOutliers' this includes any dates in which there are
    /// 1000 times less records than the non zero average month.  For example if you have 3 records in 01/01/2090 then they would be discarded if you had
    ///  an average of 3000+ records per month (after ignoring months where there are no records).  
    /// 
    /// <para>IMPORTANT: You must have run the DQE on the dataset before this class can be used and the results are based on the last DQE run on the dataset not 
    /// the live table</para>
    /// </summary>
    public class DatasetTimespanCalculator : IDetermineDatasetTimespan
    {
        /// <inheritdoc/>
        public string GetHumanReadableTimespanIfKnownOf(Catalogue catalogue,bool discardOutliers, out DateTime? accurateAsOf)
        {

            var result = GetMachineReadableTimespanIfKnownOf(catalogue, discardOutliers, out accurateAsOf);

            if (result.Item1 == null || result.Item2 == null)
                return "Unknown";

            return $"{result.Item1.Value:yyyy-MMM} To {result.Item2.Value:yyyy-MMM}";
        }

        public Tuple<DateTime?, DateTime?> GetMachineReadableTimespanIfKnownOf(Evaluation evaluation, bool discardOutliers)
        {
            var dt = PeriodicityState.GetPeriodicityForDataTableForEvaluation(evaluation, "ALL", false);

            if (dt == null || dt.Rows.Count < 2)
                return Unknown();

            int discardThreshold = discardOutliers ? GetDiscardThreshold(dt) : -1;

            DateTime? minMonth = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Convert.ToInt32(dt.Rows[i]["CountOfRecords"]) > discardThreshold)
                {
                    minMonth = DateTime.Parse(dt.Rows[i][1].ToString());
                    break;
                }
            }

            DateTime? maxMonth = null;
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                if (Convert.ToInt32(dt.Rows[i]["CountOfRecords"]) > discardThreshold)
                {
                    maxMonth = DateTime.Parse(dt.Rows[i][1].ToString());
                    break;
                }
            }

            if (maxMonth == null || minMonth == null)
                return Unknown();

            return Tuple.Create(minMonth, maxMonth);
        }

        public Tuple<DateTime?, DateTime?> GetMachineReadableTimespanIfKnownOf(Catalogue catalogue, bool discardOutliers, out DateTime? accurateAsOf)
        {
            accurateAsOf = null;
            Evaluation mostRecentEvaluation;

            try
            {
                var repo = new DQERepository(catalogue.CatalogueRepository);
                mostRecentEvaluation = repo.GetMostRecentEvaluationFor(catalogue);
            }
            catch (Exception)
            {
                return Unknown();
            }

            if (mostRecentEvaluation == null)
                return Unknown();

            accurateAsOf = mostRecentEvaluation.DateOfEvaluation;

            return GetMachineReadableTimespanIfKnownOf(mostRecentEvaluation, discardOutliers);
        }

        private Tuple<DateTime?, DateTime?> Unknown()
        {
            return Tuple.Create<DateTime?, DateTime?>(null, null);
        }

        private int GetDiscardThreshold(DataTable dt)
        {
            int total = 0;
            int counted = 0;

            foreach (DataRow row in dt.Rows)
            {
                int currentValue = Convert.ToInt32(row["CountOfRecords"]);
                
                if(currentValue == 0)
                    continue;

                total += currentValue;
                counted++;
            }

            double nonZeroAverage = total/(double)counted;

            return (int)(nonZeroAverage/1000);
        }
    }
}
