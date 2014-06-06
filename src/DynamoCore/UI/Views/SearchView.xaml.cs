﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoCommands = Dynamo.UI.Commands.DynamoCommands;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Dynamo.Search
{
    /// <summary>
    ///     Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        private SearchViewModel _viewModel;

        readonly DispatcherTimer searchTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 100), IsEnabled = false };

        public SearchView()
        {
            InitializeComponent();
            Loaded += SearchView_Loaded;
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

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

            searchTimer.Tick += SearchTimerTick;
        }

        void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            Debug.WriteLine("Note view unloaded.");

            if (dynSettings.Controller != null)
            {
                dynSettings.Controller.SearchViewModel.RequestFocusSearch -= SearchViewModel_RequestFocusSearch;
                dynSettings.Controller.SearchViewModel.RequestReturnFocusToSearch -= SearchViewModel_RequestReturnFocusToSearch;
            }
        }

        void SearchView_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel = dynSettings.Controller.SearchViewModel;

            this.MouseEnter += SearchView_MouseEnter;
            this.MouseLeave += SearchView_MouseLeave;

            PreviewKeyDown += KeyHandler;
            this.SearchTextBox.PreviewKeyDown += new KeyEventHandler(OnSearchBoxPreviewKeyDown);
            this.SearchTextBox.KeyDown += new KeyEventHandler(OnSearchBoxKeyDown);

            dynSettings.Controller.SearchViewModel.RequestFocusSearch += SearchViewModel_RequestFocusSearch;
            dynSettings.Controller.SearchViewModel.RequestReturnFocusToSearch += SearchViewModel_RequestReturnFocusToSearch;

        }

        void SearchView_MouseLeave(object sender, MouseEventArgs e)
        {
            _viewModel.SearchScrollBarVisibility = false;
        }

        void SearchView_MouseEnter(object sender, MouseEventArgs e)
        {
            _viewModel.SearchScrollBarVisibility = true;
        }

        void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            bool handleIt = false;
            e.Handled = handleIt;

            if (e.Key == Key.Escape)
            {
                ClearSearchBox();
            }
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
                e.KeyboardDevice.IsKeyDown(Key.RightAlt) )
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

                    //if there are no nodes being selected, the delete key should 
                    //delete the text in the search box of library preview
                    else {

                        //if there is no text, then jump out of the switch
                        if (String.IsNullOrEmpty(SearchTextBox.Text))
                        {
                            break;
                        }
                        else 
                        {
                            int cursorPosition = SearchTextBox.SelectionStart;
                            string searchBoxText = SearchTextBox.Text;

                            //if some piece of text is seleceted by users.
                            //delete this piece of text
                            if (SearchTextBox.SelectedText != "")
                            {
                                searchBoxText = searchBoxText.Remove(cursorPosition, 
                                    SearchTextBox.SelectionLength);
                            }

                            //if there is no text selected, delete the character after the cursor
                            else 
                            {
                                
                                //the cursor is at the end of this text string
                                if (cursorPosition == searchBoxText.Length)
                                {
                                    break;
                                }
                                else 
                                {
                                    searchBoxText = searchBoxText.Remove(cursorPosition, 1);
                                }
                            }

                            //update the SearchTextBox's text and the cursor position
                            SearchTextBox.Text = searchBoxText;
                            SearchTextBox.SelectionStart = cursorPosition;
                        }
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
            if (this.Visibility != Visibility.Collapsed)
                Keyboard.Focus(SearchTextBox);
            else
                MoveFocusToNextUIElement();
        }

        void MoveFocusToNextUIElement()
        {
            // Gets the element with keyboard focus.
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

            // Change keyboard focus.
            if (elementWithFocus != null)
                elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
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

            searchTimer.IsEnabled = true;
            searchTimer.Stop();
            searchTimer.Start();
        }

        void SearchTimerTick(object sender, EventArgs e)
        {
            searchTimer.IsEnabled = false;

            Debug.WriteLine("Updating search results...");
            // end of timer processing
            // Execute command to pop search stack
            DynamoCommands.SearchCommand.Execute(null);
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
            Button b = (Button)sender;
            Grid g = (Grid)b.Parent;
            Label lb = (Label)(g.Children[0]);
            var bc = new BrushConverter();
            lb.Foreground = (Brush)bc.ConvertFromString("#cccccc");
            Image collapsestate = (Image)(b).Content;
            var collapsestateSource = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/expand_hover.png");
            BitmapImage bmi = new BitmapImage(collapsestateSource);
            RotateTransform rotateTransform = new RotateTransform(-90, 16, 16);
            collapsestate.Source = new BitmapImage(collapsestateSource);
            
            this.Cursor = CursorLibrary.GetCursor(CursorSet.LinkSelect);
        }

        private void buttonGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Button b = (Button)sender;
            Grid g = (Grid)b.Parent;
            Label lb = (Label)(g.Children[0]);
            var bc = new BrushConverter();
            lb.Foreground = (Brush)bc.ConvertFromString("#aaaaaa");
            Image collapsestate = (Image)(b).Content;
            var collapsestateSource = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/expand_normal.png");
            collapsestate.Source = new BitmapImage(collapsestateSource);
            
            this.Cursor = null;
        }

        private void SearchTextBoxGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            var searchIconSource = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/search_hover.png");
            SearchIcon.Source = new BitmapImage(searchIconSource);
        }

        private void SearchTextBoxGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            var searchIconSource = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/search_normal.png");
            SearchIcon.Source = new BitmapImage(searchIconSource);
        }

        private void SearchCancelButton_Click(object sender, RoutedEventArgs e)
        {
            ClearSearchBox();
        }

        private void ClearSearchBox()
        {
            SearchTextBox.Text = "";
            Keyboard.Focus(SearchTextBox);
        }

    }
}
