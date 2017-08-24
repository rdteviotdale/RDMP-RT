﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ObjectVisualisation;
using CatalogueManager.Refreshing;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableUIComponents.Dependencies;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public abstract class RDMPContextMenuStrip:ContextMenuStrip
    {
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        protected IActivateItems _activator;
        private readonly DatabaseEntity _databaseEntity;

        protected ToolStripMenuItem RefreshObjectMenuItem;
        protected ToolStripMenuItem DependencyViewingMenuItem { get; set; }


        public RDMPContextMenuStrip(IActivateItems activator, DatabaseEntity databaseEntity)
        {
            _activator = activator;
            _databaseEntity = databaseEntity;
            RepositoryLocator = _activator.RepositoryLocator;

            RefreshObjectMenuItem = new ToolStripMenuItem("Refresh",FamFamFamIcons.arrow_refresh,(s,e)=>RefreshDatabaseObject(databaseEntity));
            RefreshObjectMenuItem.ShortcutKeys = Keys.F5;
            RefreshObjectMenuItem.ShowShortcutKeys = true;
            RefreshObjectMenuItem.Enabled = databaseEntity != null;

            var dependencies = databaseEntity as IHasDependencies;

            if (dependencies != null)
                DependencyViewingMenuItem = new ViewDependenciesToolStripMenuItem(dependencies, new CatalogueObjectVisualisation(activator.CoreIconProvider));
        }

        protected void AddCommonMenuItems()
        {
            Items.Add(RefreshObjectMenuItem);
            
            if(DependencyViewingMenuItem != null)
                Items.Add(DependencyViewingMenuItem);

            if(_databaseEntity != null)
            {
                foreach (var plugin in _activator.PluginUserInterfaces)
                {
                    var toAdd = plugin.GetAdditionalRightClickMenuItems(_databaseEntity);

                    if(toAdd != null && toAdd.Any())
                    {
                        Items.Add(new ToolStripSeparator());
                        Items.AddRange(toAdd);
                    }
                }

                Items.Add(new ExpandAllTreeNodesMenuItem(_activator, _databaseEntity));
            }
        }
        

        private void RefreshDatabaseObject(DatabaseEntity databaseEntity)
        {
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(databaseEntity));
        }
    }
}
