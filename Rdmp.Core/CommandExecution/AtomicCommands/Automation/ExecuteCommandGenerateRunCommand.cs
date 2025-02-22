// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Automation
{
    public class ExecuteCommandGenerateRunCommand : AutomationCommandExecution, IAtomicCommand
    {
        public ExecuteCommandGenerateRunCommand(IBasicActivateItems activator, Func<RDMPCommandLineOptions> commandGetter)
            : base(activator, commandGetter)
        {

        }

        public override string GetCommandHelp()
        {
            return "Generates the execute command line invocation (including arguments)";
        }

        public override Image<Rgba32> GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Clipboard);
        }

        public override void Execute()
        {
            base.Execute();

            BasicActivator.Show(GetCommandText());
        }
    }
}