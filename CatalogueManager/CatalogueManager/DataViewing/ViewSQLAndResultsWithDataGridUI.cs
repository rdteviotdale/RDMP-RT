﻿using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.AutoComplete;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CatalogueManager.DataViewing
{
    /// <summary>
    /// This dialog is shown when right clicking a ColumnInfo or TableInfo in TableInfoCollectionHost, it uses the DataAccessCredentials of the column/table to execute a TOP 100 query or
    /// a Group By.  The effect of this is to give you a quick view as to the content of the table/column without having to launch 'Sql Management Studio' or whatever other tool you use to
    /// query your data repository.
    /// 
    /// The query is sent using DataAccessContext.InternalDataProcessing
    /// </summary>
    public partial class ViewSQLAndResultsWithDataGridUI : RDMPUserControl, IObjectCollectionControl
    {
        private IViewSQLAndResultsCollection _collection;
        private Scintilla _scintilla;
        private Task _task;
        private DbCommand _cmd;
        private string _originalSql;
        private DiscoveredServer _server;
        private AutoCompleteProvider _autoComplete;
        private bool _isRibbonSetup = false;

        public ViewSQLAndResultsWithDataGridUI()
        {
            InitializeComponent();

            ScintillaTextEditorFactory factory = new ScintillaTextEditorFactory();
            _scintilla = factory.Create();
            splitContainer1.Panel1.Controls.Add(_scintilla);
            _scintilla.TextChanged += _scintilla_TextChanged;

            DoTransparencyProperly.ThisHoversOver(ragSmiley1,dataGridView1);
        }


        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            //if we don't exist!
            if (_collection.DatabaseObjects.Any())
                if(!((IRevertable)_collection.DatabaseObjects[0]).Exists())
                    if(ParentForm != null)
                        ParentForm.Close();
        }


        
        public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
        {
            _collection = (IViewSQLAndResultsCollection) collection;

            btnExecuteSql.Image = activator.CoreIconProvider.GetImage(RDMPConcept.SQL, OverlayKind.Execute);

            var overlayer = new IconOverlayProvider();
            btnResetSql.Image = overlayer.GetOverlay(FamFamFamIcons.text_align_left, OverlayKind.Problem);

            //the autocomplete supporting object
            var autoCompleteObject = _collection.GetAutocompleteObject();

            if(autoCompleteObject != null && _autoComplete == null)
            {
                _autoComplete = new AutoCompleteProviderFactory(activator).Create();
                _autoComplete.RegisterForEvents(_scintilla);
            }
            
            if(!_isRibbonSetup)
            {
                ribbon.SetIconProvider(activator.CoreIconProvider);
                _collection.SetupRibbon(ribbon);
                _isRibbonSetup = true;
            }

            RefreshUIFromDatabase();
        }

        private void RefreshUIFromDatabase()
        {
            try
            {
                _server = DataAccessPortal.GetInstance()
                    .ExpectServer(_collection.GetDataAccessPoint(), DataAccessContext.InternalDataProcessing);

                _server.TestConnection();

                string sql = _collection.GetSql();
                _originalSql = sql;
                //update the editor to show the user the SQL
                _scintilla.Text = sql;

                LoadDataTableAsync(_server, sql);
            }
            catch (QueryBuildingException ex)
            {
                ragSmiley1.SetVisible(true);
                ragSmiley1.Fatal(ex);
            }
            catch (SqlException ex)
            {
                ragSmiley1.SetVisible(true);
                ragSmiley1.Fatal(ex);
            }
        }

        private void LoadDataTableAsync(DiscoveredServer server, string sql)
        {
            //it is already running and not completed
            if (_task != null && !_task.IsCompleted)
            {
                ragSmiley1.SetVisible(true);
                ragSmiley1.Warning(new Exception("Cannot refresh because query is still running"));
                return;
            }

            ragSmiley1.Reset();
            ragSmiley1.SetVisible(false);
            pbLoading.Visible = true;
            llCancel.Visible = true;

            _task = Task.Factory.StartNew(() =>
            {

                int timeout = 1000;
                while (!IsHandleCreated && timeout > 0)
                {
                    timeout -= 10;
                    Thread.Sleep(10);
                }

                try
                {
                    //then execute the command
                    using (DbConnection con = server.GetConnection())
                    {
                        con.Open();

                        _cmd = server.GetCommand(sql, con);

                        DbDataAdapter a = server.GetDataAdapter(_cmd);

                        DataTable dt = new DataTable();

                        a.Fill(dt);

                        Invoke(new MethodInvoker(() => { dataGridView1.DataSource = dt; }));
                        con.Close();
                    }
                }
                catch (Exception e)
                {
                    ragSmiley1.SetVisible(true);
                    ragSmiley1.Fatal(e);
                }
                finally
                {
                    if (IsHandleCreated)
                        Invoke(new MethodInvoker(() =>
                        {
                            pbLoading.Visible = false;
                            llCancel.Visible = false;
                        }));
                }
            });
        }

        public IPersistableObjectCollection GetCollection()
        {
            return _collection;
        }

        public string GetTabName()
        {
            return _collection.GetTabName();
        }

        private void llCancel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(_cmd != null)
                _cmd.Cancel();
        }

        void _scintilla_TextChanged(object sender, EventArgs e)
        {
            //enable the reset button only if the SQL has changed (e.g. user is typing stuff)
            btnResetSql.Enabled = !_originalSql.Equals(_scintilla.Text);
            btnResetSql.Text = btnResetSql.Enabled ? "You have changed the above SQL, click to reset" : "";

        }

        private void btnExecuteSql_Click(object sender, EventArgs e)
        {
            LoadDataTableAsync(_server,_scintilla.Text);
        }

        private void btnResetSql_Click(object sender, EventArgs e)
        {
            _scintilla.Text = _originalSql;
        }
    }
}
