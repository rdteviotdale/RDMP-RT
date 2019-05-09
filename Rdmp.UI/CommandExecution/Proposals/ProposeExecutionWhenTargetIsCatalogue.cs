// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.Sharing;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.MainFormUITabs;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace Rdmp.UI.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCatalogue:RDMPCommandExecutionProposal<Catalogue>
    {
        public ProposeExecutionWhenTargetIsCatalogue(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(Catalogue target)
        {
            return true;
        }

        public override void Activate(Catalogue c)
        {
            ItemActivator.Activate<CatalogueUI, Catalogue>(c);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, Catalogue targetCatalogue, InsertOption insertOption = InsertOption.Default)
        {
            var sourceFileCollection = cmd as FileCollectionCommand;

            if(sourceFileCollection != null)
                if (sourceFileCollection.IsShareDefinition)
                    return new ExecuteCommandImportCatalogueDescriptionsFromShare(ItemActivator, sourceFileCollection,targetCatalogue);
                else
                    return new ExecuteCommandAddNewSupportingDocument(ItemActivator, sourceFileCollection, targetCatalogue);

            return null;
        }
    }
}