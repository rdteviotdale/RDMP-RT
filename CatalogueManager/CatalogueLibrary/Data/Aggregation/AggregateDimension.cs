﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// This class allows you to associate a specific extractioninformation for use in aggregate generation.  For example a dataset might have a date field AdmissionDate which you
    /// want to create an aggregate configuration (when patients were admitted) over time.  However the class also allows you to specify new SelectSQL which can change how the field
    /// is extracted e.g. you might want to change "[MyDatabase].[MyTable].[AdmissionDate]" into "YEAR([MyDatabase].[MyTable].[AdmissionDate]) as AdmissionDate" 
    /// </summary>
    public class AggregateDimension : VersionedDatabaseEntity, ISaveable, IDeleteable, IColumn,IHasDependencies
    {
        #region Database Properties

        private int _aggregateConfigurationID;
        private int _extractionInformationID;
        private string _alias;
        private string _selectSQL;
        private int _order;

        /// <summary>
        /// An <see cref="AggregateDimension"/> is a column in the SELECT, GROUP BY and ORDER BY sections of an <see cref="AggregateConfiguration"/>.  This property returns
        /// the ID of the <see cref="AggregateConfiguration"/> that this column is declared on.  
        /// </summary>
        public int AggregateConfiguration_ID
        {
            get { return _aggregateConfigurationID; }
            set { SetField(ref  _aggregateConfigurationID, value); }
        }

        /// <summary>
        /// An <see cref="AggregateDimension"/> is a column in the SELECT, GROUP BY and/or ORDER BY sections of an <see cref="AggregateConfiguration"/>.  The column must have
        /// come from an extractable column in the parent <see cref="Catalogue"/>.  The Catalogue column definition is an <see cref="ExtractionInformation"/> and documents the
        /// master SELECT Sql (which can be overriden in the current AggregateDimension) as well as what the underlying <see cref="ColumnInfo"/> / <see cref="TableInfo"/>. 
        /// 
        /// This property is the ID of the associated Catalogue master <see cref="ExtractionInformation"/>.
        /// </summary>
        public int ExtractionInformation_ID
        {
            get { return _extractionInformationID; }
            set { SetField(ref  _extractionInformationID, value); }
        }

        /// <summary>
        /// Specifies the column alias section of the SELECT statement.  When building the query (See <see cref="AggregateBuilder"/>) the Alias will be added in the SELECT section
        /// of the query generated e.g. if the Alias is 'Bob' and the SelectSQL is 'GetDate()' then the resultant line of SELECT in the query will be 'GetDate() as Bob'.
        /// </summary>
        public string Alias
        {
            get { return _alias; }
            set { SetField(ref  _alias, value); }
        }

        /// <summary>
        /// An <see cref="AggregateDimension"/> is a column in the SELECT, GROUP BY and/or ORDER BY sections of an <see cref="AggregateConfiguration"/>.  This property defines
        /// the Sql that should appear in SELECT, GROUP BY and/or ORDER BY sections of the query when it is built by the <see cref="AggregateBuilder"/>.  This will start out
        /// with the exact same string as the parent <see cref="ExtractionInformation_ID"/> but can be changed as needed e.g. wrapping in UPPER.  If you change the SelectSQL
        /// to a scalar function you should add an <see cref="Alias"/>.
        /// </summary>
        [Sql]
        public string SelectSQL
        {
            get { return _selectSQL; }
            set { SetField(ref  _selectSQL, value); }
        }

        /// <summary>
        /// An <see cref="AggregateDimension"/> is a column in the SELECT, GROUP BY and/or ORDER BY sections of an <see cref="AggregateConfiguration"/>.  The Order property determines
        /// where in the SELECT, GROUP BY and/or ORDER BY list the current <see cref="AggregateDimension"/> will appear relative to the other AggregateDimensions in the
        ///  <see cref="AggregateConfiguration"/>.
        /// </summary>
        public int Order
        {
            get { return _order; }
            set { SetField(ref  _order, value); }
        }
        #endregion

        //IExtractableColumn stuff (which references the underlying extractionInformation - does not appear in table but fetches it from the other objects table)
        private ExtractionInformation _extractionInformation;
        
        #region Relationships

        /// <inheritdoc cref="CatalogueLibrary.Data.ExtractionInformation.HashOnDataRelease"/>
        [NoMappingToDatabase]
        public bool HashOnDataRelease { get{CacheExtractionInformation(); return _extractionInformation.HashOnDataRelease; } }

        /// <inheritdoc cref="CatalogueLibrary.Data.ExtractionInformation.IsExtractionIdentifier"/>
        [NoMappingToDatabase]
        public bool IsExtractionIdentifier { get { CacheExtractionInformation(); return _extractionInformation.IsExtractionIdentifier; } }

        /// <inheritdoc cref="CatalogueLibrary.Data.ExtractionInformation.IsPrimaryKey"/>
        [NoMappingToDatabase]
        public bool IsPrimaryKey { get { CacheExtractionInformation(); return _extractionInformation.IsPrimaryKey; } }

        /// <inheritdoc cref="CatalogueLibrary.Data.ExtractionInformation.ColumnInfo"/>
        [NoMappingToDatabase]
        public ColumnInfo ColumnInfo { get { CacheExtractionInformation(); return _extractionInformation.ColumnInfo; } }

        /// <summary>
        /// An <see cref="AggregateConfiguration"/> can have a single <see cref="AggregateContinuousDateAxis"/> declared on it (if it is not functioning in a cohort identification
        /// capacity).  This property will return the axis if this AggregateDimension has one declared on it.
        /// </summary>
        /// <seealso cref="CatalogueLibrary.Data.Aggregation.AggregateContinuousDateAxis.AggregateDimension_ID"/>
        [NoMappingToDatabase]
        public AggregateContinuousDateAxis AggregateContinuousDateAxis
        {
            get
            {
                return Repository.GetAllObjectsWithParent<AggregateContinuousDateAxis>(this).SingleOrDefault();
            }
        }

        /// <inheritdoc cref="ExtractionInformation_ID"/>
        [NoMappingToDatabase]
        public ExtractionInformation ExtractionInformation {get { return Repository.GetObjectByID<ExtractionInformation>(ExtractionInformation_ID); } }

        /// <inheritdoc cref="AggregateConfiguration_ID"/>
        [NoMappingToDatabase]
        public AggregateConfiguration AggregateConfiguration { get { return Repository.GetObjectByID<AggregateConfiguration>(AggregateConfiguration_ID); } }

        #endregion

        /// <summary>
        /// Declares a new column in an <see cref="AggregateConfiguration"/> (GROUP BY query).  The new column will be based on the master Catalogue column
        /// (<see cref="ExtractionInformation"/>).
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="basedOnColumn"></param>
        /// <param name="configuration"></param>
        public AggregateDimension(ICatalogueRepository repository, ExtractionInformation basedOnColumn, AggregateConfiguration configuration)
        {
            object alias = DBNull.Value;
            if (basedOnColumn.Alias != null) alias = basedOnColumn.Alias;

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"AggregateConfiguration_ID", configuration.ID},
                {"ExtractionInformation_ID", basedOnColumn.ID},
                {"SelectSQL", basedOnColumn.SelectSQL},
                {"Alias", alias},
                {"Order", basedOnColumn.Order}
            });
        }

        internal AggregateDimension(ICatalogueRepository repository,DbDataReader r) : base(repository,r)
        {
            AggregateConfiguration_ID = int.Parse(r["AggregateConfiguration_ID"].ToString());
            ExtractionInformation_ID = int.Parse(r["ExtractionInformation_ID"].ToString());
            
            SelectSQL = r["SelectSQL"] as string;
            Alias = r["Alias"] as string;

            Order = int.Parse(r["Order"].ToString());
        }

        /// <inheritdoc/>
        public string GetRuntimeName()
        {
            return RDMPQuerySyntaxHelper.GetRuntimeName(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            try
            {
                return GetRuntimeName();
            }
            catch (Exception)
            {
                return "Unamed AggregateDimension ID " + ID;
            }
        }

        /// <inheritdoc cref="ColumnSyntaxChecker"/>
        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }

        private void CacheExtractionInformation()
        {
            if (_extractionInformation == null)
                //there is a cascade delete on the relationship between extraction informations down into dimensions that should prevent the user deleting the extraction information and it leaving an orphans defined in an aggregate.
                _extractionInformation = Repository.GetObjectByID<ExtractionInformation>(ExtractionInformation_ID);
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] {ExtractionInformation};
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return new[] { AggregateConfiguration };
        }

        
    }
}
