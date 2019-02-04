using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using CatalogueManager.PipelineUIs.Pipelines;
using CatalogueManager.PipelineUIs.Pipelines.PluginPipelineUsers;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

namespace DataExportManager.ProjectUI
{
    /// <summary>
    /// Allows you to change high level attributes of an ExtractionConfiguration in a data extraction Project.  Executing an ExtractionConfiguration involves joining the 
    /// selected datasets against the selected cohort (and substituting the private identifiers for project specific anonymous release identifiers) as well as applying any
    /// configured filters (See ConfigureDatasetUI).  You can have multiple active configurations in a project, for example you might extract 'Prescribing', 'Biochemistry' and 'Demography' for the cohort 'CasesForProject123' and
    /// only datasets 'Biochemistry' and 'Demography' for the cohort 'ControlsForProject123'.
    /// 
    /// <para>The attributes you can change include the name, description, ticketting system tickets etc.</para>
    /// 
    /// <para>You can also define global SQL parameters which will be available to all Filters in all datasets extracted as part of the configuration.</para>
    /// 
    /// <para>You can associate a specific CohortIdentificationConfiguration with the ExtractionConfiguration.  This will allow you to do a 'cohort refresh' (replace the current saved cohort 
    /// identifier list with a new version built by executing the query - helpful if you have new data being loaded regularly and this results in the study cohort changing).</para>
    /// </summary>
    public partial class ExtractionConfigurationUI : ExtractionConfigurationUI_Design, ISaveableUI
    {
        private ExtractionConfiguration _extractionConfiguration;
        private IPipelineSelectionUI _extractionPipelineSelectionUI;

        private IPipelineSelectionUI _cohortRefreshingPipelineSelectionUI;

        public ExtractionConfigurationUI()
        {
            InitializeComponent();
            
            tcRequest.Title = "Request Ticket";
            tcRequest.TicketTextChanged += tcRequest_TicketTextChanged;

            tcRelease.Title = "Release Ticket";
            tcRelease.TicketTextChanged += tcRelease_TicketTextChanged;

            cbxCohortIdentificationConfiguration.PropertySelector = sel => sel.Cast<CohortIdentificationConfiguration>().Select(cic=> cic == null? "<<None>>":cic.Name);
            AssociatedCollection = RDMPCollection.DataExport;
        }
        
        void tcRequest_TicketTextChanged(object sender, EventArgs e)
        {
            if (_extractionConfiguration == null)
                return;

            //don't change if it is already that
            if (_extractionConfiguration.RequestTicket != null && _extractionConfiguration.RequestTicket.Equals(tcRequest.TicketText))
                return;

            _extractionConfiguration.RequestTicket = tcRequest.TicketText;

            _extractionConfiguration.SaveToDatabase();
        }

        void tcRelease_TicketTextChanged(object sender, EventArgs e)
        {
            if (_extractionConfiguration == null)
                return;

            //don't change if it is already that
            if (_extractionConfiguration.ReleaseTicket != null && _extractionConfiguration.ReleaseTicket.Equals(tcRelease.TicketText))
                return;

            _extractionConfiguration.ReleaseTicket = tcRelease.TicketText;
            _extractionConfiguration.SaveToDatabase();
        }

        private bool _bLoading = false;
        
        public override void SetDatabaseObject(IActivateItems activator, ExtractionConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _bLoading = true;
            _extractionConfiguration = databaseObject;

            SetupCohortIdentificationConfiguration();

            SetupPipelineSelectionExtraction();
            SetupPipelineSelectionCohortRefresh();
            
            pbCic.Image = activator.CoreIconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration,OverlayKind.Link);
            
            tbCreated.Text = _extractionConfiguration.dtCreated.ToString();
            tcRelease.TicketText = _extractionConfiguration.ReleaseTicket;
            tcRequest.TicketText = _extractionConfiguration.RequestTicket;

            _bLoading = false;
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, ExtractionConfiguration databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbUsername, "Text", "Username", c => c.Username);
            Bind(tbID,"Text", "ID",c=>c.ID);
            Bind(tbDescription,"Text","Description",c=>c.Description);
        }

        private void SetupCohortIdentificationConfiguration()
        {
            cbxCohortIdentificationConfiguration.DataSource = _activator.CoreChildProvider.AllCohortIdentificationConfigurations;
            cbxCohortIdentificationConfiguration.SelectedItem = _extractionConfiguration.CohortIdentificationConfiguration;
        }

        private void SetupPipelineSelectionCohortRefresh()
        {
            ragSmiley1Refresh.Reset();

            if (_cohortRefreshingPipelineSelectionUI != null)
                return;
            try
            {
                //the use case is extracting a dataset
                var useCase = new CohortCreationRequest(_extractionConfiguration);

                //the user is DefaultPipeline_ID field of ExtractionConfiguration
                var user = new PipelineUser(typeof(ExtractionConfiguration).GetProperty("CohortRefreshPipeline_ID"), _extractionConfiguration);

                //create the UI for this situation
                var factory = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, user, useCase);
                _cohortRefreshingPipelineSelectionUI = factory.Create("Cohort Refresh Pipeline", DockStyle.Fill,pChooseCohortRefreshPipeline);
                _cohortRefreshingPipelineSelectionUI.Pipeline = _extractionConfiguration.CohortRefreshPipeline;
                _cohortRefreshingPipelineSelectionUI.PipelineChanged += _cohortRefreshingPipelineSelectionUI_PipelineChanged;
                _cohortRefreshingPipelineSelectionUI.CollapseToSingleLineMode();
            }
            catch (Exception e)
            {
                ragSmiley1Refresh.Fatal(e);
            }
        }

        void _cohortRefreshingPipelineSelectionUI_PipelineChanged(object sender, EventArgs e)
        {
            ragSmiley1Refresh.Reset();
            try
            {
                new CohortCreationRequest(_extractionConfiguration).GetEngine(_cohortRefreshingPipelineSelectionUI.Pipeline, new ThrowImmediatelyDataLoadEventListener());
            }
            catch (Exception ex)
            {
                ragSmiley1Refresh.Fatal(ex);
            }
        }

        private void SetupPipelineSelectionExtraction()
        {
            //already set i tup
            if (_extractionPipelineSelectionUI != null)
                return;

            //the use case is extracting a dataset
            var useCase = ExtractionPipelineUseCase.DesignTime();

            //the user is DefaultPipeline_ID field of ExtractionConfiguration
            var user = new PipelineUser(typeof(ExtractionConfiguration).GetProperty("DefaultPipeline_ID"), _extractionConfiguration);

            //create the UI for this situation
            var factory = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, user, useCase);
            _extractionPipelineSelectionUI = factory.Create("Extraction Pipeline", DockStyle.Fill, pChooseExtractionPipeline);
            _extractionPipelineSelectionUI.CollapseToSingleLineMode();
        }

        private void cbxCohortIdentificationConfiguration_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(_bLoading)
                return;

            var cic = cbxCohortIdentificationConfiguration.SelectedItem as CohortIdentificationConfiguration;

            if (cic == null)
                _extractionConfiguration.CohortIdentificationConfiguration_ID = null;
            else
                _extractionConfiguration.CohortIdentificationConfiguration_ID = cic.ID;

            SetupPipelineSelectionCohortRefresh();
        }

        private void btnClearCic_Click(object sender, EventArgs e)
        {
            cbxCohortIdentificationConfiguration.SelectedItem = null;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionConfigurationUI_Design, UserControl>))]
    public abstract class ExtractionConfigurationUI_Design : RDMPSingleDatabaseObjectControl<ExtractionConfiguration>
    {
    }
}
