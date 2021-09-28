using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// A static class containing methods for dynamically building a Node's Context Menu.
    /// </summary>
    internal static class NodeContextMenuBuilder
    {
        /// <summary>
        /// A reference to the NodeViewModel whose ContextMenu is being built.
        /// </summary>
        private static NodeViewModel NodeViewModel { get; set; }

        /// <summary>
        /// A reference to the ContextMenu to which items are being added. 
        /// </summary>
        private static ContextMenu ContextMenu { get; set; }

        // Builds the node's Context Menu, including re-adding any injected MenuItems from
        // the NodeViewCustomization process.
        internal static void Build(ContextMenu contextMenu, NodeViewModel nodeViewModel, Dictionary<string, object> nodeViewCustomizationMenuItems)
        {
            ContextMenu = contextMenu;
            NodeViewModel = nodeViewModel;
            
            AddContextMenuItem(BuildDeleteMenuItem());
            AddContextMenuItem(BuildGroupsMenuItem());
            if (nodeViewModel.ShowsVisibilityToggles) AddContextMenuItem(BuildPreviewMenuItem());
            AddContextMenuItem(BuildFreezeMenuItem());
            AddContextMenuItem(BuildShowLabelsMenuItem());
            AddContextMenuItem(BuildRenameMenuItem());
            if (nodeViewModel.ArgumentLacing != LacingStrategy.Disabled) AddContextMenuItem(BuildLacingMenuItem());
            
            //AddContextMenuItem(BuildDismissedAlertsMenuItem());
            
            if (nodeViewCustomizationMenuItems.Count > 0)
            {
                AddContextMenuSeparator();
                AddInjectedNodeViewCustomizationMenuItems(nodeViewCustomizationMenuItems);
            }
            
            if(NodeViewModel.IsInput || NodeViewModel.IsOutput) AddContextMenuSeparator();
            if(NodeViewModel.IsInput) AddContextMenuItem(BuildIsInputMenuItem());
            if(NodeViewModel.IsOutput) AddContextMenuItem(BuildIsOutputMenuItem());

            AddContextMenuSeparator();
            AddContextMenuItem(BuildHelpMenuItem());
        }
        
        /// <summary>
        /// Adds items to a context menu.
        /// </summary>
        /// <param name="menuItem"></param>
        private static void AddContextMenuItem(MenuItem menuItem) => ContextMenu.Items.Add(menuItem);

        /// <summary>
        /// Adds a new separator a context menu.
        /// </summary>
        private static void AddContextMenuSeparator() => ContextMenu.Items.Add(new Separator());

        /// <summary>
        /// Creates a new MenuItem in the node's Context Menu.
        /// Using optional named arguments here as it makes the Binding syntax
        /// much cleaner if all handled in the method body.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="header"></param>
        /// <param name="command"></param>
        /// <param name="commandParameter"></param>
        /// <param name="isCheckable"></param>
        /// <param name="isChecked"></param>
        /// <param name="visibility"></param>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        private static MenuItem CreateMenuItem
        (
            string name = null,
            string header = null,
            Binding command = null,
            string commandParameter = null,
            bool isCheckable = false,
            Binding isChecked = null,
            Binding visibility = null,
            Binding isEnabled = null,
            Binding itemsSource = null
        )
        {
            MenuItem menuItem = new MenuItem { Header = header, IsCheckable = isCheckable };

            if (!string.IsNullOrWhiteSpace(name)) menuItem.Name = name;
            if (command != null) menuItem.SetBinding(MenuItem.CommandProperty, command);
            if (commandParameter != null) menuItem.CommandParameter = commandParameter;
            if (isChecked != null) menuItem.SetBinding(MenuItem.IsCheckedProperty, isChecked);
            if (visibility != null) menuItem.SetBinding(UIElement.VisibilityProperty, visibility);
            if (isEnabled != null) menuItem.SetBinding(UIElement.IsEnabledProperty, isEnabled);
            if (itemsSource != null) menuItem.SetBinding(ItemsControl.ItemsSourceProperty, itemsSource);

            return menuItem;
        }

        private static MenuItem BuildDeleteMenuItem()
        {
            return CreateMenuItem
            (
                name: "deleteElem_cm",
                header: Properties.Resources.ContextMenuDelete,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("DeleteCommand")
                }
            );
        }

        private static MenuItem BuildGroupsMenuItem()
        {
            MenuItem groupsMenuItem = CreateMenuItem(header: Properties.Resources.ContextMenuGroups);

            groupsMenuItem.Items.Add(CreateMenuItem
                (
                    name: "createGroup_cm",
                    header: Properties.Resources.ContextCreateGroupFromSelection,
                    command: new Binding
                    {
                        Source = NodeViewModel,
                        Path = new PropertyPath("CreateGroupCommand")
                    }
                )
            );
            groupsMenuItem.Items.Add(CreateMenuItem
                (
                    name: "unGroup_cm",
                    header: Properties.Resources.ContextUnGroupFromSelection,
                    command: new Binding
                    {
                        Source = NodeViewModel,
                        Path = new PropertyPath("UngroupCommand")
                    }
                )
            );
            groupsMenuItem.Items.Add(CreateMenuItem
                (
                    name: "addtoGroup",
                    header: Properties.Resources.ContextAddGroupFromSelection,
                    command: new Binding
                    {
                        Source = NodeViewModel,
                        Path = new PropertyPath("AddToGroupCommand")
                    }
                )
            );

            return groupsMenuItem;
        }

        private static MenuItem BuildPreviewMenuItem()
        {
            MenuItem previewMenuItem = CreateMenuItem
            (
                name: "isVisible_cm",
                header: Properties.Resources.NodeContextMenuPreview,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("ToggleIsVisibleCommand")
                },
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("IsVisible"),
                    Mode = BindingMode.OneWay
                }
            );

            return previewMenuItem;
        }

        private static MenuItem BuildFreezeMenuItem()
        {
            MenuItem freezeMenuItem = CreateMenuItem
            (
                name: "nodeIsFrozen",
                header: Properties.Resources.NodesRunStatus,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("ToggleIsFrozenCommand")
                },
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("IsFrozenExplicitly"),
                    Mode = BindingMode.OneWay
                },
                isEnabled: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("CanToggleFrozen"),
                    Mode = BindingMode.OneWay
                }
            );

            return freezeMenuItem;
        }

        private static MenuItem BuildShowLabelsMenuItem()
        {
            MenuItem showLabelsMenuItem = CreateMenuItem
            (
                name: "isDisplayLabelsEnabled_cm",
                header: Properties.Resources.NodeContextMenuShowLabels,
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("IsDisplayingLabels"),
                    Mode = BindingMode.TwoWay
                },
                isEnabled: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("CanDisplayLabels")
                }
            );

            return showLabelsMenuItem;
        }

        private static MenuItem BuildRenameMenuItem()
        {
            MenuItem renameMenuItem = CreateMenuItem
            (
                name: "rename_cm",
                header: Properties.Resources.NodeContextMenuRenameNode,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("RenameCommand")
                }
            );
            return renameMenuItem;
        }

        private static MenuItem BuildLacingMenuItem()
        {
            MenuItem lacingMenuItem = CreateMenuItem(header: Properties.Resources.ContextMenuLacing);

            lacingMenuItem.Items.Add(CreateMenuItem
            (
                header: Properties.Resources.ContextMenuLacingAuto,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("SetLacingTypeCommand"),
                },
                commandParameter: "Auto",
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("ArgumentLacing"),
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = "Auto",
                    Mode = BindingMode.OneWay
                }
            ));

            lacingMenuItem.Items.Add(CreateMenuItem
            (
                header: Properties.Resources.ContextMenuLacingShortest,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("SetLacingTypeCommand"),
                },
                commandParameter: "Shortest",
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("ArgumentLacing"),
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = "Shortest",
                    Mode = BindingMode.OneWay
                }
                ));

            lacingMenuItem.Items.Add(CreateMenuItem
            (
                header: Properties.Resources.ContextMenuLacingLongest,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("SetLacingTypeCommand"),
                },
                commandParameter: "Longest",
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("ArgumentLacing"),
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = "Longest",
                    Mode = BindingMode.OneWay
                }
            ));

            lacingMenuItem.Items.Add(CreateMenuItem
            (
                header: Properties.Resources.ContextMenuLacingCrossProduct,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("SetLacingTypeCommand"),
                },
                commandParameter: "CrossProduct",
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("ArgumentLacing"),
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = "CrossProduct",
                    Mode = BindingMode.OneWay
                }
            ));

            return lacingMenuItem;
        }

        // To be connected in a future PR for Node Info States.
        private static MenuItem BuildDismissedAlertsMenuItem()
        {
            //MenuItem dismissedAlertsMenuItem = CreateMenuItem
            //(
            //    name: "dismissedAlerts",
            //    header: Wpf.Properties.Resources.NodeInformationalStateDismissedAlerts,
            //    itemsSource: new Binding
            //    {
            //        Source = viewModel,
            //        Path = new PropertyPath("DismissedAlerts"),
            //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            //    }
            //);
            //DataTemplate itemTemplate = new DataTemplate(typeof(MenuItem));

            //FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
            //grid.Name = "mainTemplateGrid";
            //grid.SetValue(WidthProperty, 220.0);
            //grid.SetValue(HeightProperty, 30.0);
            //grid.SetValue(MarginProperty, new Thickness(-15,0,0,0));
            //grid.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Left);
            //grid.SetValue(VerticalAlignmentProperty, VerticalAlignment.Stretch);
            //grid.SetValue(BackgroundProperty, new SolidColorBrush(Colors.Transparent));

            //FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
            //textBlock.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            //textBlock.SetValue(MarginProperty, new Thickness(15, 0, 0, 0));
            //textBlock.SetValue(IsHitTestVisibleProperty, false);
            //textBlock.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            //textBlock.SetValue(ForegroundProperty, new SolidColorBrush(Color.FromRgb(238,238,238)));

            //dismissedAlertsMenuItem.ItemTemplate = itemTemplate;

            //AddContextMenuItem(dismissedAlertsMenuItem, insertionPoint++);

            return null;
        }

        private static void AddInjectedNodeViewCustomizationMenuItems(Dictionary<string, object> nodeViewCustomizationMenuItems)
        {
            foreach (KeyValuePair<string, object> keyValuePair in nodeViewCustomizationMenuItems)
            {
                ContextMenu.Items.Add(keyValuePair.Value);
            }
        }

        private static MenuItem BuildIsInputMenuItem()
        {
            return CreateMenuItem
            (
                name: "isInput_cm",
                header: Properties.Resources.NodeContextMenuIsInput,
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("IsSetAsInput"),
                    Mode = BindingMode.TwoWay,
                }
            );
        }

        private static MenuItem BuildIsOutputMenuItem()
        {
            return CreateMenuItem
            (
                name: "isOutput_cm",
                header: Properties.Resources.NodeContextMenuIsOutput,
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("IsSetAsOutput"),
                    Mode = BindingMode.TwoWay,
                }
            );
        }

        private static MenuItem BuildHelpMenuItem()
        {
            return CreateMenuItem
            (
                name: "help_cm",
                header: Properties.Resources.NodeContextMenuHelp,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath("ShowHelpCommand")
                }
            );
        }

        /// <summary>
        /// A list of the names of nodes' default menu items, which cannot be dynamically injected into its context menu.
        /// </summary>
        internal static readonly List<string> NodeContextMenuDefaultItemNames = new List<string>
        {
            Properties.Resources.ContextMenuDelete,
            Properties.Resources.ContextMenuGroups,
            Properties.Resources.NodeContextMenuPreview,
            Properties.Resources.NodesRunStatus,
            Properties.Resources.NodeContextMenuShowLabels,
            Properties.Resources.NodeContextMenuRenameNode,
            Properties.Resources.ContextMenuLacing,
            //Wpf.Properties.Resources.NodeInformationalStateDismissedAlerts,
            Properties.Resources.NodeContextMenuIsInput,
            Properties.Resources.NodeContextMenuIsOutput,
            Properties.Resources.NodeContextMenuHelp
        };
    }
}
