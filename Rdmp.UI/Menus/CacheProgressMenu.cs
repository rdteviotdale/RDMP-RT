// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus
{
    internal class CacheProgressMenu : RDMPContextMenuStrip
    {
        public CacheProgressMenu(RDMPContextMenuStripArgs args, CacheProgress cacheProgress)
            : base(args, cacheProgress)
        {
            Add(new ExecuteCommandEditCacheProgress(args.ItemActivator, cacheProgress));

            Add(new ExecuteCommandSetPermissionWindow(_activator,cacheProgress));
            
            ReBrandActivateAs("Execute Caching",RDMPConcept.CacheProgress,OverlayKind.Execute);
        }
    }
}