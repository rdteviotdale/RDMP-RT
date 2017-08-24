﻿using System.Diagnostics;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections.Providers;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Providers;
using DataExportManager.DataViewing.Collections;
using DataExportManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class CustomDataTableNodeMenu:RDMPContextMenuStrip
    {
        private readonly CustomDataTableNode _customDataTableNode;

        public CustomDataTableNodeMenu(IActivateDataExportItems activator, CustomDataTableNode customDataTableNode): base( activator,null)
        {
            

            _customDataTableNode = customDataTableNode;
            Items.Add("View TOP 100 Records", CatalogueIcons.TableInfo, (s, e) => _activator.ViewDataSample(new ViewCustomCohortTableExtractionUICollection(_customDataTableNode.Cohort, _customDataTableNode.TableName)));

            if (customDataTableNode.Active)
                Items.Add("Disable Custom Data Table", CatalogueIcons.CustomDataTableNotActive, (s, e) => SetActive(false));
            else
                Items.Add("Re Enable Custom Data Table", CatalogueIcons.CustomDataTableNode, (s, e) => SetActive(true));

            Items.Add("Delete Custom Data Table", CatalogueIcons.Warning, (s, e) => Delete());
        }

        private void Delete()
        {
            if(MessageBox.Show("This will involve DELETING the table in your cohort database aswell as the custom table record, are you sure this is what you want?","Confirm Deleting Data",MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _customDataTableNode.Cohort.DeleteCustomData(_customDataTableNode.TableName);
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_customDataTableNode.Cohort));
            }
            
        }

        private void SetActive(bool flag)
        {
            _customDataTableNode.Cohort.SetActiveFlagOnCustomData(_customDataTableNode.TableName,flag);

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_customDataTableNode.Cohort));
        }
    }
}
