// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTableUI;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs;
using Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.Menus.MenuItems;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class TableInfoMenu : RDMPContextMenuStrip
    {
        private DataAccessCredentials[] _availableCredentials;

        public TableInfoMenu(RDMPContextMenuStripArgs args, TableInfo tableInfo)
            : base(args, tableInfo)
        {
            Add(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator, false));
            Add(new ExecuteCommandCreateNewCatalogueFromTableInfo(_activator, tableInfo));

            Items.Add(new ToolStripSeparator());
            Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, null, tableInfo));
            Add(new ExecuteCommandAddJoinInfo(_activator, tableInfo));
            Items.Add(new ToolStripSeparator());

            Items.Add("Synchronize TableInfo ", CatalogueIcons.Sync, delegate { TableInfo_Click(tableInfo); });
            Items.Add("Synchronize ANO Configuration ", CatalogueIcons.Sync, delegate { SynchronizeANOConfiguration_Click(tableInfo); });

            Items.Add("Add ColumnInfo ", null, delegate { AddColumnInfo_Click(tableInfo); });

            _availableCredentials = RepositoryLocator.CatalogueRepository.GetAllObjects<DataAccessCredentials>();
            var addPermission = new ToolStripMenuItem("Add Credential Usage Permission", _activator.CoreIconProvider.GetImage(RDMPConcept.DataAccessCredentials, OverlayKind.Add), (s, e) => AddCredentialPermission(tableInfo));
            addPermission.Enabled = _availableCredentials.Any();
            Items.Add(addPermission);

            Items.Add(new ToolStripSeparator());
            Add(new ExecuteCommandViewData(_activator, tableInfo));

            Add(new ExecuteCommandScriptTable(_activator, tableInfo));

            Items.Add(new ToolStripSeparator());
            Items.Add("Create Shadow _Archive Table (Do not create on highly volatile tables!)", CatalogueIcons.Backup, delegate { CreateBackupTrigger_Click(tableInfo); });

            Items.Add(new ToolStripSeparator());
            Items.Add("Configure Primary Key Collision Resolution ", CatalogueIcons.CollisionResolution, delegate { ConfigurePrimaryKeyCollisionResolution_Click(tableInfo); });

            Items.Add(new ToolStripSeparator());
            Items.Add(new SetDumpServerMenuItem(_activator, tableInfo));
            Add(new ExecuteCommandCreateNewPreLoadDiscardedColumn(_activator, tableInfo));
            Items.Add(new ToolStripSeparator());

            if (tableInfo != null && tableInfo.IsTableValuedFunction)
                Items.Add("Configure Parameters...", _activator.CoreIconProvider.GetImage(RDMPConcept.ParametersNode), delegate { ConfigureTableInfoParameters(tableInfo); });
        }

        private void AddCredentialPermission(TableInfo tableInfo)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_availableCredentials, false, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var cmd = new DataAccessCredentialsCommand((DataAccessCredentials)dialog.Selected);
                var execute = new ExecuteCommandUseCredentialsToAccessTableInfoData(_activator, cmd, tableInfo);

                if(execute.IsImpossible)
                {
                    MessageBox.Show(execute.ReasonCommandImpossible);
                    return;
                }

                execute.Execute();
            }
        }

        private void ConfigurePrimaryKeyCollisionResolution_Click(TableInfo tableInfo)
        {
            var dialog = new ConfigurePrimaryKeyCollisionResolverUI(tableInfo,_activator);
            dialog.ShowDialog(this);
        }


        private void SynchronizeANOConfiguration_Click(TableInfo tableInfo)
        {
            //let use check the TableInfo accurately reflects the underlying database first
            if (MessageBox.Show("Check that TableInfo is synchronized with underlying database first?", "Check database first?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(tableInfo);
                    bool isSynchronized = synchronizer.Synchronize(new ThrowImmediatelyCheckNotifier());
                    if (!isSynchronized)
                    {
                        MessageBox.Show("Unable to synchronize with ANO database because TableInfo is not synchronized with underlying database.");
                        return;
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(
                        "Unable to check synchronization of TableInfo with underlying database (this check must be performed before checking ANO synchronization):" +
                        exception.Message);
                    return;
                }
            }

            var ANOSynchronizer = new ANOTableInfoSynchronizer(tableInfo);

            try
            {
                ANOSynchronizer.Synchronize(new MakeChangePopup(new YesNoYesToAllDialog()));

                MessageBox.Show("ANO synchronization successful");
            }
            catch (ANOConfigurationException e)
            {
                ExceptionViewer.Show(e);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Fatal error while attempting to synchronize (" + exception.Message + ")", exception);
            }

            Publish(tableInfo);
        }

        private void TableInfo_Click(TableInfo tableInfo)
        {
            TableInfoSynchronizer syncher = new TableInfoSynchronizer(tableInfo);

            try
            {
                
                bool wasSynchedsuccessfully = syncher.Synchronize(new MakeChangePopup(new YesNoYesToAllDialog()));

                if (wasSynchedsuccessfully)
                    MessageBox.Show("Synchronization complete, TableInfo is Synchronized with the live database");
                else
                    MessageBox.Show("Synchronization failed");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

            Publish(tableInfo);

            foreach (var c in syncher.ChangedCatalogues)
                Publish(c);
        }

        private void AddColumnInfo_Click(TableInfo tableInfo)
        {
            var newColumnInfo = new ColumnInfo(RepositoryLocator.CatalogueRepository, Guid.NewGuid().ToString(), "fish", tableInfo);
            Publish(newColumnInfo);
        }
        
        private void ConfigureTableInfoParameters(TableInfo tableInfo)
        {
            ParameterCollectionUI.ShowAsDialog(new ParameterCollectionUIOptionsFactory().Create(tableInfo));
        }

        private void CreateBackupTrigger_Click(TableInfo tableInfo)
        {
            try
            {
                var checks = new PopupChecksUI("Implementing archive trigger on table " + tableInfo, true);

                TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(tableInfo);
                if (synchronizer.Synchronize(checks))
                {
                    var pks = tableInfo.ColumnInfos.Where(col => col.IsPrimaryKey).ToArray();

                    if (!pks.Any())
                        MessageBox.Show("Your table does not have any primary keys so cannot support an archive trigger");

                    if (MessageBox.Show("Are you sure you want to create a backup triggered _Archive table using the following primary keys?:" + Environment.NewLine + string.Join("" + Environment.NewLine, pks.Select(p => p.GetRuntimeName())), "Confirm Creating Archive?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {

                        var db = DataAccessPortal.GetInstance().ExpectDatabase(tableInfo, DataAccessContext.InternalDataProcessing);

                        MicrosoftSQLTriggerImplementer implementer = new MicrosoftSQLTriggerImplementer(db.ExpectTable(tableInfo.GetRuntimeName()));
                        implementer.CreateTrigger(checks);
                        MessageBox.Show("Success, look for the new table " + tableInfo.GetRuntimeName() + "_Archive which will contain old records whenever there is an update");
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }
    }
}