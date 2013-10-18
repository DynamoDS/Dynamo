using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Search.Regions;
using Dynamo.ViewModels;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;
using System.Windows.Media;
using Dynamo.Utilities;
using DynamoCommands = Dynamo.UI.Commands.DynamoCommands;
using Dynamo.Search.SearchElements;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Dynamo.Selection;

//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

namespace Dynamo.Search
{
    /// <summary>
    ///     Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        private SearchViewModel _viewModel;

        public SearchView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SearchView_Loaded);

            SearchTextBox.IsVisibleChanged += delegate
            {
                if (SearchTextBox.IsVisible)
                {
                    DynamoCommands.SearchCommand.Execute(null);
                    Keyboard.Focus(this.SearchTextBox);
                    var view = WPF.FindUpVisualTree<DynamoView>(this);
                    SearchTextBox.InputBindings.AddRange(view.InputBindings);
                }
            };
        }

        void SearchView_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel = dynSettings.Controller.SearchViewModel;

            PreviewKeyDown += KeyHandler;
            this.SearchTextBox.PreviewKeyDown += new KeyEventHandler(OnSearchBoxPreviewKeyDown);
            this.SearchTextBox.KeyDown += new KeyEventHandler(OnSearchBoxKeyDown);

            dynSettings.Controller.SearchViewModel.RequestFocusSearch += new EventHandler(SearchViewModel_RequestFocusSearch);
            dynSettings.Controller.SearchViewModel.RequestReturnFocusToSearch += new EventHandler(SearchViewModel_RequestReturnFocusToSearch);

            //setup the regions on the view model
            _viewModel.Regions = new ObservableDictionary<string, RegionBase>();
            //Regions.Add("Include Nodes from Package Manager", DynamoCommands.PackageManagerRegionCommand );
            //var region = new RevitAPIRegion(SearchViewModel.RevitAPIRegionExecute, SearchViewModel.RevitAPIRegionCanExecute);
            //region.RaiseCanExecuteChanged();
            //_viewModel.Regions.Add("Include Experimental Revit API Nodes", region);

        }

        void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            bool handleIt = false;
            e.Handled = handleIt;
        }

        void OnSearchBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool handleIt = false;
            e.Handled = handleIt;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            bool handleIt = false;

            if (false != handleIt)
            {
                base.OnPreviewKeyDown(e);
                e.Handled = true;
            }
        }

        /// <summary>
        ///     A KeyHandler method used by SearchView, increments decrements and executes based on input.
        /// </summary>
        /// <param name="sender">Originating object for the KeyHandler </param>
        /// <param name="e">Parameters describing the key push</param>
        public void KeyHandler(object sender, KeyEventArgs e)
        {

            // ignore the key command if modifiers are present
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || 
                e.KeyboardDevice.IsKeyDown(Key.RightCtrl) || 
                e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || 
                e.KeyboardDevice.IsKeyDown(Key.RightAlt))
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Return:
                    _viewModel.ExecuteSelected();
                    break;

                case Key.Delete:
                    if (DynamoSelection.Instance.Selection.Count > 0)
                    {
                        e.Handled = true;
                        dynSettings.Controller.DynamoViewModel.DeleteCommand.Execute(null);
                    }
                    break;

                case Key.Tab:
                    _viewModel.PopulateSearchTextWithSelectedResult();
                    break;

                case Key.Down:
                    _viewModel.SelectNext();
                    break;

                case Key.Up:
                    _viewModel.SelectPrevious();
                    break;
            }
        }

        void SearchViewModel_RequestReturnFocusToSearch(object sender, EventArgs e)
        {
            Keyboard.Focus(SearchTextBox);
        }

        void SearchViewModel_RequestFocusSearch(object sender, EventArgs e)
        {
            SearchTextBox.Focus();
        }

        public void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BindingExpression binding = ((TextBox) sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();
        }

        public void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((SearchViewModel) DataContext).ExecuteSelected();
        }

        public void ListBoxItem_Click(object sender, RoutedEventArgs e)
        {
            ((ListBoxItem) sender).IsSelected = true;
            Keyboard.Focus(this.SearchTextBox);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel) DataContext).RemoveLastPartOfSearchText();
            Keyboard.Focus(this.SearchTextBox);
        }

        public void ibtnServiceController_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            //RegionMenu.PlacementTarget = (UIElement) sender;
            //RegionMenu.IsOpen = true;
        }

        private void TreeViewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

		private void OnLibraryClick(object sender, RoutedEventArgs e)
        {
            //this.Width = 5;
            //if (this.Visibility == Visibility.Collapsed)
            //    this.Visibility = Visibility.Visible;
            //else
            //{
            //    dynSettings.Controller.DynamoViewModel.OnSidebarClosed(this, EventArgs.Empty);
            //   this.Visibility = Visibility.Collapsed;
            //}
            dynSettings.Controller.DynamoViewModel.OnSidebarClosed(this, EventArgs.Empty);
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            Label lb = (Label)(g.Children[0]);
            var bc = new BrushConverter();
            lb.Foreground = (Brush)bc.ConvertFromString("#cccccc");
            Image collapsestate = (Image)g.Children[1];
            var collapsestateSource = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/collapsestate_hover.png");
            BitmapImage bmi = new BitmapImage(collapsestateSource);
            RotateTransform rotateTransform = new RotateTransform(-90, 16, 16);
            collapsestate.Source = new BitmapImage(collapsestateSource);
        }

        private void buttonGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            Label lb = (Label)(g.Children[0]);
            var bc = new BrushConverter();
            lb.Foreground = (Brush)bc.ConvertFromString("#aaaaaa");
            Image collapsestate = (Image)g.Children[1];
            var collapsestateSource = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/collapsestate_normal.png");
            collapsestate.Source = new BitmapImage(collapsestateSource);
        }

        private void LibraryItem_OnMouseEnter(object sender, MouseEventArgs e)
        {
            TreeViewItem treeViewItem = sender as TreeViewItem;
            NodeSearchElement nodeSearchElement = treeViewItem.Header as NodeSearchElement;
            if (nodeSearchElement == null)
                return;

            Point pointToScreen_TopLeft = treeViewItem.PointToScreen(new Point(0, 0));
            Point topLeft = this.PointFromScreen(pointToScreen_TopLeft);
            Point pointToScreen_BotRight = new Point(pointToScreen_TopLeft.X + treeViewItem.ActualWidth, pointToScreen_TopLeft.Y + treeViewItem.ActualHeight);
            Point botRight = this.PointFromScreen(pointToScreen_BotRight);
            string infoBubbleContent = nodeSearchElement.Name + "\n" + nodeSearchElement.Description;
            InfoBubbleDataPacket data = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.LibraryItemPreview, topLeft, botRight, infoBubbleContent, InfoBubbleViewModel.Direction.Left, Guid.Empty);
            DynamoCommands.ShowLibItemInfoBubbleCommand.Execute(data);
        }

        private void LibraryItem_OnMouseLeave(object sender, MouseEventArgs e)
        {
            TreeViewItem treeViewItem = sender as TreeViewItem;
            NodeSearchElement nodeSearchElement = treeViewItem.Header as NodeSearchElement;
            if (nodeSearchElement == null)
                return;
            DynamoCommands.HideLibItemInfoBubbleCommand.Execute(null);
        }
    }
}
