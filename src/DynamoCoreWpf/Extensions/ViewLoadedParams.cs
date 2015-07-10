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
using Dynamo.Wpf.Interfaces;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// Application level parameters passed to an extension when the DynamoView is loaded
    /// </summary>
    public class ViewLoadedParams
    {
        // TBD MAGN-7366

        private readonly DynamoView dynamoView;
        private readonly DynamoViewModel dynamoViewModel;
        private readonly Menu dynamoMenu;

        private List<Tuple<MenuBarType, MenuItem>> addedMenuItems = new List<Tuple<MenuBarType, MenuItem>>();

        public IEnumerable<IWorkspaceViewModel> WorkspaceViewModels
        {
            get
            {
                return dynamoViewModel.Workspaces;
            }
        }

        public ViewLoadedParams(DynamoView dynamoV, DynamoViewModel dynamoVM)
        {
            dynamoView = dynamoV;
            dynamoViewModel = dynamoVM;
            dynamoMenu = dynamoView.titleBar.ChildOfType<Menu>();

            Disposable.Create(ClearMenuItems);
        }

        public void AddMenuItem(MenuBarType type, MenuItem menuItem)
        {
            if (dynamoMenu == null)
                return;
            
            var dynamoItem = SearchForMenuItem(type);
            if (dynamoItem == null)
                return;

            dynamoItem.Items.Add(menuItem);
            addedMenuItems.Add(Tuple.Create<MenuBarType, MenuItem>(type, menuItem));
        }

        private void ClearMenuItems()
        {
            foreach (var item in addedMenuItems)
            {
                var dynamoItem = SearchForMenuItem(item.Item1);
                if (dynamoItem == null)
                    continue;

                dynamoItem.Items.Remove(item.Item2);
            }
        }

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
