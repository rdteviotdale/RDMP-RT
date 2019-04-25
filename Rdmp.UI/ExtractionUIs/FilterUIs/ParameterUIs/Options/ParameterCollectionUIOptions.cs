// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Cohort;
using Rdmp.Core.CatalogueLibrary.QueryBuilding.Parameters;
using Rdmp.Core.CatalogueLibrary.Repositories;
using Rdmp.Core.CatalogueLibrary.Spontaneous;

namespace Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs.Options
{
    public delegate ISqlParameter CreateNewSqlParameterHandler(ICollectSqlParameters collector,string parameterName);

    public class ParameterCollectionUIOptions
    {
        
        public ICollectSqlParameters Collector { get; set; }
        public ParameterLevel CurrentLevel { get; set; }
        public ParameterManager ParameterManager { get; set; }
        private CreateNewSqlParameterHandler _createNewParameterDelegate;

        public readonly ParameterRefactorer Refactorer = new ParameterRefactorer();

        public string UseCase { get; private set; }

        public readonly string[]  ProhibitedParameterNames = new string[]
        {
            
"@CohortDefinitionID",
"@ProjectNumber",
"@dateAxis",
"@currentDate",
"@dbName",
"@sql",
"@isPrimaryKeyChange",
"@Query",
"@Columns",
"@value",
"@pos",
"@len",
"@startDate",
"@endDate"};

        public ParameterCollectionUIOptions(string useCase, ICollectSqlParameters collector, ParameterLevel currentLevel, ParameterManager parameterManager ,CreateNewSqlParameterHandler createNewParameterDelegate = null)
        {

            UseCase = useCase;
            Collector = collector;
            CurrentLevel = currentLevel;
            ParameterManager = parameterManager;
            _createNewParameterDelegate = createNewParameterDelegate;

            if (_createNewParameterDelegate == null)
                if (AnyTableSqlParameter.IsSupportedType(collector.GetType()))
                    _createNewParameterDelegate = CreateNewParameterDefaultImplementation;
        }


        /// <summary>
        /// Method called when creating new parameters if no CreateNewSqlParameterHandler was provided during construction
        /// </summary>
        /// <returns></returns>
        private ISqlParameter CreateNewParameterDefaultImplementation(ICollectSqlParameters collector, string parameterName)
        {
            if (!parameterName.StartsWith("@"))
                parameterName = "@" + parameterName;

            var entity = (IMapsDirectlyToDatabaseTable) collector;
            var newParam = new AnyTableSqlParameter((ICatalogueRepository)entity.Repository, entity, "DECLARE " + parameterName + " as varchar(10)");
            newParam.Value = "'todo'";
            newParam.SaveToDatabase();
            return newParam;
        }

        public bool CanNewParameters()
        {
            return _createNewParameterDelegate != null;
        }

        public ISqlParameter CreateNewParameter(string parameterName)
        {
            return _createNewParameterDelegate(Collector,parameterName);
        }
        
        public bool IsHigherLevel(ISqlParameter parameter)
        {
            return ParameterManager.GetLevelForParameter(parameter) > CurrentLevel;
        }

        private bool IsDifferentLevel(ISqlParameter p)
        {
            return ParameterManager.GetLevelForParameter(p) != CurrentLevel;
        }

        public bool IsOverridden(ISqlParameter sqlParameter)
        {
            return ParameterManager.GetOverrideIfAnyFor(sqlParameter) != null;
        }

        public bool ShouldBeDisabled(ISqlParameter p)
        {
            return IsOverridden(p) || IsHigherLevel(p) || p is SpontaneousObject;
        }

        public bool ShouldBeReadOnly(ISqlParameter p)
        {
            return IsOverridden(p) || IsDifferentLevel(p) || p is SpontaneousObject;
        }
    }
}
