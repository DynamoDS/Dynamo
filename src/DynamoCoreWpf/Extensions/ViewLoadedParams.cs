using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Extensions;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// Application level parameters passed to an extension when the DynamoView is loaded
    /// </summary>
    public class ViewLoadedParams : ReadyParams
    {
        private readonly DynamoView dynamoView;
        private readonly DynamoViewModel dynamoViewModel;
        public readonly Menu dynamoMenu;
        private readonly ViewStartupParams viewStartupParams;

        /// <summary>
        /// A reference to the <see cref="ViewStartupParams"/> class.
        /// Useful if this extension will be loaded from a package, as its startup method will not be called.
        /// </summary>
        public ViewStartupParams ViewStartupParams
        {
            get
            {
                return viewStartupParams;
            }
        }

        /// <summary>
        /// A reference to the background preview viewmodel for geometry selection,
        /// hit testing, mouse and keyboard event handling for events in the background preview 
        /// </summary>
        public IWatch3DViewModel BackgroundPreviewViewModel { get { return dynamoViewModel.BackgroundPreviewViewModel; } }

        /// <summary>
        /// A reference to the factory for creating render packages in the extension
        /// </summary>
        public IRenderPackageFactory RenderPackageFactory
        {
            get { return dynamoViewModel.RenderPackageFactoryViewModel.Factory; }
        }

        /// <summary>
        /// A reference to the Dynamo Window object. Useful for correctly setting the parent of a 
        /// newly created window.
        /// </summary>
        public Window DynamoWindow
        {
            get
            {
                return dynamoView;
            }
        }

        internal ViewLoadedParams(DynamoView dynamoV, DynamoViewModel dynamoVM) :
            base(dynamoVM.Model)
        {
            dynamoView = dynamoV;
            dynamoViewModel = dynamoVM;
            dynamoMenu = dynamoView.titleBar.ChildOfType<Menu>();
            viewStartupParams = new ViewStartupParams(dynamoVM);
            DynamoSelection.Instance.Selection.CollectionChanged += OnSelectionCollectionChanged;

        }

        public void AddMenuItem(MenuBarType type, MenuItem menuItem, int index = -1)
        {
            AddItemToMenu(type, menuItem, index);
        }

        public void AddSeparator(MenuBarType type, Separator separatorObj, int index = -1)
        {
            AddItemToMenu(type, separatorObj, index);
        }

        private ICommandExecutive commandExecutive;
        /// <summary>
        /// View Extension specific implementation to execute Recordable commands on DynamoViewModel
        /// </summary>
        public override ICommandExecutive CommandExecutive
        {
            get { return commandExecutive ?? (commandExecutive = new ViewExtensionCommandExecutive(dynamoViewModel)); }
        }

        /// <summary>
        /// Event raised when there's a change in selection of nodes in the workspace.
        /// This event is subscribed to in the extension for any callback necessary for this event
        /// </summary>
        public event Action<NotifyCollectionChangedEventArgs> SelectionCollectionChanged;
        private void OnSelectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (SelectionCollectionChanged != null)
            {
                SelectionCollectionChanged(notifyCollectionChangedEventArgs);
            }
        }

        private void AddItemToMenu(MenuBarType type, Control itemToAdd, int index)
        {
            if (dynamoMenu == null) return;

            var dynamoItem = SearchForMenuItem(type);
            if (dynamoItem == null) return;

            if (index >= 0 && index < dynamoItem.Items.Count)
            {
                dynamoItem.Items.Insert(index, itemToAdd);
            }
            else
            {
                dynamoItem.Items.Add(itemToAdd);
            }
        }

        /// <summary>
        /// Searchs for dynamo parent menu item. Parent item can be:
        /// file menu, edit menu, view menu and help mebu bars.
        /// </summary>
        /// <param name="menuBarType">File, Edit, View or Help.</param>
        private MenuItem SearchForMenuItem(MenuBarType type)
        {
            var dynamoMenuItems = dynamoMenu.Items.OfType<MenuItem>();
            return dynamoMenuItems.First(item => item.Header.ToString() == type.ToDisplayString());
        }

    }
    /// <summary>
    /// An enum that represents the different possible 
    /// MenuBars which ViewExtensions may add items to.
    /// </summary>
    public enum MenuBarType
    {
        File,
        Edit,
        View,
        Help,
        Packages
    }

}
