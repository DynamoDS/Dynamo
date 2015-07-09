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
        // TBD MAGN-7366
        //
        // Implementation notes:
        // 
        // This should be designed primarily to support the separation of the Package Manager from Core
        // and minimize exposing unnecessary innards.
        //
        // It is expected that this class will be extended in the future, so it should stay as minimal as possible.
        //
        // Here's a start on the implementation
        //

        private DynamoView dynamoView;
        private DynamoViewModel dynamoViewModel;

        public IEnumerable<IWorkspaceModel> WorkSpaces
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
