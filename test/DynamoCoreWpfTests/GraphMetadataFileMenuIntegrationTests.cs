using Dynamo.Wpf.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Windows.Controls;

namespace DynamoCoreWpfTests
{
    public partial class GraphMetadataFileMenuIntegrationTests : DynamoTestUIBase
    {
        [Test]
        public void FileMenuContainsGraphMetadataSubmenu()
        {
            // Arrange
            var fileMenu = View.FindName("fileMenu") as MenuItem;
            Assert.NotNull(fileMenu, "fileMenu was not found in the view.");

            var headerShowProps = Dynamo.Wpf.Properties.Resources.DynamoViewFileMenuShowGraphProperties;
            var headerGeneral = Dynamo.Wpf.Properties.Resources.DynamoViewFileMenuGraphPropertiesGeneral;

            // Act
            var graphPropsItem = fileMenu.Items.OfType<MenuItem>().FirstOrDefault(mi => (mi.Header as string) == headerShowProps);
            Assert.NotNull(graphPropsItem, "Show Graph Properties submenu not found.");

            var generalItem = graphPropsItem.Items.OfType<MenuItem>().FirstOrDefault(mi => (mi.Header as string) == headerGeneral);
            Assert.NotNull(generalItem, "General sub-item not found under Show Graph Properties.");

            // Positioning check: appears after Import Library
            var importLibrary = View.FindName("importLibrary") as MenuItem;
            Assert.NotNull(importLibrary, "importLibrary menu item not found.");

            var idxImport = fileMenu.Items.IndexOf(importLibrary);
            var idxGraph = fileMenu.Items.IndexOf(graphPropsItem);
            Assert.Greater(idxGraph, idxImport, "Show Graph Properties should appear after Import Library");
        }

        [Test]
        public void ShowGraphPropertiesCommandTogglesGraphMetadataMenuItem()
        {
            // The command should be enabled in a Home workspace:
            Assert.IsTrue(ViewModel.ShowGraphPropertiesCommand.CanExecute(null));

            // Execute it (this raises the event on the VM, and DynamoView handles it).
            ViewModel.ShowGraphPropertiesCommand.Execute(null);

            // Locate the right extension by UniqueId and check its MenuItem state:
            const string GraphMetadataExtensionId = "28992e1d-abb9-417f-8b1b-05e053bee670";

            var provider = View.viewExtensionManager.ViewExtensions
                .OfType<IExtensionMenuProvider>()
                .FirstOrDefault(ext => (ext as IViewExtension)?.UniqueId == GraphMetadataExtensionId);

            Assert.NotNull(provider, "Graph Metadata extension not found.");

            var menuItem = provider.GetFileMenuItem();
            Assert.NotNull(menuItem, "Graph Metadata menu item not provided.");
            Assert.IsTrue(menuItem.IsChecked, "Menu item should be checked after command executes.");
        }

        [Test]
        public void ShowGraphPropertiesCommandIsEnabledOnlyInHomeWorkspace()
        {
            // Home workspace is default.
            Assert.IsTrue(ViewModel.ShowGraphPropertiesCommand.CanExecute(null), "Command should be enabled in Home workspace.");

            // Open a custom node and switch to it.
            Open(@"core\CustomNodes\add.dyf");

            ViewModel.UIDispatcher.Invoke(new Action(() =>
            {
                // Make the last workspace (the custom node) current.
                var customWs = ViewModel.Model.Workspaces.Last();
                ViewModel.Model.CurrentWorkspace = customWs;
            }));

            Assert.IsFalse(ViewModel.ShowGraphPropertiesCommand.CanExecute(null), "Command should be disabled in Custom Node.");
        }
    }
}
