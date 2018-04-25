﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.Tutorials;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.TransparentHelpSystem;

namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{

    /// <summary>
    /// This dialog is shown when the RDMP learns about a new data table in your data repository that you want it to curate.  This can be either following a the successful flat file import
    /// or after selecting an existing table for importing metadata from (See ImportSQLTable).
    /// 
    /// <para>If you click 'Cancel' then no dataset (Catalogue) will be created and you will only have the TableInfo/ColumnInfo collection stored in your RDMP database, you will need to manually wire
    /// these up to a Catalogue or delete them if you decied you want to make the dataset extractable later on. </para>
    /// 
    /// <para>Alternatively you can create a new Catalogue, this will result in a Catalogue (dataset) of the same name as the table and a CatalogueItem being created for each ColumnInfo imported.
    /// If you choose to you can make these CatalogueItems extractable by creating ExtractionInformation too or you may choose to do this by hand later on (in CatalogueItemTab).  It is likely that
    /// you don't want to release every column in the dataset to researchers so make sure to review the extractability of the columns created. </para>
    /// 
    /// <para>You can choose a single extractable column to be the Patient Identifier (e.g. CHI / NHS number etc). This column must be the same (logically/datatype) across all your datasets i.e. 
    /// you can use either CHI number or NHS Number but you can't mix and match (but you could have fields with different names e.g. PatCHI, PatientCHI, MotherCHI, FatherChiNo etc).</para>
    /// 
    /// <para>The final alternative is to add the imported Columns to another already existing Catalogue.  Only use this option if you know it is possible to join the new table with the other 
    /// table(s) that underlie the selected Catalogue (e.g. if you are importing a Results table which joins to a Header table in the dataset Biochemistry on primary/foreign key LabNumber).
    /// If you choose this option you must configure the JoinInfo logic (See JoinConfiguration)</para>
    /// </summary>
    public partial class ConfigureCatalogueExtractabilityUI : Form
    {
        private readonly object[] _extractionCategories;
        
        private IActivateItems _activator;

        private string NotExtractable = "Not Extractable";
        private Catalogue _catalogue;
        private TableInfo _tableInfo;
        private bool _choicesFinalised;
        private HelpWorkflow _workflow;
        private CatalogueItem[] _catalogueItems;
        private bool _ddChangeAllChanged = false;
        public Catalogue CatalogueCreatedIfAny { get { return _catalogue; }}
        public TableInfo TableInfoCreated{get { return _tableInfo; }}

        public ConfigureCatalogueExtractabilityUI(IActivateItems activator, ITableInfoImporter importer)
        {
            InitializeComponent();

            _activator = activator;
                    ColumnInfo[] cols;
                    importer.DoImport(out _tableInfo, out cols);

            var forwardEngineer = new ForwardEngineerCatalogue(_tableInfo, cols, false);
            ExtractionInformation[] eis;
            forwardEngineer.ExecuteForwardEngineering(out _catalogue,out _catalogueItems,out eis);
            
            //Every CatalogueItem is either mapped to a ColumnInfo (not extractable) or a ExtractionInformation (extractable).  To start out with they are not extractable
            foreach (CatalogueItem ci in _catalogueItems)
                olvColumnExtractability.AddObject(new Node(ci, cols.Single(col => ci.ColumnInfo_ID == col.ID)));

            _extractionCategories = new object[]
            {
                NotExtractable,
                ExtractionCategory.Core,
                ExtractionCategory.Supplemental,
                ExtractionCategory.SpecialApprovalRequired,
                ExtractionCategory.Internal,
                ExtractionCategory.Deprecated
            };

            ddCategoriseMany.Items.AddRange(_extractionCategories);

            olvExtractionCategory.AspectGetter += ExtractionCategoryAspectGetter;
            olvColumnExtractability.AlwaysGroupByColumn = olvExtractionCategory;

            olvColumnExtractability.CellEditStarting += TlvColumnExtractabilityOnCellEditStarting;
            olvColumnExtractability.CellEditFinishing += TlvColumnExtractabilityOnCellEditFinishing;
            olvColumnExtractability.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;

            olvIsExtractionIdentifier.AspectPutter += IsExtractionIdentifier_AspectPutter;
            olvIsExtractionIdentifier.AspectGetter += IsExtractionIdentifier_AspectGetter;
            
            olvColumnInfoName.ImageGetter = ImageGetter;
            olvColumnExtractability.RebuildColumns();
        }

        private void IsExtractionIdentifier_AspectPutter(object rowobject, object newvalue)
        {
            var n = (Node) rowobject;

            if (n.ExtractionInformation == null)
                MakeExtractable(n, true, ExtractionCategory.Core);

            Debug.Assert(n.ExtractionInformation != null, "n.ExtractionInformation != null");
            n.ExtractionInformation.IsExtractionIdentifier = (bool)newvalue;
            n.ExtractionInformation.SaveToDatabase();
        }

        private object ImageGetter(object rowObject)
        {
            var n = (Node) rowObject;

            return _activator.CoreIconProvider.GetImage((object) n.ExtractionInformation ?? n.ColumnInfo);
        }

        private object IsExtractionIdentifier_AspectGetter(object rowObject)
        {
            var n = (Node)rowObject;

            if (n.ExtractionInformation == null)
                return false;

            return n.ExtractionInformation.IsExtractionIdentifier;
        }


        private void MakeExtractable(object o, bool shouldBeExtractable, ExtractionCategory? category = null)
        {
            var n = (Node)o;
            
            //if it has extraction information
            if(n.ExtractionInformation != null)
            {
                if(shouldBeExtractable)
                {
                    //if they want to change the extraction category
                    if (category.HasValue && n.ExtractionInformation.ExtractionCategory != category.Value)
                    {
                        n.ExtractionInformation.ExtractionCategory = category.Value;
                        n.ExtractionInformation.SaveToDatabase();
                        olvColumnExtractability.RefreshObject(n);
                    }
                    return;
                }
                else
                {
                    //make it not extractable by deleting the extraction information
                    n.ExtractionInformation.DeleteInDatabase();
                    n.ExtractionInformation = null;
                }
            }
            else
            {
                //it doesn't have ExtractionInformation

                if(!shouldBeExtractable) //it's already not extractable job done
                    return;
                else
                {
                   //make it extractable
                    var newExtractionInformation = new ExtractionInformation((ICatalogueRepository) n.ColumnInfo.Repository, n.CatalogueItem, n.ColumnInfo,n.ColumnInfo.Name);

                    if (category.HasValue)
                    {
                        newExtractionInformation.ExtractionCategory = category.Value;
                        newExtractionInformation.SaveToDatabase();
                    }

                    n.ExtractionInformation = newExtractionInformation;
                }
            }

            olvColumnExtractability.RefreshObject(n);
        }


        private object ExtractionCategoryAspectGetter(object rowobject)
        {
            var n = (Node)rowobject;

            if (n.ExtractionInformation == null)
                return "Not Extractable";

            return n.ExtractionInformation.ExtractionCategory;
        }


        private void TlvColumnExtractabilityOnCellEditStarting(object sender, CellEditEventArgs cellEditEventArgs)
        {
            var n = (Node)cellEditEventArgs.RowObject;

            if (cellEditEventArgs.Column == olvColumnInfoName)
                cellEditEventArgs.Cancel = true;

            if (cellEditEventArgs.Column == olvExtractionCategory)
            {
                var cbx = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Bounds = cellEditEventArgs.CellBounds
                };
                
                cbx.Items.AddRange(_extractionCategories);
                cbx.SelectedItem = n.ExtractionInformation != null ? (object) n.ExtractionInformation.ExtractionCategory : NotExtractable;
                cellEditEventArgs.Control = cbx;
            }
        }

        private void TlvColumnExtractabilityOnCellEditFinishing(object sender, CellEditEventArgs cellEditEventArgs)
        {
            var n = (Node) cellEditEventArgs.RowObject;

            if (cellEditEventArgs.Column == olvExtractionCategory)
            {
                var cbx = (ComboBox) cellEditEventArgs.Control;

                if (Equals(cbx.SelectedItem, NotExtractable))
                    MakeExtractable(n, false, null);
                else
                    MakeExtractable(n, true, (ExtractionCategory) cbx.SelectedItem);
            }
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            olvColumnExtractability.UseFiltering = true;

            var textFilter = new TextMatchFilter(olvColumnExtractability, tbFilter.Text);
            textFilter.Columns = new[] {olvColumnInfoName};
            olvColumnExtractability.ModelFilter = textFilter;
        }

        private void ddCategoriseMany_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filteredObjects = olvColumnExtractability.FilteredObjects.Cast<Node>().ToArray();
            object toChangeTo = ddCategoriseMany.SelectedItem;
            
            if (MessageBox.Show("Set " + filteredObjects.Length + " to '" + toChangeTo + "'?",
                    "Confirm Overwrite?", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {

                foreach (object o in filteredObjects)
                {
                    if (toChangeTo.Equals(NotExtractable))
                        MakeExtractable(o, false);
                    else
                        MakeExtractable(o, true, (ExtractionCategory) toChangeTo);
                }

                _ddChangeAllChanged = true;
            }

        }

        private void FinaliseExtractability()
        {
            new ExtractableDataSet(_activator.RepositoryLocator.DataExportRepository, _catalogue);
        }

        private void btnAddToExisting_Click(object sender, EventArgs e)
        {
            var eis = GetExtractionInformations();

            if (!eis.Any())
            {
                MessageBox.Show("You must set at least one column to extractable before you can add them to another Catalogue");
                return;
            }
            

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator.CoreChildProvider.AllCatalogues, false, false);
                if (dialog.ShowDialog() == DialogResult.OK)

                    if (MessageBox.Show("This will add " + eis.Length + " new columns to " + dialog.Selected + ". Are you sure this is what you want?","Add to existing", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        AddToExistingCatalogue((Catalogue) dialog.Selected,eis);
        }

        private void AddToExistingCatalogue(Catalogue addToInstead, ExtractionInformation[] eis)
        {
            //move all the CatalogueItems to the other Catalogue instead
            foreach (ExtractionInformation ei in eis)
            {
                var ci = ei.CatalogueItem;
                ci.Catalogue_ID = addToInstead.ID;
                ci.SaveToDatabase();
            }
            
            _choicesFinalised = true;
            _catalogue.DeleteInDatabase();
            _catalogue = null;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

            var eis = GetExtractionInformations();

            if (!eis.Any())
            {
                MessageBox.Show("You have not marked any columns as extractable, try selecting an ExtractionCategory for your columns");
                return;
            }

            if (eis.Any(ei=>ei.IsExtractionIdentifier))
                FinaliseExtractability();
            else
            {
                if (MessageBox.Show("You have not chosen a column to be IsExtractionIdentifier, do you wish to continue?", "Confirm", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
            }

            _choicesFinalised = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private ExtractionInformation[] GetExtractionInformations()
        {
            return olvColumnExtractability.Objects.Cast<Node>()
                .Where(n => n.ExtractionInformation != null)
                .Select(ei => ei.ExtractionInformation)
                .ToArray();
        }

        private void ConfigureCatalogueExtractabilityUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!_choicesFinalised)
            {
                if (MessageBox.Show("Your data table will still exist but no Catalogue will be created",
                    "Confirm", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    DialogResult = DialogResult.Cancel;
                    _catalogue.DeleteInDatabase();
                    _catalogue = null;
                }
                else
                    e.Cancel = true;
            }
            else
            {
                if(CatalogueCreatedIfAny != null)
                    _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(CatalogueCreatedIfAny));
            }
        }

        private void ConfigureCatalogueExtractabilityUI_Load(object sender, EventArgs e)
        {
            _workflow = new HelpWorkflow(this, new Guid("74e6943e-1ed8-4c43-89c2-96158c1360fa"), new TutorialTracker(_activator));
            var stage1 = new HelpStage(olvColumnExtractability, "This is a collection of all the column definitions imported, change the Extractable status of one of the columns to make it extractable", () => GetExtractionInformations().Any());
            var stage2 = new HelpStage(olvColumnExtractability, "One of your columns should contain a patient identifier, tick IsExtractionIdentifier on your patient identifier column", () => GetExtractionInformations().Any(ei=>ei.IsExtractionIdentifier));
            var stage3 = new HelpStage(pChangeAll, "Change this dropdown to change all at once", () =>  _ddChangeAllChanged);
            var stage4 = new HelpStage(pFilter, "Type in here if you are trying to find a specific column", () => !string.IsNullOrWhiteSpace(tbFilter.Text));

            stage1.SetNext(stage2);
            stage2.SetNext(stage3);
            stage2.OptionButtonText = "I don't have one of those";
            stage2.OptionDestination = stage3;
            stage3.SetNext(stage4);

            _workflow.RootStage = stage1;
            _workflow.Start();

            helpIcon1.SetHelpText("Configure Extractability", "Click for tutorial", _workflow);
        }

        class Node
        {
            public CatalogueItem CatalogueItem;
            public ColumnInfo ColumnInfo;
            public ExtractionInformation ExtractionInformation;
            
            public Node(CatalogueItem ci, ColumnInfo col)
            {
                CatalogueItem = ci;
                ColumnInfo = col;
            }

            public override string ToString()
            {
                return CatalogueItem.Name;
            }
        }
    }
}
