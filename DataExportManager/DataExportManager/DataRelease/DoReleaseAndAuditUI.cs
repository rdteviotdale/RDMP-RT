﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.Refreshing;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable.Revertable;
using RDMPObjectVisualisation.Pipelines;
using RDMPObjectVisualisation.Pipelines.PluginPipelineUsers;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Progress;

namespace DataExportManager.DataRelease
{
    /// <summary>
    /// Shows all the currently selected configurations you are trying to release in a DataReleaseUI (See DataReleaseUI and ConfigurationReleasePotentialUI for fuller documentation about
    /// the releasable process).
    /// </summary>
    public partial class DoReleaseAndAuditUI : UserControl
    {
        private Project _project;
        
        public Dictionary<IExtractionConfiguration, List<ReleasePotential>> ConfigurationsForRelease { get; private set; }
        
        public DoReleaseAndAuditUI()
        {
            InitializeComponent();

            ConfigurationsForRelease = new Dictionary<IExtractionConfiguration, List<ReleasePotential>>();
        }

        private ReleaseEnvironmentPotential _environmentPotential;

        private ReleaseState _releaseState = ReleaseState.Nothing;
        private IActivateItems _activator;

        public void AddToRelease(ReleasePotential[] datasetReleasePotentials, ReleaseEnvironmentPotential environmentPotential)
        {
            if (_releaseState == ReleaseState.DoingPatch)
            {
                MessageBox.Show("You are already trying to do a patch release, you cannot also do a proper release");
                return;
            }

            if (!datasetReleasePotentials.Any())
            {
                MessageBox.Show("You cannot release zero datasets!");
                return;
            }
            
            IExtractionConfiguration toAdd = datasetReleasePotentials.First().Configuration;
            
            if(datasetReleasePotentials.Any(p=>p.Configuration.ID != toAdd.ID))
                throw new Exception("ReleasePotential array contained datasets from multiple configurations");

            if(toAdd.Project_ID != _project.ID)
                throw new Exception("Mismatch between ProjectID of datasets selected for release and what this UI component recons the project is");

            if (ConfigurationsForRelease.Keys.Any(config=>config.ID == toAdd.ID))
            {
                MessageBox.Show("Configuration already added!");
                return;
            }

            if (ConfigurationsForRelease.Keys.Any(config => config.ReleaseTicket != toAdd.ReleaseTicket))
            {
                MessageBox.Show("You cannot add a Configuration belonging to another Release Ticket!");
                return;
            }

            CheckForCumulativeExtractionResults(datasetReleasePotentials);

            if (_environmentPotential != null)
                if (_environmentPotential.Assesment != _environmentPotential.Assesment)
                    throw new Exception("We have been given two ReleaseEnvironmentPotentials but they have different .Assesment properties");

            _environmentPotential = environmentPotential;

            CheckForMixedReleaseTypes(ConfigurationsForRelease.SelectMany(cfr => cfr.Value).Union(datasetReleasePotentials));

            ConfigurationsForRelease.Add((ExtractionConfiguration) toAdd, datasetReleasePotentials.ToList());
            _releaseState = ReleaseState.DoingProperRelease;

            ReloadTreeView();
        }

        private void CheckForMixedReleaseTypes(IEnumerable<ReleasePotential> datasetReleasePotentials)
        {
            if (datasetReleasePotentials.Select(rp => rp.DatasetExtractionResult.DestinationType).Distinct().Count() > 1)
                throw new Exception(
                    "There is a mix of extraction types (DB and filesystem) in the datasets you are trying to add. This is not allowed.");
        }

        private void CheckForCumulativeExtractionResults(ReleasePotential[] datasetReleasePotentials)
        {
            var staleDatasets = datasetReleasePotentials.Where(
                p => p.DatasetExtractionResult.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted).ToArray();
           
            if (staleDatasets.Any())
                throw new Exception(
                    "The following ReleasePotentials relate to expired (stale) extractions, you or someone else has executed another data extraction since you added this dataset to the release.  Offending datasets were (" +
                    string.Join(",", staleDatasets.Select(ds => ds.ToString())) + ").  You can probably fix this problem by reloading/refreshing the Releaseability window.  If you have already added them to a planned Release you will need to add the newly recalculated one instead.");
        }

