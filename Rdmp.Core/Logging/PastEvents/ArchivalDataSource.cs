// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;

namespace Rdmp.Core.Logging.PastEvents
{
    /// <summary>
    /// Readonly audit of a historical 'data source' (See HIC.Logging.DataSource) that contributed records to a table that was loaded in the last (See 
    /// ArchivalTableLoadInfo).
    /// </summary>
    public class ArchivalDataSource : IArchivalLoggingRecordOfPastEvent, IComparable
    {

        public string MD5 { get; internal set; }
        public string Source { get; internal set; }
        public string Archive { get; internal set; }
        public DateTime? OriginDate { get; internal set; }
        public int ID { get; set; }

        public ArchivalDataSource(DbDataReader r)
        {
            
            ID = Convert.ToInt32(r["ID"]);
            var od = r["originDate"];

            if (od == null || od == DBNull.Value)
                OriginDate = null;
            else
                OriginDate = (DateTime)od;

            Source = r["source"] as string;
            Archive = r["archive"] as string;
            MD5 = r["MD5"] as string;
        }
        
        public string ToShortString()
        {

            var s = ToString();
            if (s.Length > ArchivalDataLoadInfo.MaxDescriptionLength)
                return s.Substring(0, ArchivalDataLoadInfo.MaxDescriptionLength) + "...";
            return s;
        }

        public override string ToString()
        {
            return  "Source:" + Source + (string.IsNullOrWhiteSpace(MD5) ? "" : "(MD5=" + MD5 + ")");
        }

        public int CompareTo(object obj)
        {
            var other = obj as ArchivalDataSource;
            if (other != null)
                if (OriginDate == other.OriginDate)
                    return 0;
                else
                {

                    if (!OriginDate.HasValue)
                        return -1;

                    if (!other.OriginDate.HasValue)
                        return 1;
                    
                    return OriginDate > other.OriginDate ? 1 : -1;

                }

            return System.String.Compare(ToString(), obj.ToString(), System.StringComparison.Ordinal);
        }
    }
}