using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.Annotations;

namespace CatalogueManager.Rules
{
    public class BinderWithErrorProviderFactory
    {
        private readonly IActivateItems _activator;

        public BinderWithErrorProviderFactory(IActivateItems activator)
        {
            _activator = activator;
        }

        public void Bind<T>(Control c, string propertyName, T databaseObject, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode,Func<T,object> getter) where T:IMapsDirectlyToDatabaseTable
        {
            c.DataBindings.Clear();
            c.DataBindings.Add(propertyName, databaseObject, dataMember, formattingEnabled, updateMode);

            var property = databaseObject.GetType().GetProperty(dataMember);

            if (property.GetCustomAttributes(typeof (UniqueAttribute), true).Any())
                new UniqueRule<T>(_activator, databaseObject, getter, c);
            
            if (property.GetCustomAttributes(typeof(NotNullAttribute), true).Any())
                new NotNullRule<T>(_activator, databaseObject, getter, c);
        }
    }
}