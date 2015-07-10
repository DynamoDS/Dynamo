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
        }

        public void AddMenuItem(MenuBarType type, MenuItem menuItem)
        {
            var dynamoMenu = dynamoView.titleBar.ChildOfType<Menu>();

            if (dynamoMenu == null)
                return;

            var dynamoMenuItems = dynamoMenu.Items.OfType<MenuItem>();
            var dynamoItem = dynamoMenuItems.First(item => item.Header.ToString() == "_" + type);
            if (dynamoItem == null)
                return;

            dynamoItem.Items.Add(menuItem);
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