        public void AddPatchRelease(ReleasePotential toPatchIn, ReleaseEnvironmentPotential environmentPotential)
        {
            if (_releaseState == ReleaseState.DoingProperRelease)
            {
                MessageBox.Show("You are already trying to do a Full release, you cannot also do a patch");
                return;
            }

            if (_environmentPotential != null)
                if (_environmentPotential.Assesment != _environmentPotential.Assesment) //DAFUCK?
                    throw new Exception("We have been given two ReleaseEnvironmentPotentials but they have different .Assesment properties");

            _environmentPotential = environmentPotential;

            if (toPatchIn.Configuration.Project_ID != _project.ID)
                throw new Exception("Mismatch between ProjectID of datasets selected for release and what this UI component recons the project is");

            if (ConfigurationsForRelease.Where(cfr => cfr.Key == toPatchIn.Configuration).Any(kvp => kvp.Value.Any(releasePotential => releasePotential.DataSet.ID == toPatchIn.DataSet.ID)))
            {
                MessageBox.Show("Dataset already included in the patch");
                return;
            }

            if (!ConfigurationsForRelease.ContainsKey(toPatchIn.Configuration))
                ConfigurationsForRelease.Add(toPatchIn.Configuration,new List<ReleasePotential>());

            ConfigurationsForRelease[toPatchIn.Configuration].Add(toPatchIn);
            _releaseState = ReleaseState.DoingPatch;
            ReloadTreeView();
        }

        private void ReloadTreeView()
        {
            treeView1.Nodes.Clear();

            foreach (var kvp in ConfigurationsForRelease)
            {
                TreeNode configurationNode = new TreeNode();
                configurationNode.Tag = kvp.Key;
                configurationNode.Text = kvp.Key.Name;
                
                treeView1.Nodes.Add(configurationNode);

                foreach (ReleasePotential potential in kvp.Value)
                {
                    TreeNode datasetReleaseNode = new TreeNode();
                    datasetReleaseNode.Tag = potential;
                    datasetReleaseNode.Text = potential.DataSet + " (" + potential.DatasetExtractionResult + ")";
                    configurationNode.Nodes.Add(datasetReleaseNode);
                }
            }

            if(treeView1.Nodes.Count == 0)
                _releaseState = ReleaseState.Nothing;
            
        }
        
        private void btnRelease_Click(object sender, EventArgs e)
        {
            if (ConfigurationsForRelease.Count == 0)
            {
                MessageBox.Show("Nothing yet selected for release");
                return;
            }

            if (_pipelineUI.Pipeline == null)
                return;
            
            // the data listener in this case is a window for monitoring the progress
            var progressUI = new ProgressUI();

            //the release context for the project
            var context = new ReleaseUseCase(_project, GetReleaseData());

            //translated into an engine
            var engine = context.GetEngine(_pipelineUI.Pipeline, progressUI);
            var checksUI = new PopupChecksUI("Checking engine", false, allowYesNoToAll: false);
            checksUI.Check(engine);
            
            if (checksUI.GetWorst() < CheckResult.Fail)
            {
                try
                {
                    _activator.ShowWindow(progressUI, false);
            
                    progressUI.ShowRunning(true);
                    //and execute it
                    engine.ExecutePipeline(new GracefulCancellationToken());
                }
                finally
                {
                    progressUI.ShowRunning(false);
                }
            }

        }
        
        private void treeView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {

                if(treeView1.SelectedNode != null)
                {
                    ExtractionConfiguration toDelete = treeView1.SelectedNode.Tag as ExtractionConfiguration;

                    if(toDelete != null)
                    {
                        ConfigurationsForRelease.Remove(toDelete);
                        ReloadTreeView();
                    }
                }
            }
        }

        public void SetProject(IActivateItems activator, Project project)
        {
            _activator = activator;
            _project = project;
            ConfigurationsForRelease.Clear();
            ReloadTreeView();
        
            SetupPipeline();
        }

        private IPipelineSelectionUI _pipelineUI;

        private void SetupPipeline()
        {
            if (_pipelineUI == null)
            {
                var releaseData = GetReleaseData();
                releaseData.IsDesignTime = true;
                var context = new ReleaseUseCase(_project, releaseData);
                _pipelineUI = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, null, context).Create("Release", DockStyle.Fill, pnlPipeline);
            }
        }

        private ReleaseData GetReleaseData()
        {
            return new ReleaseData
            {
                ConfigurationsForRelease = ConfigurationsForRelease,
                EnvironmentPotential = _environmentPotential,
                ReleaseState = _releaseState
            };
        }
    }

    
}