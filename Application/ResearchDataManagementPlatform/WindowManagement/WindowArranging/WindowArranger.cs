// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ItemActivation.Arranging;
using Rdmp.UI.LoadExecutionUIs;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.WindowArranging
{
    /// <inheritdoc/>
    public class WindowArranger : IArrangeWindows
    {
        private readonly IActivateItems _activator;
        private readonly WindowManager _windowManager;

        public WindowArranger(IActivateItems activator, WindowManager windowManager, DockPanel mainDockPanel)
        {
            _activator = activator;
            _windowManager =windowManager;
        }
        
        public void SetupEditAnything(object sender, IMapsDirectlyToDatabaseTable o)
        {            
            _activator.RequestItemEmphasis(this, new EmphasiseRequest(o));

            var activate = new ExecuteCommandActivate(_activator, o);

            //activate it if possible
            if (!activate.IsImpossible)
                activate.Execute();
            else
                _activator.RequestItemEmphasis(this, new EmphasiseRequest(o, 1)); //otherwise just show it
        
        }

        public void Setup(WindowLayout target)
        {
            //Do not reload an existing layout
            string oldXml = _windowManager.MainForm.GetCurrentLayoutXml();
            string newXml = target.LayoutData;

            if(AreBasicallyTheSameLayout(oldXml, newXml))
                return;
            
            _windowManager.CloseAllToolboxes();
            _windowManager.CloseAllWindows();
            _windowManager.MainForm.LoadFromXml(target);
        }

        private bool AreBasicallyTheSameLayout(string oldXml, string newXml)
        {
            var patStripActive = @"Active.*=[""\-\d]*";
            oldXml = Regex.Replace(oldXml, patStripActive, "");
            newXml = Regex.Replace(newXml, patStripActive, "");

            return oldXml.Equals(newXml, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}