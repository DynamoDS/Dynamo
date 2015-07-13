using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.UI.Controls;
using Dynamo.ViewModels;
using Dynamo.Utilities;
using Dynamo.Interfaces;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// Application level parameters passed to an extension when the DynamoView is loaded
    /// </summary>
    public class ViewLoadedParams
    {
        private readonly DynamoView dynamoView;
        private readonly DynamoViewModel dynamoViewModel;
        public readonly Menu dynamoMenu;

        public IEnumerable<IWorkspaceModel> WorkspaceModels
        {
            get
            {
                return dynamoViewModel.Model.Workspaces;
            }
        }

        public ViewLoadedParams(DynamoView dynamoV, DynamoViewModel dynamoVM)
        {
            dynamoView = dynamoV;
            dynamoViewModel = dynamoVM;
            dynamoMenu = dynamoView.titleBar.ChildOfType<Menu>();
        }

        public void AddMenuItem(MenuBarType type, MenuItem menuItem)
        {
            if (dynamoMenu == null)
                return;

            var dynamoItem = SearchForMenuItem(type);
            if (dynamoItem == null)
                return;

            dynamoItem.Items.Add(menuItem);
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
