// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Providers.Nodes.LoadMetadataNodes
{
    public class LoadMetadataScheduleNode : Node,IOrderable
    {
        public LoadMetadata LoadMetadata { get; private set; }

        public LoadMetadataScheduleNode(LoadMetadata loadMetadata)
        {
            LoadMetadata = loadMetadata;
        }

        public override string ToString()
        {
            return "Scheduling";
        }

        protected bool Equals(LoadMetadataScheduleNode other)
        {
            return Equals(LoadMetadata, other.LoadMetadata);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoadMetadataScheduleNode) obj);
        }

        public override int GetHashCode()
        {
            return (LoadMetadata != null ? LoadMetadata.GetHashCode() : 0);
        }

        public int Order { get { return 0; } set{} }
    }
}