using Dynamo.Interfaces;
using Dynamo.Publish.Models;
using Dynamo.Publish.ViewModels;
using Dynamo.Publish.Views;
using Dynamo.Wpf.Extensions;
using System;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Publish.Properties;
using Greg;
using Reach.Upload;

namespace Dynamo.Publish
{
    public class DynamoPublishExtension : IViewExtension, ILogSource
    {
        private ViewStartupParams startupParams;
        private ViewLoadedParams loadedParams;

        private Menu dynamoMenu;
        private MenuItem extensionMenuItem; 
        private MenuItem inviteMenuItem; 
        private MenuItem manageCustomizersMenuItem;
        private Separator separator = new Separator();
 
        #region IViewExtension implementation

        public string UniqueId
        {
            get { return "BCABC211-D56B-4109-AF18-F434DFE48139"; }
        }

        public string Name
        {
            get { return "DynamoPublishExtension"; }
        }

        public void Startup(ViewStartupParams p)
        {
            this.startupParams = p;
        }

        public void Loaded(ViewLoadedParams p)
        {
            this.loadedParams = p;

            dynamoMenu = p.dynamoMenu;
            extensionMenuItem = GenerateMenuItem();
            p.AddMenuItem(MenuBarType.File, extensionMenuItem, 11);

            manageCustomizersMenuItem = GenerateManageCustomizersMenuItem();
            p.AddMenuItem(MenuBarType.File, manageCustomizersMenuItem, 12);

            inviteMenuItem = GenerateInviteMenuItem();
            p.AddMenuItem(MenuBarType.File, inviteMenuItem, 11);

            p.AddSeparator(MenuBarType.File, separator, 14);

            p.CurrentWorkspaceChanged += CurrentWorkspaceChanged;

        }

        private void CurrentWorkspaceChanged(IWorkspaceModel ws)
        {
            var isEnabled = ws is HomeWorkspaceModel && startupParams.AuthProvider != null;
            extensionMenuItem.IsEnabled = isEnabled;
        }

        public void Shutdown()
        {

        }

        public void Dispose()
        {
            loadedParams.CurrentWorkspaceChanged -= CurrentWorkspaceChanged;

            ClearMenuItem(extensionMenuItem, inviteMenuItem, manageCustomizersMenuItem, separator);
        }

        #endregion

        #region ILogSource implementation

        public event Action<ILogMessage> MessageLogged;

        private void OnMessageLogged(ILogMessage msg)
        {
            if (this.MessageLogged != null)
            {
                this.MessageLogged(msg);
            }
        }

        #endregion

        #region Helpers

        private MenuItem GenerateMenuItem()
        {
            var item = new MenuItem();
            item.Header = Resources.DynamoViewMenuItemPublishTitle;

            var isEnabled = loadedParams.CurrentWorkspaceModel is HomeWorkspaceModel && startupParams.AuthProvider != null;
            item.IsEnabled = isEnabled;

            item.Click += (sender, args) =>
            {
                var model = new PublishModel(startupParams.AuthProvider, startupParams.CustomNodeManager);
                model.MessageLogged += this.OnMessageLogged;

                var viewModel = new PublishViewModel(model)
                {
                    CurrentWorkspaceModel = loadedParams.CurrentWorkspaceModel
                };

                var window = new PublishView(viewModel)
                {
                    Owner = loadedParams.DynamoWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                window.ShowDialog();

                model.MessageLogged -= this.OnMessageLogged;
            };

            return item;
        }

        /// <summary>
        /// Generates the invite menu item.
        /// </summary>
        /// <returns></returns>
        private MenuItem GenerateInviteMenuItem()
        {
            var item = new MenuItem();
            item.Header = Resources.InviteViewMenuTitle;

            var isEnabled = startupParams.AuthProvider != null;
            item.IsEnabled = isEnabled;

            item.Click += (sender, args) =>
            {
                var model = new InviteModel(startupParams.AuthProvider);
                model.MessageLogged += this.OnMessageLogged;

                var viewModel = new InviteViewModel(model);

                var view = new InviteView(viewModel)
                {
                    Owner = loadedParams.DynamoWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                view.ShowDialog();

                model.MessageLogged -= this.OnMessageLogged;
            };

            return item;
        }

        private MenuItem GenerateManageCustomizersMenuItem()
        {
            var item = new MenuItem();
            item.Header = Resources.ManageButtonTitle;

            item.Click += (sender, args) =>
            {
                System.Diagnostics.Process.Start(PublishModel.ManagerURL);
            };

            return item;
        }

        /// <summary>
        /// Delete menu item from Dynamo.
        /// </summary>
        private void ClearMenuItem(params Control[] menuItem)
        {
            if (dynamoMenu == null)
                return;

            foreach (var item in menuItem)
            {
                var dynamoItem = SearchForMenuItemRecursively(dynamoMenu.Items, item);
                if (dynamoItem == null)
                    return;

                dynamoItem.Items.Remove(menuItem);
            }
        }


        /// <summary>
        /// Searches for given menu item. 
        /// First it tries to find it among first layer items (e.g. File, Edit, etc.)
        /// If it doesn't find given menuitem, it tries to search one layer deeper.
        /// </summary>
        /// <param name="menuItems">Menu items among which we will search for needed item.</param>
        /// <param name="searchItem">Menu item, that we want to find.</param>
        /// <returns>Returns parent item for searched item.</returns>
        private MenuItem SearchForMenuItemRecursively(ItemCollection menuItems, Control searchItem)
        {
            var collectionOfItems = menuItems.OfType<Control>();
            foreach (var item in collectionOfItems)
            {
                if (item == searchItem)
                    return item.Parent as MenuItem;
            }

            var collectionOfMenuItems = menuItems.OfType<MenuItem>();
            foreach (var item in collectionOfMenuItems)
            {
                return SearchForMenuItemRecursively(item.Items, searchItem);
            }

            return null;
        }
        #endregion
    }
}
