﻿using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.Interfaces.Data.DataTables;
using HIC.Logging.Listeners;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;

namespace RDMPAutomationService.Runners
{
    /// <summary>
    /// Runs the release process for one or more <see cref="ExtractionConfiguration"/> in the same <see cref="Project"/>.  This is the proces by which we gather all the artifacts
    /// produced by the Extraction Engine (anonymised project extracts, bundled lookups and documents etc) and transmit them somewhere as a final released package.
    /// </summary>
    public class ReleaseRunner:ManyRunner
    {
        private readonly ReleaseOptions _options;
        private Pipeline _pipeline;
        private IProject _project;
        private IExtractionConfiguration[] _configurations;
        private ISelectedDataSets[] _selectedDatasets;

        public ReleaseRunner(ReleaseOptions options):base(options)
        {
            _options = options;
        }

        protected override void Initialize()
        {
            _pipeline = RepositoryLocator.CatalogueRepository.GetObjectByID<Pipeline>(_options.Pipeline);
            
            //some datasets only
            _selectedDatasets = RepositoryLocator.DataExportRepository.GetAllObjectsInIDList<SelectedDataSets>(_options.SelectedDataSets).ToArray();

            //get all configurations user has picked or are refernced by _selectedDatasets
            HashSet<int> configurations = new HashSet<int>(_options.Configurations);
            foreach (ISelectedDataSets selectedDataSets in _selectedDatasets)
                configurations.Add(selectedDataSets.ExtractionConfiguration_ID);

            //fetch them all by ID
            _configurations = RepositoryLocator.DataExportRepository.GetAllObjectsInIDList<ExtractionConfiguration>(configurations).ToArray();

            if (!_configurations.Any())
                throw new Exception("No Configurations have been selected for release");

            _project = _configurations.Select(c => c.Project).Distinct().Single();
        }

        protected override void AfterRun()
        {
            
        }

        protected override ICheckable[] GetCheckables()
        {
            List<ICheckable> toReturn = new List<ICheckable>();

            List<ReleasePotential> ReleasePotentials = new List<ReleasePotential>();

            if(_options.ReleaseGlobals)
                toReturn.Add(new GlobalsReleaseChecker(_configurations));

            foreach (IExtractionConfiguration configuration in _configurations)
                toReturn.AddRange(GetReleasePotentials(configuration));

            return toReturn.ToArray();
        }

        private List<ReleasePotential> GetReleasePotentials(IExtractionConfiguration configuration)
        {
            var toReturn = new List<ReleasePotential>();

            //create new ReleaseAssesments
            foreach (ISelectedDataSets selectedDataSet in GetSelectedDataSets(configuration))//todo only the ones user ticked
            {

                var extractionResults = configuration.CumulativeExtractionResults.FirstOrDefault(r => r.IsFor(selectedDataSet));

                //if it has never been extracted
                if (extractionResults == null || extractionResults.DestinationDescription == null)
                    toReturn.Add(new NoReleasePotential(RepositoryLocator, selectedDataSet)); //the potential is ZERO to release this dataset
                else
                {
                    //it's been extracted!, who extracted it?
                    var destinationThatExtractedIt = (IExecuteDatasetExtractionDestination)new ObjectConstructor().Construct(extractionResults.GetDestinationType());

                    //destination tell us how releasable it is
                    var releasePotential = destinationThatExtractedIt.GetReleasePotential(RepositoryLocator, selectedDataSet);

                    //it is THIS much releasability!
                    toReturn.Add(releasePotential);
                }
            }

            return toReturn;
        }

        protected override object[] GetRunnables()
        {
            var data = new ReleaseData(RepositoryLocator);

            data.EnvironmentPotential = new ReleaseEnvironmentPotential(_configurations.First());

            foreach (IExtractionConfiguration configuration in _configurations)
            {
                var potentials = GetReleasePotentials(configuration);
                data.ConfigurationsForRelease.Add(configuration,potentials);
            }

            return new []{new ReleaseUseCase(_project,data)};
        }

        protected override void ExecuteRun(object runnable, OverrideSenderIDataLoadEventListener listener)
        {
            var useCase = (ReleaseUseCase) runnable;
            var engine = useCase.GetEngine(_pipeline, listener);
            engine.ExecutePipeline(Token);
        }

        private IEnumerable<ISelectedDataSets> GetSelectedDataSets(IExtractionConfiguration configuration)
        {
            //todo only the ones user ticked

            //are we only releasing some of the datasets?
            var onlySomeDatasets = _selectedDatasets.Where(sds => sds.ExtractionConfiguration_ID == configuration.ID).ToArray();

            if (onlySomeDatasets.Any())
                return onlySomeDatasets;

            //no, we are releasing all of them
            return configuration.SelectedDataSets;
        }
    }
}
