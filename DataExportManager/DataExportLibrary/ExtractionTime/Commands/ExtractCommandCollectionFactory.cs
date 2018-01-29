﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.UserPicks;

namespace DataExportLibrary.ExtractionTime.Commands
{ 
    /// <summary>
    /// Identifies all extractable components of a given ExtractionConfiguration (all datasets, any custom tables etc).  These are returned as an
    /// ExtractCommandCollection.  
    /// </summary>
    public class ExtractCommandCollectionFactory
    {
        public ExtractCommandCollection Create(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ExtractionConfiguration configuration)
        {
            var cohort = configuration.Cohort;
            var datasets = configuration.GetAllExtractableDataSets();
            
            var customTableCommands = cohort.GetCustomTableNames().Select(name => new ExtractCohortCustomTableCommand(configuration, cohort, name));

            var datasetBundles = datasets.Select(ds => CreateDatasetCommand(repositoryLocator,ds, configuration));

            return new ExtractCommandCollection(datasetBundles,customTableCommands);
        }

        private ExtractDatasetCommand CreateDatasetCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator,IExtractableDataSet dataset, ExtractionConfiguration configuration)
        {
            var catalogue = dataset.Catalogue;

            //get all extractable locals AND extractable globals first time then just extractable locals
            var docs = catalogue.GetAllSupportingDocuments(FetchOptions.ExtractableLocals);
            var sqls = catalogue.GetAllSupportingSQLTablesForCatalogue(FetchOptions.ExtractableLocals);
            
            //Now find all the lookups and include them into the bundle
            List<TableInfo> lookupsFound;
            List<TableInfo> normalTablesFound;
            catalogue.GetTableInfos(out normalTablesFound, out lookupsFound);

            //bundle consists of:
            var bundle = new ExtractableDatasetBundle(
                dataset,//the dataset
                docs,//all non global extractable docs (SupportingDocuments)
                sqls.Where(sql => sql.IsGlobal == false).ToArray(),//all non global extractable sql (SupportingSQL)
                lookupsFound.ToArray());//all lookups associated with the Catalogue (the one behind the ExtractableDataset)

            return new ExtractDatasetCommand(repositoryLocator,configuration, bundle);
        }
    }
}
