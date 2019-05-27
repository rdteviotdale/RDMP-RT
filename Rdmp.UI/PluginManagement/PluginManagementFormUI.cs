// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Sharing.Transmission;
using Rdmp.Core.Startup.PluginManagement;
using Rdmp.UI.CommandExecution.AtomicCommands.Sharing;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Dialogs;
using ReusableUIComponents.Progress;
using ReusableUIComponents.SingleControlForms;

namespace Rdmp.UI.PluginManagement
{
    /// <summary>
    /// Shows all the currently configured Plugins you have uploaded into your Catalogue Database.  Plugins are .zip files which contain one or more dlls.  The name of the zip file is the name
    /// of the Plugin.  You can upload a plugin by dropping the zip file into left hand tree view (where it says 'Drop Here').  Once uploaded, all the contents of the zip file are saved in the
    /// LoadModuleAssembly table in your Catalogue Database.  Then when any user launches an RDMP program they will receive a copy of the plugin downloaded into their %appdata%\MEF directory.
    /// 
    /// <para>Clicking a plugin will expand to show all the dll files in the plugin.  Expanding a dll will show all the list of RDMP compatible (Exported) classes in that dll.  Clicking on one of the
    /// classes will open populate the dependencies and allow you to view the MISL of the plugin (See PluginDependencyVisualisation).</para>
    /// 
    /// <para>Pressing the 'Delete' key on your keyboard will delete the selected Plugin or Dll from the LoadModuleAssembly table in your Catalogue Database.  This will not immediately unload the 
    /// plugin locally because all the plugins will be currently read locked however the next time you restart the application (or start a new RDMP application) the local copies of the plugin
    /// will also be deleted. </para>
    /// </summary>
    public partial class PluginManagementFormUI : RDMPForm
    {
        public PluginManagementFormUI(IActivateItems activator):base(activator)
        {
            InitializeComponent();

            var sink = new SimpleDropSink();

            sink.CanDrop += sink_CanDrop;
            sink.Dropped += sink_Dropped;
            
            olvPlugins.DropSink = sink;
            olvPlugins.FormatRow += TreeListViewOnFormatRow;
            olvPlugins.AlwaysGroupByColumn = olvPluginName;
            olvPlugins.ItemActivate += Plugins_Activate;

            olvPluginName.AspectGetter = PluginName_AspectGetter;
            olvVersion.AspectGetter = Version_AspectGetter;

            
            olvLegacyPlugins.AlwaysGroupByColumn = olvLegacyPluginName;
            olvLegacyPluginName.AspectGetter = PluginName_AspectGetter;
            olvLegacyVersion.AspectGetter = (s)=>"Unknown";
        }

        private void Plugins_Activate(object sender, EventArgs e)
        {
            try
            {
                var a = GetAssemblyForLoadModuleAssembly(((ObjectListView) sender).SelectedObject as LoadModuleAssembly);
                if (a == null || string.IsNullOrWhiteSpace(a.Location))
                    return;
            
                var f = new FileInfo(a.Location);

                if(f.Exists)
                    UsefulStuff.GetInstance().ShowFileInWindowsExplorer(f);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }

        }

        private object PluginName_AspectGetter(object rowobject)
        {
            var lma = rowobject as LoadModuleAssembly;

            if (lma == null)
                return null;

            var p = lma.Plugin;

            return p.Name.Replace(".zip", "") + " (" + p.PluginVersion +")";
        }

        private object Version_AspectGetter(object rowobject)
        {
            var lma = rowobject as LoadModuleAssembly;

            if (lma == null || analysers == null)
                return null;

            Assembly a = GetAssemblyForLoadModuleAssembly(lma);

            if (a == null || a.Location == null)
                return null;

            var v = FileVersionInfo.GetVersionInfo(a.Location);
            return v.FileVersion;
        }

        private Assembly GetAssemblyForLoadModuleAssembly(LoadModuleAssembly lma)
        {
            if (lma == null)
                return null;

            //not analysed yet
            if (!analysers.ContainsKey(lma.Plugin))
                return null;

            if (!analysers[lma.Plugin].Reports.ContainsKey(lma))
                return null;

            return analysers[lma.Plugin].Reports[lma].Assembly;
        }

