using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf.Utilities;
using NUnit.Framework;

namespace DynamoCoreWpfTests.Utility
{
    public class NodeContextMenuBuilderTests : DynamoTestUIBase
    {
        /// <summary>
        /// Checks that the Build method constructs the node context menu as expected.
        /// </summary>
        [Test]
        public void TestBuildContextMenu()
        {
            // Arrange
            var dummyNode = new DummyNode();

            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(dummyNode, false);

            Assert.That(ViewModel.Model.CurrentWorkspace.Nodes.Contains(dummyNode));

            var dummyNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.Id == dummyNode.GUID);

            ContextMenu contextMenu = new ContextMenu();

            // Act
            NodeContextMenuBuilder.Build
            (
                contextMenu,
                dummyNodeViewModel,
                new OrderedDictionary()
            );

            // Assert
            List<string> menuItemNames = contextMenu.Items
                .OfType<MenuItem>()
                .Select(x => x.Header.ToString())
                .ToList();

            CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.ContextMenuDelete);
            CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.ContextMenuGroups);
            CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.NodesRunStatus);
            CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.NodeContextMenuShowLabels);
            CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.NodeContextMenuRenameNode);
            CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.NodeContextMenuHelp);

            if (dummyNodeViewModel.ShowsVisibilityToggles)
            {
                CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.NodeContextMenuShowLabels);
            }
            
            if (dummyNodeViewModel.ArgumentLacing != LacingStrategy.Disabled)
            {
                CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.ContextMenuLacing);
            }
            
            if (dummyNodeViewModel.IsInput)
            {
                CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.NodeContextMenuIsInput);
            }
            
            if (dummyNodeViewModel.IsOutput)
            {
                CollectionAssert.Contains(menuItemNames, Dynamo.Wpf.Properties.Resources.NodeContextMenuIsOutput);
            }

            int expectedSeparatorsCount = dummyNodeViewModel.IsInput || dummyNodeViewModel.IsOutput ? 2 : 1;

            List<Separator> separators = contextMenu.Items.OfType<Separator>().ToList();
            Assert.AreEqual(expectedSeparatorsCount, separators.Count);
        }

        /// <summary>
        /// Checks that the NodeContextMenuItemBuilder is able to load MenuItems which come from the
        /// NodeViewCustomization process. Builds one ContextMenu without custom items, and another with two of them.
        /// </summary>
        [Test]
        public void AddNodeViewCustomizationMenuItems()
        {
            // Arrange
            var dummyNode = new DummyNode();
            var dummyNodeForCustomization = new DummyNode();

            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(dummyNode, false);
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(dummyNodeForCustomization, false);

            var dummyNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.Id == dummyNode.GUID);

            var dummyNodeForCustomizationViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.Id == dummyNode.GUID);

            Assert.That(ViewModel.Model.CurrentWorkspace.Nodes.Contains(dummyNode));
            Assert.That(ViewModel.Model.CurrentWorkspace.Nodes.Contains(dummyNodeForCustomization));

            ContextMenu contextMenu = new ContextMenu();
            ContextMenu contextMenuForNodeViewCustomization = new ContextMenu();

            OrderedDictionary emptyDictionary = new OrderedDictionary();
            OrderedDictionary nodeViewCustomizationMenuItems = new OrderedDictionary();

            MenuItem menuItem1 = new MenuItem { Header = "TestItem1" };
            MenuItem menuItem2 = new MenuItem { Header = "TestItem2" };

            nodeViewCustomizationMenuItems.Add(menuItem1.Header, menuItem1);
            nodeViewCustomizationMenuItems.Add(menuItem2.Header, menuItem2);
            
            // Act
            NodeContextMenuBuilder.Build
            (
                contextMenu,
                dummyNodeViewModel,
                emptyDictionary
            );
            
            NodeContextMenuBuilder.Build
            (
                contextMenuForNodeViewCustomization,
                dummyNodeForCustomizationViewModel,
                nodeViewCustomizationMenuItems
            );

            // Assert
            CollectionAssert.DoesNotContain(contextMenu.Items, menuItem1);
            CollectionAssert.Contains(contextMenuForNodeViewCustomization.Items, menuItem1);
            CollectionAssert.DoesNotContain(contextMenu.Items, menuItem2);
            CollectionAssert.Contains(contextMenuForNodeViewCustomization.Items, menuItem2);
        }

        /// <summary>
        /// Tests the individual build methods that are part of the NodeContextMenuBuilder.
        /// </summary>
        [Test]
        public void TestBuildMethods()
        {
            // Act
            MenuItem buildDeleteMenuItem = NodeContextMenuBuilder.BuildDeleteMenuItem();
            MenuItem buildGroupsMenuItem = NodeContextMenuBuilder.BuildGroupsMenuItem();
            MenuItem buildPreviewMenuItem = NodeContextMenuBuilder.BuildPreviewMenuItem();
            MenuItem buildFreezeMenuItem = NodeContextMenuBuilder.BuildFreezeMenuItem();
            MenuItem buildShowLabelsMenuItem = NodeContextMenuBuilder.BuildShowLabelsMenuItem();
            MenuItem buildRenameMenuItem = NodeContextMenuBuilder.BuildRenameMenuItem();
            MenuItem buildLacingMenuItem = NodeContextMenuBuilder.BuildLacingMenuItem();
            MenuItem buildIsInputMenuItem = NodeContextMenuBuilder.BuildIsInputMenuItem();
            MenuItem buildIsOutputMenuItem = NodeContextMenuBuilder.BuildIsOutputMenuItem();
            MenuItem buildHelpMenuItem = NodeContextMenuBuilder.BuildHelpMenuItem();
            
            // Assert
            Assert.IsNotNull(buildDeleteMenuItem);
            Assert.IsNotNull(buildGroupsMenuItem);
            Assert.IsNotNull(buildPreviewMenuItem);
            Assert.IsNotNull(buildFreezeMenuItem);
            Assert.IsNotNull(buildShowLabelsMenuItem);
            Assert.IsNotNull(buildRenameMenuItem);
            Assert.IsNotNull(buildLacingMenuItem);
            Assert.IsNotNull(buildIsInputMenuItem);
            Assert.IsNotNull(buildIsOutputMenuItem);
            Assert.IsNotNull(buildHelpMenuItem);
            
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.ContextMenuDelete, buildDeleteMenuItem.Header);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.ContextMenuGroups, buildGroupsMenuItem.Header);
            Assert.AreEqual(3, buildGroupsMenuItem.Items.Count);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.NodeContextMenuPreview, buildPreviewMenuItem.Header);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.NodesRunStatus, buildFreezeMenuItem.Header);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.NodeContextMenuShowLabels, buildShowLabelsMenuItem.Header);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.NodeContextMenuRenameNode, buildRenameMenuItem.Header);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.ContextMenuLacing, buildLacingMenuItem.Header);
            Assert.AreEqual(4, buildLacingMenuItem.Items.Count);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.NodeContextMenuIsInput, buildIsInputMenuItem.Header);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.NodeContextMenuIsOutput, buildIsOutputMenuItem.Header);
            Assert.AreEqual(Dynamo.Wpf.Properties.Resources.NodeContextMenuHelp, buildHelpMenuItem.Header);
        }

        /// <summary>
        /// Tests that the AddContextMenuItem method adds the given MenuItem to the target ContextMenu.
        /// </summary>
        [Test]
        public void TestAddMenuItem()
        {
            // Arrange
            ContextMenu contextMenu = new ContextMenu();
            NodeContextMenuBuilder.ContextMenu = contextMenu;

            MenuItem menuItem1 = new MenuItem { Header = "TestItem1" };
            
            CollectionAssert.DoesNotContain(contextMenu.Items, menuItem1);

            // Act
            NodeContextMenuBuilder.AddContextMenuItem(menuItem1);

            // Assert
            CollectionAssert.Contains(contextMenu.Items, menuItem1);
        }

        /// <summary>
        /// Tests that the AddContextMenuSeparator method adds a new Separator to the target ContextMenu.
        /// </summary>
        [Test]
        public void TestAddSeparator()
        {
            // Arrange
            ContextMenu contextMenu = new ContextMenu();
            NodeContextMenuBuilder.ContextMenu = contextMenu;

            Assert.IsEmpty(contextMenu.Items);

            // Act
            NodeContextMenuBuilder.AddContextMenuSeparator();

            // Assert
            Assert.AreEqual(1, contextMenu.Items.Count);
            Assert.IsInstanceOf(typeof(Separator), contextMenu.Items[0]);
        }
    }
}
