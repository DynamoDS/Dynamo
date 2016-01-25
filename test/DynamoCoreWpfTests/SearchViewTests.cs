using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Search;
using Dynamo.UI.Views;
using Dynamo.Wpf.ViewModels;

namespace DynamoCoreWpfTests
{
    class SearchViewTests : DynamoTestUIBase
    {
        private TextBox SearchTextBox
        {
            get { return View.ChildOfType<SearchView>().SearchTextBox; }
        }

        private void RemoveFocusFromSearch()
        {
            View.FocusableGrid.Focus();
            Keyboard.Focus(View);
            Assert.IsFalse(SearchTextBox.IsFocused);
        }

        public override void Open(string path)
        {
            base.Open(path);
            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();
            DispatcherUtil.DoEvents();
        }

        [Test]
        public void ClickOnLibraryBackgroundReturnsFocusToSearch()
        {
            RemoveFocusFromSearch();

            var libGrid = View.ChildOfType<LibraryView>().LibraryGrid;
            // Raise a click event to check returning focus
            libGrid.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseUpEvent
            });

            Assert.IsTrue(SearchTextBox.IsFocused);
        }

        [Test]
        public void ClickOnLibraryItemReturnsFocusToSearch()
        {
            RemoveFocusFromSearch();

            var category = View.ChildOfType<LibraryView>().ChildOfType<TreeViewItem>();
            Assert.IsTrue(category.DataContext is NodeCategoryViewModel);
            
            // Raise a click event to check returning focus
            category.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseUpEvent
            });

            Assert.IsTrue(SearchTextBox.IsFocused);
        }
    }
}