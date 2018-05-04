﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CommandExecution.AtomicCommands;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsProject:RDMPCommandExecutionProposal<Project>
    {
        public ProposeExecutionWhenTargetIsProject(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(Project target)
        {
            return true;
        }

        public override void Activate(Project target)
        {
            ItemActivator.Activate<ProjectUI.ProjectUI, Project>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, Project project, InsertOption insertOption = InsertOption.Default)
        {
            //drop a cic on a Project to associate it with that project
            var cicCommand = cmd as CohortIdentificationConfigurationCommand;
            if(cicCommand != null)
                return new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(ItemActivator).SetTarget(cicCommand.CohortIdentificationConfiguration).SetTarget(project);

            var cataCommand = cmd as CatalogueCommand;
            if (cataCommand != null)
                return new ExecuteCommandMakeCatalogueProjectSpecific(ItemActivator).SetTarget(cataCommand.Catalogue).SetTarget(project);

            return null;
        }
    }
}
