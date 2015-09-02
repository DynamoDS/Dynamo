using Dynamo.Interfaces;
using Dynamo.Publish.Models;
using Dynamo.Publish.ViewModels;
using Dynamo.Publish.Views;
using Dynamo.Wpf.Extensions;
using System;
using System.Windows.Controls;
using System.Linq;
using Dynamo.Models;
using Dynamo.Publish.Properties;

namespace Dynamo.Publish
{
    public class DynamoPublishExtension : IViewExtension, ILogSource
    {
        private PublishViewModel publishViewModel;
        private PublishModel publishModel;
        private InviteViewModel inviteViewModel;
        private InviteModel inviteModel;
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
            publishModel = new PublishModel(p.AuthProvider, p.CustomNodeManager);
            publishViewModel = new PublishViewModel(publishModel);
           
            inviteModel = new InviteModel(p.AuthProvider);
            inviteModel.MessageLogged += this.OnMessageLogged;
            inviteViewModel = new InviteViewModel(inviteModel);            
        }

        public void Loaded(ViewLoadedParams p)
        {
            if (publishViewModel == null || inviteViewModel == null)
                return;

            publishViewModel.Workspaces = p.WorkspaceModels;
            publishViewModel.CurrentWorkspaceModel = p.CurrentWorkspaceModel;

            dynamoMenu = p.dynamoMenu;
            extensionMenuItem = GenerateMenuItem();
            p.AddMenuItem(MenuBarType.File, extensionMenuItem, 11);

            manageCustomizersMenuItem = GenerateManageCustomizersMenuItem();
            p.AddMenuItem(MenuBarType.File, manageCustomizersMenuItem, 12);

            inviteMenuItem = GenerateInviteMenuItem();
            p.AddMenuItem(MenuBarType.File, inviteMenuItem, 11);

            p.AddSeparator(MenuBarType.File, separator, 14);

            p.CurrentWorkspaceChanged += (ws) =>
            {
                publishViewModel.CurrentWorkspaceModel = ws;

                var isEnabled = ws is HomeWorkspaceModel && publishModel.HasAuthProvider;
                extensionMenuItem.IsEnabled = isEnabled;
            };

        }

        public void Shutdown()
        {

        }

        public void Dispose()
        {
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
            MenuItem item = new MenuItem();
            item.Header = Resources.DynamoViewMenuItemPublishTitle;

            var isEnabled = publishViewModel.CurrentWorkspaceModel is HomeWorkspaceModel && publishModel.HasAuthProvider;

            item.IsEnabled = isEnabled;

            item.Click += (sender, args) =>
                {
                    PublishView publishWindow = new PublishView(publishViewModel);
                    publishWindow.ShowDialog();                    
                };

            return item;
        }

        /// <summary>
        /// Generates the invite menu item.
        /// </summary>
        /// <returns></returns>
        private MenuItem GenerateInviteMenuItem()
        {
            MenuItem item = new MenuItem();
            item.Header = Resources.InviteViewMenuTitle;

            var isEnabled = inviteModel.HasAuthProvider;

            item.IsEnabled = isEnabled;

            item.Click += (sender, args) =>
            {
                InviteView inviteWindow = new InviteView(inviteViewModel);
                inviteWindow.ShowDialog();
            };

            return item;

        }

        private MenuItem GenerateManageCustomizersMenuItem()
        {
            MenuItem item = new MenuItem();
            item.Header = Resources.ManageButtonTitle;

            item.Click += (sender, args) =>
            {
                System.Diagnostics.Process.Start(publishViewModel.ManagerURL);
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