        private void TreeListViewOnFormatRow(object sender, FormatRowEventArgs formatRowEventArgs)
        {
            var plugin = formatRowEventArgs.Model as Core.Curation.Data.Plugin;
            var lma = formatRowEventArgs.Model as LoadModuleAssembly;
            var part = formatRowEventArgs.Model as PluginPart;
            var exception = formatRowEventArgs.Model as Exception;

            if(plugin != null)
            {
                if (!analysers.ContainsKey(plugin))
                {
                    formatRowEventArgs.Item.ForeColor = Color.DimGray;
                    return;
                }

                if (analysers[plugin].Reports.Any())
                {
                    var worstStatus = analysers[plugin].Reports.Min(kvp => kvp.Value.Status);
                    formatRowEventArgs.Item.ForeColor = worstStatus == PluginAssemblyStatus.Healthy ? Color.Green : Color.Red;
                }
                else
                {
                    formatRowEventArgs.Item.ForeColor = Color.IndianRed;
                }
            }
            
            if (lma != null)
            {
                if (!analysers.Keys.Any(p => p.ID == lma.Plugin_ID))
                {
                    formatRowEventArgs.Item.ForeColor = Color.DimGray;
                    return;
                }

                var report = analysers[lma.Plugin].Reports[lma];
                formatRowEventArgs.Item.ForeColor = report.Status == PluginAssemblyStatus.Healthy? Color.Green: Color.Red;

                if (report.Status == PluginAssemblyStatus.Healthy && report.Parts.Any())
                    formatRowEventArgs.Item.ForeColor = Color.LimeGreen;
            }

            if (part != null)
                formatRowEventArgs.Item.ForeColor = part.Dependencies.Any(d => d.Exception != null)? Color.Red: Color.Black;

            if(exception != null)
            {
                formatRowEventArgs.Item.Font = new Font(formatRowEventArgs.Item.Font, FontStyle.Underline);
                formatRowEventArgs.Item.ForeColor = Color.Blue;
            }
            
        }

        #region Drag and Drop To Create New Plugins
        void sink_CanDrop(object sender, OlvDropEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            DataObject dataObject = e.DataObject as DataObject;

            if (dataObject != null)
            {
                StringCollection files = dataObject.GetFileDropList();
                
                if(files.Count >= 0)
                {
                    string[] zipFiles = files.Cast<string>().Where(s => s.EndsWith(PackPluginRunner.PluginPackageSuffix)).ToArray();

                    if(zipFiles.Any())
                        e.Effect = DragDropEffects.Copy;
                }
            }
        }
        void sink_Dropped(object sender, OlvDropEventArgs e)
        {
            var zipFiles = ((DataObject)e.DataObject).GetFileDropList().Cast<string>().Where(s => s.EndsWith(PackPluginRunner.PluginPackageSuffix)).ToArray();

            if (!zipFiles.Any())
                return;

            foreach (string file in zipFiles)
                AddPlugin(file);

            PromptRestart();
        }

        private void AddPlugin(string file)
        {
            var checks = new PopupChecksUI("Uploading Plugin", false);
            var f = new FileInfo(file);
            
            
            if(f.Exists)
            {
                var pluginProcessor = new PackPluginRunner(new PackOptions(){File = file});
            
                pluginProcessor.Run(Activator.RepositoryLocator,null,new PopupChecksUI(file,false),new GracefulCancellationToken());

                RefreshObjects();
            }
        }

        private void RefreshObjects()
        {
            foreach (var o in olvPlugins.Objects.OfType<DatabaseEntity>().ToArray())
            {
                if(!o.Exists())
                    olvPlugins.RemoveObject(o);
            }
        }

        private void PromptRestart()
        {
            if(MessageBox.Show("Application must restart for new Plugin(s) to be loaded, would you like to restart now","Restart Now",MessageBoxButtons.YesNo) == DialogResult.Yes)
                Application.Restart();
        }

        private string _version;

        #endregion

        private IList<Core.Curation.Data.Plugin> wrongPlugins;
        private IList<Core.Curation.Data.Plugin> compatiblePlugins;
        BackgroundWorker analyser;

        private void RefreshUIFromDatabase()
        {
            if (analyser != null)
            {
                MessageBox.Show("Cannot refresh at this time,plugin analysis is still running");
                return;
            }

            olvPlugins.ClearObjects();
            olvLegacyPlugins.ClearObjects();

            compatiblePlugins = Activator.RepositoryLocator.CatalogueRepository.PluginManager.GetCompatiblePlugins().ToList();
            wrongPlugins = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Plugin>().Except(compatiblePlugins).ToList();

            olvPlugins.AddObjects(compatiblePlugins.SelectMany(p => p.LoadModuleAssemblies).ToArray());
            olvLegacyPlugins.AddObjects(wrongPlugins.SelectMany(p => p.LoadModuleAssemblies).ToArray());

            analyser = new BackgroundWorker();
            analyser.DoWork += analyser_DoWork;
            analyser.RunWorkerCompleted += analyser_RunWorkerCompleted;
            analyser.RunWorkerAsync();
        }

