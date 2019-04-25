// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ItemActivation.Emphasis;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShow : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IMapsDirectlyToDatabaseTable _objectToShow;
        private readonly int _expansionDepth;
        private readonly bool _useIconAndTypeName;

        public ExecuteCommandShow(IActivateItems activator, IMapsDirectlyToDatabaseTable objectToShow, int expansionDepth, bool useIconAndTypeName=false):base(activator)
        {
            _objectToShow = objectToShow;
            _expansionDepth = expansionDepth;
            _useIconAndTypeName = useIconAndTypeName;
        }

        public override string GetCommandName()
        {
            return _useIconAndTypeName? "Show " + _objectToShow.GetType().Name :base.GetCommandName();
        }

        public override void Execute()
        {
            base.Execute();

            Activator.RequestItemEmphasis(this, new EmphasiseRequest(_objectToShow,_expansionDepth));
        }

        public override string GetCommandHelp()
        {
            return "Opens the containing toolbox collection and shows the object";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return _useIconAndTypeName? iconProvider.GetImage(_objectToShow):null;
        }
    }
}