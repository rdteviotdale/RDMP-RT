﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Annotations;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Often an ExtractionFilter will have a parameter associated with it (or more than one).  In this case it can be that you want to curate various values and give them
    /// meaningful titles.  For exmaple if you have a filter 'Hospitalised with condition X' which has parameter @ConditionList.  Then you decide that you want to curate
    /// a list 'A101.23,B21.1' as 'People hospitalised with drug dependency'.  This 'known meaningful parameter values set' is called a ExtractionFilterParameterSet.  You 
    /// can provide a name and a description for the concept.  Then you create a value for each parameter in the associated filter.  See ExtractionFilterParameterSetValue for
    /// the value recordings.
    /// </summary>
    public class ExtractionFilterParameterSet:DatabaseEntity, ICollectSqlParameters,INamed
    {
        #region Database Properties
        private string _name;
        private string _description;
        private int _extractionFilterID;

        /// <inheritdoc/>
        [NotNull]
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name , value);}
        }
       
        /// <summary>
        /// Human readable description of what the parameter set identifies e.g. 'Diabetes Drugs' and any supporting information about how it works, quirks etc
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { SetField(ref _description , value);}
        }

        /// <summary>
        /// The filter which the parameter values are designed to work with
        /// </summary>
        public int ExtractionFilter_ID
        {
            get { return _extractionFilterID; }
            set { SetField(ref _extractionFilterID, value); }
        }

        #endregion

        #region Relationships

        /// <inheritdoc cref ="ExtractionFilter_ID"/>
        [NoMappingToDatabase]
        public ExtractionFilter ExtractionFilter { get {return Repository.GetObjectByID<ExtractionFilter>(ExtractionFilter_ID);} }
        
        /// <summary>
        /// Gets all the individual parameter values required for populating the filter to achieve this concept (e.g. 'Diabetes Drugs' might have 2 parameter values @DrugList='123.122.1,1.2... etc' and @DrugCodeFormat='bnf')
        /// </summary>
        [NoMappingToDatabase]
        public IEnumerable<ExtractionFilterParameterSetValue> Values { get {return Repository.GetAllObjectsWithParent<ExtractionFilterParameterSetValue>(this);} }

        #endregion

        internal ExtractionFilterParameterSet(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Name = r["Name"].ToString();
            Description = r["Description"] as string;
            ExtractionFilter_ID = Convert.ToInt32(r["ExtractionFilter_ID"]);
        }

        /// <summary>
        /// Defines a new set of known parameter values to achieve a given goal (e.g. identify 'diabetic drugs' in dataset prescriptions) in combination with a parent <see cref="IFilter"/>.
        /// <para>A single <see cref="ExtractionFilter"/> (e.g. 'Drug Prescriptions of X' with parameter @DrugList) could have many <see cref="ExtractionFilterParameterSet"/></para>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="filter"></param>
        /// <param name="name"></param>
        public ExtractionFilterParameterSet(ICatalogueRepository repository, ExtractionFilter filter, string name = null)
        {
            name = name ?? "New ExtractionFilterParameterSet " + Guid.NewGuid();

            repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"Name",name},
                {"ExtractionFilter_ID",filter.ID}
            });
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc cref="Values"/>
        public ISqlParameter[] GetAllParameters()
        {
            return Values.ToArray();
        }


        /// <summary>
        /// Creates new value entries for each parameter in the filter that does not yet have a value in this value set
        /// </summary>
        /// <returns></returns>
        public ExtractionFilterParameterSetValue[] CreateNewValueEntries()
        {
            List<ExtractionFilterParameterSetValue> toReturn = new List<ExtractionFilterParameterSetValue>();

            var existingMasters = ExtractionFilter.GetAllParameters().Cast<ExtractionFilterParameter>().ToArray();

            var personalChildren = Values.ToArray();

            foreach (ExtractionFilterParameter master in existingMasters)
                if (personalChildren.All(c => c.ExtractionFilterParameter_ID != master.ID))
                    //we have a master that does not have any child values yet
                     toReturn.Add(new ExtractionFilterParameterSetValue((ICatalogueRepository) Repository, this, master));

            return toReturn.ToArray();
        }
    }
}
