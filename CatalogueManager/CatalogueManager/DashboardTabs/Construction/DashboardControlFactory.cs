﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.DashboardTabs.Construction.Exceptions;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;

namespace CatalogueManager.DashboardTabs.Construction
{
    public class DashboardControlFactory
    {
        private readonly IActivateItems _activator;
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly Point _startLocationForNewControls;

        public DashboardControlFactory(IActivateItems activator, IRDMPPlatformRepositoryServiceLocator repositoryLocator,Point startLocationForNewControls)
        {
            _activator = activator;
            _repositoryLocator = repositoryLocator;
            _startLocationForNewControls = startLocationForNewControls;
        }

        public Type[] GetAvailableControlTypes()
        {
            List<Exception> whoCares;
            return _repositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out whoCares)
                .Where(IsCompatibleType).ToArray();
        }

        private bool IsCompatibleType(Type arg)
        {
            return 
                typeof (IDashboardableControl).IsAssignableFrom(arg)
                &&
                typeof(UserControl).IsAssignableFrom(arg);
        }

        /// <summary>
        /// Creates an instance of the user control described by the database record DashboardControl, including providing the control with a hydrated IPersistableObjectCollection that reflects
        /// the last saved state of the control.  Then mounts it on a DashboardableControlHostPanel and returns it
        /// </summary>
        /// <param name="toCreate"></param>
        /// <returns></returns>
        public DashboardableControlHostPanel Create(DashboardControl toCreate)
        {
            var controlType = _repositoryLocator.CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(toCreate.ControlType);

            var instance = CreateControl(controlType);
            
            return Hydrate((IDashboardableControl)instance, toCreate);
        }

        /// <summary>
        /// Creates a new instance of Type t (which must be an IDashboardableControl derrived ultimately from UserControl) which is then hydrated with an empty collection and a database
        /// record is created which can be used to save it's collection state for the lifetime of the control (allowing you to restore the state later)
        /// </summary>
        /// <param name="forLayout"></param>
        /// <param name="t"></param>
        /// <param name="theControlCreated"></param>
        /// <returns></returns>
        public DashboardControl Create(DashboardLayout forLayout, Type t, out DashboardableControlHostPanel theControlCreated)
        {
            var instance = CreateControl(t);
            
            //get the default size requirements of the control as it exists post construction
            int w = instance.Width;
            int h = instance.Height;

            var dbRecord = new DashboardControl(_repositoryLocator.CatalogueRepository, forLayout, t, _startLocationForNewControls.X, _startLocationForNewControls.Y, w, h, "");
            theControlCreated = Hydrate((IDashboardableControl) instance, dbRecord);

            return dbRecord;
        }

        private DashboardableControlHostPanel Hydrate(IDashboardableControl theControlCreated, DashboardControl dbRecord)
        {
            var emptyCollection = theControlCreated.ConstructEmptyCollection(dbRecord);
            
            foreach (DashboardObjectUse objectUse in dbRecord.ObjectsUsed)
            {
                var o =_repositoryLocator.GetArbitraryDatabaseObject(objectUse.RepositoryTypeName, objectUse.TypeName,objectUse.ObjectID);
                emptyCollection.DatabaseObjects.Add(o);
            }
            try
            {
                emptyCollection.LoadExtraText(dbRecord.PersistenceString);
            }
            catch (Exception e)
            {
                throw new DashboardControlHydrationException("Could not resolve extra text persistence string for control '"+ theControlCreated.GetType() +"'",e);
            }

            theControlCreated.SetCollection(_activator,emptyCollection);

            var host = new DashboardableControlHostPanel(_activator, dbRecord, theControlCreated);

            host.Location = new Point(dbRecord.X, dbRecord.Y);
            host.Width = dbRecord.Width;
            host.Height = dbRecord.Height;

            return host;
        }

        private UserControl CreateControl(Type t)
        {
            if (!IsCompatibleType(t))
                throw new ArgumentException("Type '" + t + "' is not a compatible Type", "t");

            var constructor = new ObjectConstructor();
            var instance = (UserControl)constructor.Construct(t);

            instance.Dock = DockStyle.None;
            instance.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            
            return instance;
        }

    }
}
