using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
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
        internal static NodeViewModel NodeViewModel { get; set; }

        /// <summary>
        /// A reference to the ContextMenu to which items are being added. 
        /// </summary>
        internal static ContextMenu ContextMenu { get; set; }

        // Builds the node's Context Menu, including re-adding any injected MenuItems from
        // the NodeViewCustomization process.
        internal static void Build(ContextMenu contextMenu, NodeViewModel nodeViewModel, OrderedDictionary nodeViewCustomizationMenuItems)
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
            
            if(NodeViewModel.DismissedAlerts.Count > 0)
            {
                AddContextMenuItem(BuildDismissedAlertsMenuItem());
            }
            
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
        internal static void AddContextMenuItem(MenuItem menuItem) => ContextMenu.Items.Add(menuItem);

        /// <summary>
        /// Adds a new separator a context menu.
        /// </summary>
        internal static void AddContextMenuSeparator() => ContextMenu.Items.Add(new Separator());

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
        /// <param name="itemsSource"></param>
        /// <param name="hotkey"></param>
        /// <returns></returns>
        internal static MenuItem CreateMenuItem
        (
            string name = null,
            string header = null,
            Binding command = null,
            string commandParameter = null,
            bool isCheckable = false,
            Binding isChecked = null,
            Binding visibility = null,
            Binding isEnabled = null,
            Binding itemsSource = null,
            string hotkey = null
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
            if (!string.IsNullOrWhiteSpace(hotkey)) menuItem.InputGestureText = hotkey;

            return menuItem;
        }

        internal static MenuItem BuildDeleteMenuItem()
        {
            return CreateMenuItem
            (
                name: "deleteElem_cm",
                header: Properties.Resources.ContextMenuDelete,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.DeleteCommand))
                }
            );
        }

        internal static MenuItem BuildGroupsMenuItem()
        {
            MenuItem groupsMenuItem = CreateMenuItem(header: Properties.Resources.ContextMenuGroups);

            groupsMenuItem.Items.Add(CreateMenuItem
                (
                    name: "createGroup_cm",
                    header: Properties.Resources.ContextCreateGroupFromSelection,
                    command: new Binding
                    {
                        Source = NodeViewModel,
                        Path = new PropertyPath(nameof(NodeViewModel.CreateGroupCommand))
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
                        Path = new PropertyPath(nameof(NodeViewModel.UngroupCommand))
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
                        Path = new PropertyPath(nameof(NodeViewModel.AddToGroupCommand))
                    }
                )
            );

            return groupsMenuItem;
        }

        internal static MenuItem BuildPreviewMenuItem()
        {
            MenuItem previewMenuItem = CreateMenuItem
            (
                name: "isVisible_cm",
                header: Properties.Resources.NodeContextMenuPreview,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.ToggleIsVisibleCommand))
                },
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.IsVisible)),
                    Mode = BindingMode.OneWay
                }
            );

            return previewMenuItem;
        }

        internal static MenuItem BuildFreezeMenuItem()
        {
            MenuItem freezeMenuItem = CreateMenuItem
            (
                name: "nodeIsFrozen",
                header: Properties.Resources.NodesRunStatus,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.ToggleIsFrozenCommand))
                },
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.IsFrozenExplicitly)),
                    Mode = BindingMode.OneWay
                },
                isEnabled: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.CanToggleFrozen)),
                    Mode = BindingMode.OneWay
                }
            );

            return freezeMenuItem;
        }

        internal static MenuItem BuildShowLabelsMenuItem()
        {
            MenuItem showLabelsMenuItem = CreateMenuItem
            (
                name: "isDisplayLabelsEnabled_cm",
                header: Properties.Resources.NodeContextMenuShowLabels,
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.IsDisplayingLabels)),
                    Mode = BindingMode.TwoWay
                },
                isEnabled: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.CanDisplayLabels))
                }
            );

            return showLabelsMenuItem;
        }

        internal static MenuItem BuildRenameMenuItem()
        {
            MenuItem renameMenuItem = CreateMenuItem
            (
                name: "rename_cm",
                header: Properties.Resources.NodeContextMenuRenameNode,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.RenameCommand))
                }
            );
            return renameMenuItem;
        }

        internal static MenuItem BuildLacingMenuItem()
        {
            MenuItem lacingMenuItem = CreateMenuItem(header: Properties.Resources.ContextMenuLacing);

            lacingMenuItem.Items.Add(CreateMenuItem
            (
                header: Properties.Resources.ContextMenuLacingAuto,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.SetLacingTypeCommand)),
                },
                commandParameter: "Auto",
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.ArgumentLacing)),
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
                    Path = new PropertyPath(nameof(NodeViewModel.SetLacingTypeCommand)),
                },
                commandParameter: "Shortest",
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.ArgumentLacing)),
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
                    Path = new PropertyPath(nameof(NodeViewModel.SetLacingTypeCommand)),
                },
                commandParameter: "Longest",
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.ArgumentLacing)),
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
                    Path = new PropertyPath(nameof(NodeViewModel.SetLacingTypeCommand)),
                },
                commandParameter: "CrossProduct",
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.ArgumentLacing)),
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = "CrossProduct",
                    Mode = BindingMode.OneWay
                }
            ));

            return lacingMenuItem;
        }

        internal static MenuItem BuildDismissedAlertsMenuItem()
        {
            MenuItem dismissedAlertsMenuItem = CreateMenuItem
            (
                name: "dismissedAlerts",
                header: Properties.Resources.NodeInformationalStateDismissedAlerts,
                itemsSource: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.DismissedAlerts)),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                }
            );
            
            dismissedAlertsMenuItem.Click += DismissedAlertsMenuItemOnClick;
            return dismissedAlertsMenuItem;
        }

        /// <summary>
        /// Allows for any previously-dismissed errors/warnings/info messages to be un-dismissed
        /// and re-displayed on the node in question.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DismissedAlertsMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem menuItem)) return;

            NodeViewModel.ErrorBubble.UndismissMessageCommand.Execute(menuItem.Header);

            Analytics.TrackEvent(Actions.Undismiss, Categories.NodeContextMenuOperations, "NodeAlerts");
        }

        /// <summary>
        /// Loops through the previously-stashed collection of MenuItems that were injected
        /// during the NodeViewCustomization process and adds them back into the node's
        /// context menu. This ensures they appear in a consistent location.
        /// </summary>
        /// <param name="nodeViewCustomizationMenuItems"></param>
        internal static void AddInjectedNodeViewCustomizationMenuItems(OrderedDictionary nodeViewCustomizationMenuItems)
        {
            foreach (DictionaryEntry keyValuePair in nodeViewCustomizationMenuItems)
            {
                ContextMenu.Items.Add(keyValuePair.Value);
            }
        }

        internal static MenuItem BuildIsInputMenuItem()
        {
            return CreateMenuItem
            (
                name: "isInput_cm",
                header: Properties.Resources.NodeContextMenuIsInput,
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.IsSetAsInput)),
                    Mode = BindingMode.TwoWay,
                }
            );
        }

        internal static MenuItem BuildIsOutputMenuItem()
        {
            return CreateMenuItem
            (
                name: "isOutput_cm",
                header: Properties.Resources.NodeContextMenuIsOutput,
                isCheckable: true,
                isChecked: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.IsSetAsOutput)),
                    Mode = BindingMode.TwoWay,
                }
            );
        }

        internal static MenuItem BuildHelpMenuItem()
        {
            return CreateMenuItem
            (
                name: "help_cm",
                header: Properties.Resources.NodeContextMenuHelp,
                command: new Binding
                {
                    Source = NodeViewModel,
                    Path = new PropertyPath(nameof(NodeViewModel.ShowHelpCommand))
                },
                hotkey: "F1"
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
            Properties.Resources.NodeInformationalStateDismissedAlerts,
            Properties.Resources.NodeContextMenuIsInput,
            Properties.Resources.NodeContextMenuIsOutput,
            Properties.Resources.NodeContextMenuHelp
        };
    }
}
