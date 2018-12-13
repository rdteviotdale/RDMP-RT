﻿using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace CatalogueManager.ExtractionUIs.FilterUIs
{
    public class FilterGraphObjectCollection : PersistableObjectCollection
    {
        public FilterGraphObjectCollection()
        {
        }

        public FilterGraphObjectCollection(AggregateConfiguration graph, ConcreteFilter filter):this()
        {
            if (graph.IsCohortIdentificationAggregate)
                throw new ArgumentException("Graph '" + graph + "' is a Cohort Identification Aggregate, this is not allowed.  Aggregat must be a graph aggregate");
            DatabaseObjects.Add(graph);
            DatabaseObjects.Add(filter);
        }

        public AggregateConfiguration GetGraph()
        {
            return (AggregateConfiguration) DatabaseObjects.Single(o => o is AggregateConfiguration);
        }
        public IFilter GetFilter()
        {
            return (IFilter)DatabaseObjects.Single(o => o is IFilter);
        }

        public void HandleRefreshObject(RefreshObjectEventArgs e)
        {
            foreach (IMapsDirectlyToDatabaseTable o in DatabaseObjects)
                if (o.Equals(e.Object))
                    ((IRevertable) o).RevertToDatabaseState();
        }
    }
}