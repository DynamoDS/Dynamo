using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Dynamo.Controls;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;

using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Dynamo.Search
{
    /// <summary>
    ///     Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        private readonly SearchViewModel viewModel;
        private readonly DynamoViewModel dynamoViewModel;

        private const string baseUrl = @"pack://application:,,,/DynamoCoreWpf;component/UI/Images/";

        private BitmapImage searchIconBitmapNormal =
            new BitmapImage(new Uri(baseUrl + "search_normal.png"));
        private BitmapImage searchIconBitmapHover =
            new BitmapImage(new Uri(baseUrl + "search_hover.png"));

        private SolidColorBrush searchForegroundBrushNormal =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#878787"));
        private SolidColorBrush searchForegroundBrushHover =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));

        public SearchView(SearchViewModel searchViewModel, DynamoViewModel dynamoViewModel)
        {
            viewModel = searchViewModel;
            this.dynamoViewModel = dynamoViewModel;

            DataContext = viewModel;
            InitializeComponent();
            Loaded += OnSearchViewLoaded;
            Unloaded += OnSearchViewUnloaded;

            SearchTextBox.IsVisibleChanged += delegate
            {
                if (SearchTextBox.IsVisible)
                {
                    this.viewModel.SearchCommand.Execute(null);
                    Keyboard.Focus(this.SearchTextBox);
                    var view = WpfUtilities.FindUpVisualTree<DynamoView>(this);
                    SearchTextBox.InputBindings.AddRange(view.InputBindings);
                    SearchTextBlock.Text = Properties.Resources.SearchTextBlockText;
                }
            };

            searchForegroundBrushNormal.Freeze();
            searchForegroundBrushHover.Freeze();
        }

        private void OnSearchViewUnloaded(object sender, EventArgs e)
        {
            viewModel.RequestFocusSearch -= OnSearchViewModelRequestFocusSearch;
            viewModel.RequestReturnFocusToSearch -= OnSearchViewModelRequestReturnFocusToSearch;
        }

        private void OnSearchViewLoaded(object sender, RoutedEventArgs e)
        {
            MouseEnter += OnSearchViewMouseEnter;
            MouseLeave += OnSearchViewMouseLeave;

            PreviewKeyDown += KeyHandler;
            SearchTextBox.PreviewKeyDown += OnSearchBoxPreviewKeyDown;
            SearchTextBox.KeyDown += OnSearchBoxKeyDown;


            viewModel.RequestFocusSearch += OnSearchViewModelRequestFocusSearch;
            viewModel.RequestReturnFocusToSearch += OnSearchViewModelRequestReturnFocusToSearch;

            this.librarySearchView.SearchTextBox = SearchTextBox;
        }

        private void OnSearchViewMouseLeave(object sender, MouseEventArgs e)
        {
            viewModel.SearchScrollBarVisibility = false;
        }

        private void OnSearchViewMouseEnter(object sender, MouseEventArgs e)
        {
            viewModel.SearchScrollBarVisibility = true;
        }

        private void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            bool handleIt = false;
            e.Handled = handleIt;

            if (e.Key == Key.Escape)
            {
                ClearSearchBox();
            }
        }

        private void OnSearchBoxPreviewKeyDown(object sender, KeyEventArgs e)
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
                case Key.Delete:
                    if (DynamoSelection.Instance.Selection.Count > 0)
                    {
                        e.Handled = true;
                        dynamoViewModel.DeleteCommand.Execute(null);
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
                case Key.Up:
                case Key.Enter:
                    if (viewModel.CurrentMode == SearchViewModel.ViewMode.LibrarySearchView)
                        librarySearchView.SelectNext(e.Key);
                    break;
            }
        }

        private void OnSearchViewModelRequestReturnFocusToSearch(object sender, EventArgs e)
        {
            if (Visibility != Visibility.Collapsed)
                Keyboard.Focus(SearchTextBox);
            else
                MoveFocusToNextUIElement();
        }

        private void MoveFocusToNextUIElement()
        {
            // Gets the element with keyboard focus.
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

            // Change keyboard focus.
            if (elementWithFocus != null)
                elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void OnSearchViewModelRequestFocusSearch(object sender, EventArgs e)
        {
            SearchTextBox.Focus();
        }

        public void OnSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();

            viewModel.SearchCommand.Execute(null);
        }

        // Not used anywhere.
        public void ListBoxItem_Click(object sender, RoutedEventArgs e)
        {
            ((ListBoxItem)sender).IsSelected = true;
            Keyboard.Focus(SearchTextBox);
        }

        // Not used anywhere.
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel)DataContext).RemoveLastPartOfSearchText();
            Keyboard.Focus(SearchTextBox);
        }

        private void OnTreeViewScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnLibraryExpanderClick(object sender, RoutedEventArgs e)
        {
            //this.Width = 5;
            //if (this.Visibility == Visibility.Collapsed)
            //    this.Visibility = Visibility.Visible;
            //else
            //{
            //    dynamoModel.DynamoViewModel.OnSidebarClosed(this, EventArgs.Empty);
            //   this.Visibility = Visibility.Collapsed;
            //}
            dynamoViewModel.OnSidebarClosed(this, EventArgs.Empty);
        }

        private void OnLibraryExpanderMouseEnter(object sender, MouseEventArgs e)
        {
            Button b = (Button)sender;
            Grid g = (Grid)b.Parent;
            Label lb = (Label)(g.Children[0]);
            var bc = new BrushConverter();
            lb.Foreground = (Brush)bc.ConvertFromString("#cccccc");
            Image collapsestate = (Image)(b).Content;
            var collapsestateSource = new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/expand_hover.png");
            BitmapImage bmi = new BitmapImage(collapsestateSource);
            RotateTransform rotateTransform = new RotateTransform(-90, 16, 16);
            collapsestate.Source = new BitmapImage(collapsestateSource);

            Cursor = CursorLibrary.GetCursor(CursorSet.LinkSelect);
        }

        private void OnLibraryExpanderMouseLeave(object sender, MouseEventArgs e)
        {
            Button b = (Button)sender;
            Grid g = (Grid)b.Parent;
            Label lb = (Label)(g.Children[0]);
            var bc = new BrushConverter();
            lb.Foreground = (Brush)bc.ConvertFromString("#aaaaaa");
            Image collapsestate = (Image)(b).Content;
            var collapsestateSource = new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/expand_normal.png");
            collapsestate.Source = new BitmapImage(collapsestateSource);

            Cursor = null;
        }

        private void OnSearchTextBoxGridMouseEnter(object sender, MouseEventArgs e)
        {
            SearchIcon.Source = searchIconBitmapHover;
            SearchTextBlock.Foreground = searchForegroundBrushHover;
            SearchTextBlock.Text = Properties.Resources.SearchTextBlockText;
        }

        private void OnSearchTextBoxGridMouseLeave(object sender, MouseEventArgs e)
        {
            SearchIcon.Source = searchIconBitmapNormal;
            SearchTextBlock.Foreground = searchForegroundBrushNormal;
            SearchTextBlock.Text = Properties.Resources.SearchTextBlockText;
        }

        private void OnSearchCancelButtonClick(object sender, RoutedEventArgs e)
        {
            ClearSearchBox();
        }

        private void ClearSearchBox()
        {
            SearchTextBox.Text = "";
            Keyboard.Focus(SearchTextBox);
        }

        private void TextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (viewModel != null)
                viewModel.SearchIconAlignment = System.Windows.HorizontalAlignment.Left;
        }

        private void TextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (viewModel != null)
            {
                if (string.IsNullOrEmpty(viewModel.SearchText))
                    viewModel.SearchIconAlignment = System.Windows.HorizontalAlignment.Center;
                else
                    viewModel.SearchIconAlignment = System.Windows.HorizontalAlignment.Left;
            }
        }

        private void OnLibraryViewPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            SearchTextBox.Text = "";
        }


        /// <summary>
        /// On drag&drop starts change cursor to cursor, that is shown when the user is hovering over the workspace.
        /// </summary>
        private void OnLibraryContainerViewGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = e.Effects.HasFlag(DragDropEffects.Copy) || e.Effects.HasFlag(DragDropEffects.Move);

            if (!e.UseDefaultCursors)
                Mouse.SetCursor(CursorLibrary.GetCursor(CursorSet.DragMove));

            e.Handled = true;
        }
    }
}
