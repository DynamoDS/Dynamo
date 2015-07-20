﻿using Dynamo.Interfaces;
using Dynamo.Publish.Models;
using Dynamo.Publish.ViewModels;
using Dynamo.Publish.Views;
using Dynamo.Wpf.Extensions;
using System;
using System.Windows.Controls;
using System.Linq;

namespace Dynamo.Publish
{
    public class DynamoPublishExtension : IViewExtension, ILogSource
    {

        private PublishViewModel publishViewModel;
        private PublishModel publishModel;
        private Menu dynamoMenu;
        private MenuItem extensionMenuItem;

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
        }

        public void Loaded(ViewLoadedParams p)
        {
            if (publishViewModel == null)
                return;

            publishViewModel.Workspaces = p.WorkspaceModels;

            dynamoMenu = p.dynamoMenu;
            extensionMenuItem = GenerateMenuItem();
            p.AddMenuItem(MenuBarType.File, extensionMenuItem, 11);
        }

        public void Shutdown()
        {

        }

        public void Dispose()
        {
            ClearMenuItem(extensionMenuItem);
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
            item.Header = Resource.DynamoViewMenuItemPublishTitle;

            item.Click += (sender, args) =>
                {
                    PublishView publishWindow = new PublishView(publishViewModel);
                    publishWindow.ShowDialog();
                };

            return item;
        }

        /// <summary>
        /// Delete menu item from Dynamo.
        /// </summary>
        private void ClearMenuItem(MenuItem menuItem)
        {
            if (dynamoMenu == null)
                return;

            var dynamoItem = SearchForMenuItemRecursively(dynamoMenu.Items, menuItem);
            if (dynamoItem == null)
                return;

            dynamoItem.Items.Remove(menuItem);
        }


        /// <summary>
        /// Searches for given menu item. 
        /// First it tries to find it among first layer items (e.g. File, Edit, etc.)
        /// If it doesn't find given menuitem, it tries to search one layer deeper.
        /// </summary>
        /// <param name="menuItems">Menu items among which we will search for needed item.</param>
        /// <param name="searchItem">Menu item, that we want to find.</param>
        /// <returns>Returns parent item for searched item.</returns>
        private MenuItem SearchForMenuItemRecursively(ItemCollection menuItems, MenuItem searchItem)
        {
            var collectionOfItems = menuItems.OfType<MenuItem>();
            foreach (var item in collectionOfItems)
            {
                if (item == searchItem)
                    return item.Parent as MenuItem;
            }

            foreach (var item in collectionOfItems)
            {
                return SearchForMenuItemRecursively(item.Items, searchItem);
            }

            return null;
        }
        #endregion
    }
}
