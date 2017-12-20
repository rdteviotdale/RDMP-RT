﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;
using CatalogueManager.PluginChildProvision;
using DataExportLibrary.Data;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using ReusableLibraryCode;

namespace DataExportManager.CommandExecution
{
    public class DataExportPluginUserInterface:PluginUserInterface
    {
        
        public DataExportPluginUserInterface(IActivateItems itemActivator) : base(itemActivator)
        {
            
        }
        
        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            var cata = o as Catalogue;
            

            if (cata != null)
                return GetMenuArray(
                    new ExecuteCommandChangeExtractability(ItemActivator, cata)
                    );

            return null;
        }
    }
}
