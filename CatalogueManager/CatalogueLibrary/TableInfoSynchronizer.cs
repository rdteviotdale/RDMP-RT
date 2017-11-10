﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing.Text;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;

namespace CatalogueLibrary
{
    public class TableInfoSynchronizer
    {
        private readonly TableInfo _tableToSync;
        private DiscoveredServer _toSyncTo;
        private CatalogueRepository _repository;

        /// <summary>
        /// Synchronizes the TableInfo against the underlying database to ensure the Catalogues understanding of what columns exist, what are primary keys,
        /// collation types etc match the reality.  Pass in an alternative 
        /// </summary>
        /// <param name="tableToSync"></param>
        public TableInfoSynchronizer(TableInfo tableToSync)
        {
            _tableToSync = tableToSync;
            _repository = (CatalogueRepository)_tableToSync.Repository;

            _toSyncTo = DataAccessPortal.GetInstance().ExpectServer(tableToSync, DataAccessContext.InternalDataProcessing);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="FatalDeSyncException">Could not figure out how to resolve a synchronization problem between the TableInfo and the underlying table structure</exception>
        /// <param name="notifier">Called every time a fixable problem is detected, method must return true or false.  True = apply fix, False = don't - but carry on checking</param>
        public bool Synchronize(ICheckNotifier notifier)
        {
            bool IsSynched = true;

            //server exists and is accessible?
            try
            {
                _toSyncTo.TestConnection();
            }
            catch (Exception e)
            {
                throw new FatalDeSyncException("Could not connect to " + _toSyncTo, e);
            }

            //database exists?
            var expectedDatabase = _toSyncTo.ExpectDatabase(_tableToSync.GetDatabaseRuntimeName());
            if(!expectedDatabase.Exists())
                throw new FatalDeSyncException("Server did not contain a database called " + _tableToSync.GetDatabaseRuntimeName());

            //identify new columns
            DiscoveredColumn[] liveColumns;
            DiscoveredTable expectedTable;

            if (_tableToSync.IsTableValuedFunction)
            {
                expectedTable = expectedDatabase.ExpectTableValuedFunction(_tableToSync.GetRuntimeName());
                if(!expectedTable.Exists())
                    throw new FatalDeSyncException("Database " + expectedDatabase + " did not contain a TABLE VALUED FUNCTION called " + _tableToSync.GetRuntimeName());
            }
            else
            {
                //table exists?
                expectedTable = expectedDatabase.ExpectTable(_tableToSync.GetRuntimeName());
                if(!expectedTable.Exists())
                    throw new FatalDeSyncException("Database " + expectedDatabase + " did not contain a table called " + _tableToSync.GetRuntimeName());
            }

            try
            {
                liveColumns = expectedTable.DiscoverColumns();
            }
            catch (SqlException e)
            {
                throw new Exception("Failed to enumerate columns in " +
                    _toSyncTo + 
                    " (we were attempting to synchronize the TableInfo " + _tableToSync + " (ID=" + _tableToSync.ID + ").  Check the inner exception for specifics", e);
            }

            ColumnInfo[] catalogueColumns = _tableToSync.ColumnInfos.ToArray();


            DataAccessCredentials credentialsIfExists = _tableToSync.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
            string pwd = null;
            string usr = null;
            if (credentialsIfExists != null)
            {
                usr = credentialsIfExists.Username;
                pwd = credentialsIfExists.GetDecryptedPassword();
            }

            ITableInfoImporter importer;

            //for importing new stuff
            if (_tableToSync.IsTableValuedFunction)
                importer = new TableValuedFunctionImporter(_repository, (DiscoveredTableValuedFunction) expectedTable);
            else
                importer = new TableInfoImporter(_repository, _toSyncTo.Name, _toSyncTo.GetCurrentDatabase().GetRuntimeName(), _tableToSync.GetRuntimeName(), _tableToSync.DatabaseType, username: usr, password: pwd);

            DiscoveredColumn[] newColumnsInLive = 
                liveColumns.Where(
                    live => !catalogueColumns.Any(columnInfo => 
                        columnInfo.GetRuntimeName()
                        .Equals(live.GetRuntimeName()))).ToArray();

            //there are new columns in the live database that are not in the Catalogue
            if(newColumnsInLive.Any())
            {
                //see if user wants to add missing columns
                bool addMissingColumns = notifier.OnCheckPerformed(new CheckEventArgs("The following columns are missing from the TableInfo:" +string.Join(",",newColumnsInLive.Select(c=>c.GetRuntimeName())),CheckResult.Fail,null,"The ColumnInfos will be created and added to the TableInfo"));
                
                List<ColumnInfo> added = new List<ColumnInfo>();

                if(addMissingColumns)
                {
                    foreach (DiscoveredColumn missingColumn in newColumnsInLive)
                        added.Add(importer.CreateNewColumnInfo(_tableToSync, missingColumn));

                    ForwardEngineerExtractionInformationIfAppropriate(added,notifier);
                }
                else
                    IsSynched = false;
            }

            //See if we need to delete any ColumnInfos
            ColumnInfo[] columnsInCatalogueButSinceDisapeared = 
                catalogueColumns
                    .Where(columnInfo => !liveColumns.Any( //there are not any
                        c=>columnInfo.GetRuntimeName().Equals(c.GetRuntimeName())) //columns with the same name between discovery/columninfo
                        ).ToArray();

            if (columnsInCatalogueButSinceDisapeared.Any())
            {
                foreach (var columnInfo in columnsInCatalogueButSinceDisapeared)
                {
                    
                    bool deleteExtraColumnInfos = notifier.OnCheckPerformed(new CheckEventArgs("The ColumnInfo " +columnInfo.GetRuntimeName() + " no longer appears in the live table." ,CheckResult.Fail,null,"Delete ColumnInfo " + columnInfo.GetRuntimeName()));
                    if (deleteExtraColumnInfos)
                        columnInfo.DeleteInDatabase();
                    else
                        IsSynched = false;
                }
            }

            if (IsSynched)
                IsSynched = SynchronizeTypes(notifier,liveColumns);

            if (IsSynched && !_tableToSync.IsTableValuedFunction)//table valued functions don't have primary keys!
                IsSynched = SynchronizePrimaryKeys(notifier);

            if (IsSynched && _tableToSync.IsTableValuedFunction)
                IsSynched = SynchronizeParameters((TableValuedFunctionImporter)importer,notifier);

            //get list of primary keys from underlying table
            return IsSynched;
        }

        private void ForwardEngineerExtractionInformationIfAppropriate(List<ColumnInfo> added, ICheckNotifier notifier)
        {
            //Is there one Catalogue behind this dataset?
            var relatedCatalogues = _tableToSync.GetAllRelatedCatalogues();

            //if there is only one catalogue powered by this TableInfo
            if (relatedCatalogues.Length == 1)
                //And there are ExtractionInformations already for ColumnInfos in this _tableToSync
                if (relatedCatalogues[0].GetAllExtractionInformation(ExtractionCategory.Any).Any(e => e.ColumnInfo != null && e.ColumnInfo.TableInfo_ID == _tableToSync.ID))
                    //And user wants to create new ExtractionInformations for the newly created sync'd ColumnInfos
                    if (notifier.OnCheckPerformed(
                        new CheckEventArgs("Would you also like to make these columns Extractable in Catalogue " + relatedCatalogues[0].Name + "?", CheckResult.Warning, null,
                        "Also make columns Extractable?")))
                    {
                        //Create CatalogueItems for the new columns
                        ForwardEngineerCatalogue c = new ForwardEngineerCatalogue(_tableToSync, added.ToArray(), true);

                        //In the Catalogue
                        c.ExecuteForwardEngineering(relatedCatalogues[0]);
                    }
        }

        private bool SynchronizeTypes(ICheckNotifier notifier, DiscoveredColumn[] liveColumns)
        {
            bool IsSynched = true;
            
            foreach (var columnInfo in _tableToSync.ColumnInfos)
            {
                var liveState = liveColumns.Single(c => c.GetRuntimeName().Equals(columnInfo.GetRuntimeName()));
                
                //deal with mismatch in type
                if (!liveState.DataType.SQLType.Equals(columnInfo.Data_type))
                    if (notifier.OnCheckPerformed(new CheckEventArgs(
                        "ColumnInfo " + columnInfo.GetRuntimeName() + " is type " + liveState.DataType.SQLType +
                        " in the live database but in the Catalogue appears as " + columnInfo.Data_type,CheckResult.Fail,null,
                        "Update type in Catalogue?")))
                    {
                        columnInfo.Data_type = liveState.DataType.SQLType;
                        columnInfo.SaveToDatabase();
                    }
                    else
                        IsSynched = false;

                //if column has collation and live collation is not matching the one in the catalogue
                if (!string.IsNullOrWhiteSpace(liveState.Format) && !liveState.Format.Equals(columnInfo.Format))
                    if (
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "Mismatch between format in live of " + liveState.Format + " and Catalogue entry " +
                            columnInfo.Format,CheckResult.Fail,null, "Fix collation on ColumnInfo record to match live")))
                    {
                        columnInfo.Format = liveState.Format;
                        columnInfo.SaveToDatabase();
                    }
                    else
                        IsSynched = false;
            }

