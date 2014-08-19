using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibraryContainerView.xaml
    /// </summary>
    public partial class LibraryContainerView : UserControl
    {
        //TODO: use LibraryContainerViewModel when it will be ready
        private readonly SearchViewModel viewModel;
        private readonly DynamoViewModel dynamoViewModel;

        public LibraryContainerView(SearchViewModel searchViewModel, DynamoViewModel dynamoViewModel)
        {
            this.viewModel = searchViewModel;
            this.dynamoViewModel = dynamoViewModel;

            InitializeComponent();

            this.Loaded += LibraryContainerViewLoaded;
            this.Dispatcher.ShutdownStarted += DispatcherShutdownStarted;
        }

        #region LibraryContainerView event handlers

        private void LibraryContainerViewLoaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this.viewModel;

            this.MouseEnter += LibraryContainerViewMouseEnter;
            this.MouseLeave += LibraryContainerViewMouseLeave;
            this.PreviewKeyDown += LibraryContainerViewKeyHandler;

            this.viewModel.RequestFocusSearch += SearchViewModelRequestFocusSearch;
            this.viewModel.RequestReturnFocusToSearch += SearchViewModelRequestReturnFocusToSearch;
        }

        private void LibraryContainerViewMouseLeave(object sender, MouseEventArgs e)
        {
            viewModel.SearchScrollBarVisibility = false;
        }

        private void LibraryContainerViewMouseEnter(object sender, MouseEventArgs e)
        {
            viewModel.SearchScrollBarVisibility = true;
        }

        /// <summary>
        ///     A KeyHandler method used by SearchView, increments decrements and executes based on input.
        /// </summary>
        /// <param name="sender">Originating object for the KeyHandler </param>
        /// <param name="e">Parameters describing the key push</param>
        private void LibraryContainerViewKeyHandler(object sender, KeyEventArgs e)
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
                    viewModel.Execute();
                    break;

                case Key.Delete:
                    if (DynamoSelection.Instance.Selection.Count > 0)
                    {
                        e.Handled = true;
                        this.dynamoViewModel.DeleteCommand.Execute(null);
                    }

                    //if there are no nodes being selected, the delete key should 
                    //delete the text in the search box of library preview
                    else
                    {

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
                    viewModel.PopulateSearchTextWithSelectedResult();
                    break;

                case Key.Down:
                    viewModel.SelectNext();
                    break;

                case Key.Up:
                    viewModel.SelectPrevious();
                    break;
            }
        }

        #endregion

        #region SearchViewModel event handlers

        private void SearchViewModelRequestFocusSearch(object sender, EventArgs e)
        {
            SearchTextBox.Focus();
        }

        private void SearchViewModelRequestReturnFocusToSearch(object sender, EventArgs e)
        {
            if (this.Visibility != Visibility.Collapsed)
                Keyboard.Focus(SearchTextBox);
            else
                MoveFocusToNextUIElement();
        }

        #endregion

        #region Library Expander event handlers

        private void LibraryExpanderMouseEnter(object sender, MouseEventArgs e)
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

        private void LibraryExpanderMouseLeave(object sender, MouseEventArgs e)
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

        private void LibraryExpanderClick(object sender, RoutedEventArgs e)
        {
            this.dynamoViewModel.OnSidebarClosed(this, EventArgs.Empty);
        }

        #endregion

        #region SearchTextBox Grid event handlers

        private void SearchTextBoxGridMouseEnter(object sender, MouseEventArgs e)
        {
            var searchIconSource = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/search_hover.png");
            SearchIcon.Source = new BitmapImage(searchIconSource);
        }

        private void SearchTextBoxGridMouseLeave(object sender, MouseEventArgs e)
        {
            var searchIconSource = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/search_normal.png");
            SearchIcon.Source = new BitmapImage(searchIconSource);
        }

        #endregion

        #region SearchTextBox event handlers

        private void SearchTextBoxIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (SearchTextBox.IsVisible)
            {
                this.viewModel.SearchCommand.Execute(null);
                Keyboard.Focus(this.SearchTextBox);
                var view = Application.Current.MainWindow;
                SearchTextBox.InputBindings.AddRange(view.InputBindings);
            }
        }

        private void SearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();

            this.viewModel.SearchCommand.Execute(null);
        }

        private void SearchTextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (viewModel != null)
                viewModel.SearchIconAlignment = System.Windows.HorizontalAlignment.Left;
        }

        private void SearchTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (viewModel != null)
            {
                if (string.IsNullOrEmpty(viewModel.SearchText))
                    viewModel.SearchIconAlignment = System.Windows.HorizontalAlignment.Center;
                else
                    viewModel.SearchIconAlignment = System.Windows.HorizontalAlignment.Left;
            }
        }

        private void SearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            bool handleIt = false;
            e.Handled = handleIt;

            if (e.Key == Key.Escape)
            {
                ClearSearchBox();
            }
        }

        private void SearchTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool handleIt = false;
            e.Handled = handleIt;
        }

        #endregion

        private void SearchCancelButtonClick(object sender, RoutedEventArgs e)
        {
            ClearSearchBox();
        }

        private void DispatcherShutdownStarted(object sender, EventArgs e)
        {
            this.viewModel.RequestFocusSearch -= SearchViewModelRequestFocusSearch;
            this.viewModel.RequestReturnFocusToSearch -= SearchViewModelRequestReturnFocusToSearch;
        }

        private void MoveFocusToNextUIElement()
        {
            // Gets the element with keyboard focus.
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

            // Change keyboard focus.
            if (elementWithFocus != null)
                elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void ClearSearchBox()
        {
            SearchTextBox.Text = "";
            Keyboard.Focus(SearchTextBox);
        }
    }
}
