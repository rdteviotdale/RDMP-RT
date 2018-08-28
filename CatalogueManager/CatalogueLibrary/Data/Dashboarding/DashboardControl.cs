﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Dashboarding
{
    /// <summary>
    /// Records position and Type of an IDashboardableControl on a DashboardLayout.  The lifecycle goes:
    /// 1. Control instance created (must have a blank constructor)
    /// 2. ConstructEmptyCollection called on instance of control
    /// 3. Step2 collection Hydrated witht he PersistenceString (which can be null/empty)
    /// 4. Step2 collection given the Objects referenced by ObjectsUsed
    /// 5. Control instance given the Hydrated collection with SetCollection method
    /// </summary>
    public class DashboardControl:DatabaseEntity
    {
        #region Database Properties
        private int _dashboardLayout_ID;
        private int _x;
        private int _y;
        private int _width;
        private int _height;
        private string _controlType;
        private string _persistenceString;

        /// <summary>
        /// Records which <see cref="DashboardLayout"/> the control exists on
        /// </summary>
        public int DashboardLayout_ID
        {
            get { return _dashboardLayout_ID; }
            set { SetField(ref _dashboardLayout_ID, value); }
        }

        /// <summary>
        /// The X Coordinate of the control within the <see cref="DashboardLayout"/> window
        /// </summary>
        public int X
        {
            get { return _x; }
            set { SetField(ref _x, value); }
        }

        /// <summary>
        /// The Y Coordinate of the control within the <see cref="DashboardLayout"/> window
        /// </summary>
        public int Y
        {
            get { return _y; }
            set { SetField(ref _y, value); }
        }

        /// <summary>
        /// The Width of the control within the <see cref="DashboardLayout"/> window
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { SetField(ref _width, value); }
        }

        /// <summary>
        /// The Height of the control within the <see cref="DashboardLayout"/> window
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { SetField(ref _height, value); }
        }

        /// <summary>
        /// The C# Class name of an IDashboardableControl which this class documents the existence of
        /// </summary>
        public string ControlType
        {
            get { return _controlType; }
            set { SetField(ref _controlType, value); }
        }

        /// <summary>
        /// Serialized settings as configured by the user for the IDashboardableControl referenced by <see cref="ControlType"/>
        /// </summary>
        public string PersistenceString
        {
            get { return _persistenceString; }
            set { SetField(ref _persistenceString, value); }
        }

        #endregion

        #region Relationships

        /// <summary>
        /// Gets all <see cref="IMapsDirectlyToDatabaseTable"/> objects used by the IDashboardableControl.  E.g. if the control is a pie chart of which columns in a dataset
        /// are missing column descriptions then this will return the <see cref="ICatalogue"/> which represents that dataset
        /// </summary>
        [NoMappingToDatabase]
        public DashboardObjectUse[] ObjectsUsed{ get { return Repository.GetAllObjectsWithParent<DashboardObjectUse>(this); }}

        /// <inheritdoc cref="DashboardLayout_ID"/>
        [NoMappingToDatabase]
        public DashboardLayout ParentLayout { get { return Repository.GetObjectByID<DashboardLayout>(DashboardLayout_ID); } }
        #endregion

        internal DashboardControl(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            DashboardLayout_ID = Convert.ToInt32(r["DashboardLayout_ID"]);
            X = Convert.ToInt32(r["X"]);
            Y = Convert.ToInt32(r["Y"]);
            Height = Convert.ToInt32(r["Height"]);
            Width = Convert.ToInt32(r["Width"]);

            ControlType = r["ControlType"].ToString();//cannot be null
            PersistenceString = r["PersistenceString"] as string;//can be null
        }

        /// <summary>
        /// Adds a new IDashboardableControl (<see cref="controlType"/>) to the given <see cref="DashboardLayout"/> (<see cref="parent"/>) at the specified position.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parent"></param>
        /// <param name="controlType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="persistenceString"></param>
        public DashboardControl(ICatalogueRepository repository, DashboardLayout parent, Type controlType, int x, int y, int w, int h, string persistenceString)
        {
            Repository = repository;

            Repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"DashboardLayout_ID",parent.ID},
                {"X",x},
                {"Y",y},
                {"Width",w},
                {"Height",h},
                {"ControlType",controlType.Name},
                {"PersistenceString",persistenceString}
            });   
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ControlType + "( " + ID + " )";
        }

        /// <summary>
        /// Serializes the current state settings of the IDashboardableControl into <see cref="PersistenceString"/>
        /// </summary>
        /// <param name="collection"></param>
        public void SaveCollectionState(IPersistableObjectCollection collection)
        {
            //save ourselves
            PersistenceString = collection.SaveExtraText();
            SaveToDatabase();

            //save our objects
            foreach (var o in ObjectsUsed)
                o.DeleteInDatabase();

            foreach (IMapsDirectlyToDatabaseTable objectToSave in collection.DatabaseObjects)
                new DashboardObjectUse((ICatalogueRepository) Repository, this, objectToSave);
        }
    }
}