            return IsSynched;

        }

        private bool SynchronizePrimaryKeys( ICheckNotifier notifier)
        {
            bool IsSynched = true;

            ColumnInfo[] columnsInCatalogue = _tableToSync.ColumnInfos.ToArray();

            DiscoveredColumn[] pksInLive = _toSyncTo.GetCurrentDatabase().ExpectTable(_tableToSync.GetRuntimeName()).DiscoverColumns()
                .Where(
                c=>c.IsPrimaryKey
                ).ToArray();

            //if there are live pks that are not know to the catalogue
            foreach (string key in pksInLive.Select(pk=>pk.GetRuntimeName()))
            {
                //find the corresponding ColumnInfo
                ColumnInfo matchingColumnInfo = columnsInCatalogue.Single(ci=>ci.GetRuntimeName().Equals(key));

                //if it is not currently already flagged as pk
                if(!matchingColumnInfo.IsPrimaryKey)
                    if (
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "Field in table " + _tableToSync + " has a primary key including " + key +
                            "(that the Catalogue does not know about)",CheckResult.Fail,null, "Mark ColumnInfo " + key+ " as IsPrimaryKey=1")))
                        //see if the user wants to flag it as one
                    {
                        matchingColumnInfo.IsPrimaryKey = true; //he does
                        matchingColumnInfo.SaveToDatabase();
                    }
                    else
                        IsSynched = false;
            }

            //get columnInfos that are flaggaed as primary key but are not in the live primary key collection (Catalogue thinks they are pk but they aint)
            ColumnInfo[] redundantPrimaryKeysInCatalogue 
                = columnsInCatalogue.Where(ci=>ci.IsPrimaryKey //that are pk in catalogue
                    && 
                    !pksInLive.Any(pk=>pk.GetRuntimeName().Equals(ci.GetRuntimeName())) //and there are not any pks in the actual table that have the same runtime names
                    ).ToArray();

            foreach (ColumnInfo columnInfo in redundantPrimaryKeysInCatalogue)
            {
                if (
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "ColumnInfo " + columnInfo.GetRuntimeName() +
                        " is marked as primary key but the underlying table does not indicate it as a primary key ",CheckResult.Fail,null,
                        " Set "+columnInfo.GetRuntimeName()+" IsPrimaryKey=0")))
                {

                    columnInfo.IsPrimaryKey = false;
                    columnInfo.SaveToDatabase();
                }
                else
                {
                    IsSynched = false;
                }
            }
            return IsSynched;
        }

        
        private bool SynchronizeParameters(TableValuedFunctionImporter importer, ICheckNotifier notifier)
        {
            var discoveredParameters = _toSyncTo.GetCurrentDatabase().ExpectTableValuedFunction(_tableToSync.GetRuntimeName()).DiscoverParameters();
            var currentParameters = _tableToSync.GetAllParameters();
            
            //For each parameter in underlying database
            foreach (DiscoveredParameter parameter in discoveredParameters)
            {
               ISqlParameter existingCatalogueReference = currentParameters.SingleOrDefault(p => p.ParameterName.Equals(parameter.ParameterName));
                if (existingCatalogueReference == null)// that is not known about by the TableInfo
                {
                    bool create = notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "TableInfo " + _tableToSync +
                            " is a Table Valued Function but it does not have a record of the parameter " +
                            parameter.ParameterName + " which appears in the underlying database", CheckResult.Fail,
                            null, "Create the Parameter"));

                    if (!create)
                        return false; //no longer synched

                    importer.CreateParameter(_tableToSync, parameter);
                }
                else
                {
                    //it is known about by the Catalogue but has it mysteriously changed datatype since it was imported / last synced?

                    var dbDefinition = importer.GetParamaterDeclarationSQL(parameter);
                    //if there is a disagreement on type etc
                    if (existingCatalogueReference.ParameterSQL != dbDefinition)
                    {
                        bool modify =
                            notifier.OnCheckPerformed(
                                new CheckEventArgs(
                                    "Parameter " + existingCatalogueReference + " is declared as '" + dbDefinition +
                                    "' but in the Catalogue it appears as '" +
                                    existingCatalogueReference.ParameterSQL +"'", CheckResult.Fail, null,
                                    "Change the definition in the Catalogue to '" + dbDefinition + "'"));

                        if (!modify)
                            return false;

                        existingCatalogueReference.ParameterSQL = dbDefinition;
                        existingCatalogueReference.SaveToDatabase();
                    }
                }   
            }

            //Find redundant parameters - parameters that the catalogue knows about but no longer appear in the table valued function signature in the database
            foreach (ISqlParameter currentParameter in currentParameters)
                if (!discoveredParameters.Any(p => p.ParameterName.Equals(currentParameter.ParameterName)))
                {
                    bool delete =
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "TableInfo " + _tableToSync +
                                " is a Table Valued Function, in the Catalogue it has a parameter called " +
                                currentParameter.ParameterName +
                                " but this parameter no longer appears in the underlying database", CheckResult.Fail,
                                null, "Delete Parameter " + currentParameter.ParameterName));

                    if (!delete)
                        return false;

                    ((IDeleteable)currentParameter).DeleteInDatabase();
                    
                }

            return true;
        }

    }
}
