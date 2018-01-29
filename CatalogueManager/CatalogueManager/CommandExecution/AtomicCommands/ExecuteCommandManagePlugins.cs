using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.PluginManagement;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandManagePlugins : BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandManagePlugins(IActivateItems activator) : base(activator)
        {
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Plugin,OverlayKind.Edit);
        }
        
        public override void Execute()
        {
            base.Execute();

            var f = new PluginManagementForm();
            f.RepositoryLocator = Activator.RepositoryLocator;
            f.Show();
        }
    }
}