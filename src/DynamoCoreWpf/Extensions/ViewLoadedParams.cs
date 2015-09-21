using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Controls;
using Dynamo.UI.Controls;
using Dynamo.ViewModels;
using Dynamo.Utilities;
using Dynamo.Interfaces;
using Dynamo.Extensions;
using Dynamo.Selection;

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

        public Watch3DView BackgroundPreView { get { return dynamoView.BackgroundPreview; } }

        internal ViewLoadedParams(DynamoView dynamoV, DynamoViewModel dynamoVM) :
            base(dynamoVM.Model)
        {
            dynamoView = dynamoV;
            dynamoViewModel = dynamoVM;
            dynamoMenu = dynamoView.titleBar.ChildOfType<Menu>();

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

        public event Action<NotifyCollectionChangedEventArgs> SelectionCollectionChanged;
        private void OnSelectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (SelectionCollectionChanged != null)
                SelectionCollectionChanged(notifyCollectionChangedEventArgs);
        }

        public void OnRequestCreateModels(IEnumerable<IRenderPackage> packages)
        {
            dynamoViewModel.BackgroundPreviewViewModel.OnRequestCreateModels(packages);
        }

        public void OnRequestDeleteModels(string identifier)
        {
            dynamoViewModel.BackgroundPreviewViewModel.DeleteGeometryForIdentifier(identifier);
        }

        private void AddItemToMenu(MenuBarType type, Control itemToAdd, int index)
        {
            if (dynamoMenu == null)
                return;

            var dynamoItem = SearchForMenuItem(type);
            if (dynamoItem == null)
                return;

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
            return dynamoMenuItems.First(item => item.Header.ToString() == "_" + type);
        }

    }

    public enum MenuBarType
    {
        File,
        Edit,
        View,
        Help
    }
}