        void analyser_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            analyser = null;
        }

        void analyser_DoWork(object sender, DoWorkEventArgs e)
        {
            analysers.Clear();

            var mef = Activator.RepositoryLocator.CatalogueRepository.MEF;

            foreach (Core.Curation.Data.Plugin plugin in compatiblePlugins)
            {
                var pluginDir = plugin.GetPluginDirectoryName(mef.DownloadDirectory);
                var pa = new PluginAnalyser(plugin, new DirectoryInfo(pluginDir), mef.SafeDirectoryCatalog);

                pa.ProgressMade += pa_ProgressMade;

                pa.Analyse();
                analysers.Add(plugin, pa);
            }
        }

        void pa_ProgressMade(PluginAnalyser sender, PluginAnalyserProgressEventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => pa_ProgressMade(sender, eventArgs)));
                return;
            }
            
            pbAnalysing.Maximum = eventArgs.ProgressMax;
            pbAnalysing.Value = eventArgs.Progress;
            lblProgressAnalysing.Text = eventArgs.CurrentAssemblyBeingProcessed != null
                ? "Analysing " + eventArgs.CurrentAssemblyBeingProcessed.Name
                : "Done";

        }

        private Dictionary<Core.Curation.Data.Plugin, PluginAnalyser> analysers = new Dictionary<Core.Curation.Data.Plugin, PluginAnalyser>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(VisualStudioDesignMode)
                return;

            RefreshUIFromDatabase();
        }

        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var olv = (ObjectListView) sender;

                var deleteables = olv.SelectedObjects.OfType<IDeleteable>().ToArray();

                if (!deleteables.Any())
                    return;
                
                
                if(MessageBox.Show("Are you sure you want to delete? (Changes will take effect after restart)","Confirm Deleting", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    foreach (var d in deleteables)
                    {
                        d.DeleteInDatabase();
                        olvPlugins.RemoveObject(d);
                        olvLegacyPlugins.RemoveObject(d);
                    }

                    //delete any plugins for which there are no dlls left
                    foreach (Core.Curation.Data.Plugin emptyPlugin in Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Plugin>().Where(p => !p.LoadModuleAssemblies.Any()))
                        emptyPlugin.DeleteInDatabase();
                }
            }
        }

        private void treeListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            var lma = olvPlugins.SelectedObject as LoadModuleAssembly;
            var cpa = olvPlugins.SelectedObject as PluginPart;

            if(lma != null)
            {
                if(!analysers.ContainsKey(lma.Plugin))
                    return;

                pluginDependencyVisualisation1.Select(analysers[lma.Plugin].Reports[lma]);
                
            }
            else
            if(cpa != null)
                pluginDependencyVisualisation1.Select(cpa);
            else
                pluginDependencyVisualisation1.ClearSelection();

        }

        private void treeListView_ItemActivate(object sender, EventArgs e)
        {
            if(olvPlugins.SelectedObject is Exception)
                ExceptionViewer.Show((Exception)olvPlugins.SelectedObject);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Plugins|*"+PackPluginRunner.PluginPackageSuffix+"|;";
            fd.CheckFileExists = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                AddPlugin(fd.FileName);
                PromptRestart();
            }

        }

        private void btnSaveToRemote_Click(object sender, EventArgs e)
        {
            if (!compatiblePlugins.Any())
            {
                MessageBox.Show(this, "There are no compatible plugins in the system...", "Error");
                return;
            }

            var barsUI = new ProgressBarsUI("Pushing to remotes", true);
            var service = new RemotePushingService(Activator.RepositoryLocator, barsUI);
            var f = new SingleControlForm(barsUI);
            f.Show();

            service.SendToAllRemotes(compatiblePlugins.ToArray(), barsUI.Done);
        }

        private void btnExportToDisk_Click(object sender, EventArgs e)
        {
            var cmd = new ExecuteCommandExportObjectsToFileUI(Activator, compatiblePlugins.ToArray());
            cmd.Execute();
        }
    }
}
