using Dynamo.TestInfrastructure;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using MutationTestInfrastructure.Properties;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MutationTestInfrastructure
{
    public class MutationTestInfrastructureExtension : IViewExtension
    {
        private MutatorDriver mutatorDriver;

        private Menu dynamoMenu;
        private MenuItem mutationMenuItem;

        private DynamoViewModel dynamoViewModel;

        #region IViewExtension

        public string UniqueId
        {
            get { return ""; }
        }

        public string Name
        {
            get { return "MutationInfrastructureExtension"; }
        }

        public void Startup(ViewStartupParams p)
        {
            mutatorDriver = new MutatorDriver(p.DynamoViewModel);

            dynamoViewModel = p.DynamoViewModel;
        }

        public void Loaded(ViewLoadedParams p)
        {
            dynamoMenu = p.dynamoMenu;

            mutationMenuItem = GenerateMutationMenuItem();
            p.AddMenuItem(MenuBarType.Debug, mutationMenuItem, 3);
        }

        public void Shutdown()
        {

        }

        public void Dispose()
        {
            ClearMenuItem(mutationMenuItem);
        }

        #endregion

        #region Helpers

        private MenuItem GenerateMutationMenuItem()
        {
            MenuItem item = new MenuItem();
            item.Header = Resources.MutationMenuItemTitle;

            item.Click += (sender, args) =>
            {
                mutatorDriver.RunMutationTests();
            };

            Binding binding = new Binding();
            binding.Source = dynamoViewModel;
            binding.Path = new PropertyPath("HomeSpaceViewModel.RunSettingsViewModel.RunButtonEnabled");
            binding.Mode = BindingMode.OneWay;

            BindingOperations.SetBinding(item, MenuItem.IsEnabledProperty, binding);

            return item;
        }

        /// <summary>
        /// Delete menu item from Dynamo.
        /// </summary>
        private void ClearMenuItem(params MenuItem[] menuItem)
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
