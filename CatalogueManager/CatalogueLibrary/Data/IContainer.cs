﻿using System.Collections.Generic;
using CatalogueLibrary.Data.Aggregation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes which logical keyword to use to interspace IFilters (and sub IContainers) within an IContainer.  If you have an IContainer with only one IFilter in it then
    /// it makes no difference which FilterContainerOperation you specify.  Once an IContainer has more than one IFilter they will be seperated with the 
    /// FilterContainerOperation (AND / OR See SqlQueryBuilderHelper)
    /// </summary>
    public enum FilterContainerOperation
    {
        /// <summary>
        /// Subcontainers / filters should be separated by the AND SQL keyword
        /// </summary>
        AND,

        /// <summary>
        /// Subcontainers / filters should be separated by the OR SQL keyword
        /// </summary>
        OR
    }

    /// <summary>
    /// Interface for grouping IFilters (lines of WHERE Sql) into an AND/OR tree e.g. WHERE ('Hb is Tayside' OR 'Record is older than 5 months') AND 
    /// ('result is clinically significant').  Each subcontainer / IFilter are seperated with the Operation (See FilterContainerOperation) when building SQL
    /// (See SqlQueryBuilderHelper).
    /// </summary>
    public interface IContainer:IDeleteable,ISaveable,IMapsDirectlyToDatabaseTable
    {

        /// <summary>
        /// Defines the boolean operation (AND / OR) to separate contained lines of WHERE Sql (See <see cref="IFilter"/>).  If the container has only one IFilter
        /// then no operation is used, if there are 2+ then the resultant SQL built will be each filter's WhereSQL separated by the AND/OR.  This also applies to 
        /// subcontainers e.g. an IContainer AND with two subcontainers will have the resultant SQL from compiling the two subcontainers separated by the AND/OR of the
        /// current IContainer.
        /// </summary>
        FilterContainerOperation Operation { get; set; }

        /// <summary>
        /// Gets the parental IContainer that this IContainer is a subcontainer of (inside).  This will return null if the IContainer is a root level container or an orphan.
        /// </summary>
        /// <returns></returns>
        IContainer GetParentContainerIfAny();

        /// <summary>
        /// Gets a list of all the IContainers that are subcontainers of the this ones.
        /// </summary>
        /// <returns></returns>
        IContainer[] GetSubContainers();


        /// <summary>
        /// Gets all the <see cref="IFilter"/> (lines of Where SQL) which are in the current IContainer.
        /// </summary>
        /// <remarks>This only includes IFilters in the current IContainer, if you want to also include subcontainers then use
        ///  <see cref="GetAllFiltersIncludingInSubContainersRecursively"/></remarks>
        /// <returns></returns>
        IFilter[] GetFilters();

        /// <summary>
        /// Makes the specified IContainer into a child of this current IContainer.  This is a branch level operation so will include all subcontainers/filters of the moved
        /// IContainer moving with it.
        /// </summary>
        /// <param name="child"></param>
        void AddChild(IContainer child);

        /// <summary>
        /// Makes the specified IFilter a child of this current IContainer.  This will result in it not being a part of any previous IContainer it might have been in.
        /// </summary>
        /// <param name="filter"></param>
        void AddChild(IFilter filter);

        /// <summary>
        /// Removes the IContainer from any parent IContainer it might be inside effectively turning it into an orphan (unless it is already referenced as a root container
        /// e.g. by <see cref="AggregateConfiguration.RootFilterContainer_ID"/>).
        /// </summary>
        void MakeIntoAnOrphan();

        //ContainerHelper implements these if you are writting a sane IContainer you can instantiate the helper and use it's methods

        /// <inheritdoc cref="ContainerHelper.GetRootContainerOrSelf"/>
        IContainer GetRootContainerOrSelf();

        /// <inheritdoc cref="ContainerHelper.GetAllSubContainersRecursively"/>
        List<IContainer> GetAllSubContainersRecursively();

        /// <inheritdoc cref="ContainerHelper.GetAllFiltersIncludingInSubContainersRecursively"/>
        List<IFilter> GetAllFiltersIncludingInSubContainersRecursively();

        /// <summary>
        /// If the IContainer is not part of an orphan hierarchy then there will be a resolvable root IContainer which will be referenced by something e.g. an
        /// <see cref="AggregateConfiguration"/>.  This method returns the <see cref="Catalogue"/> which that the root object operates on.
        /// </summary>
        /// <returns></returns>
        Catalogue GetCatalogueIfAny();
    }
}
