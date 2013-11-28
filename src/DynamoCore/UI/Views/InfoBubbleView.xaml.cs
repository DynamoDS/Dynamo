using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using InfoBubbleViewModel = Dynamo.ViewModels.InfoBubbleViewModel;
using Dynamo.ViewModels;
using Dynamo.Utilities;
using System.Diagnostics;
using Dynamo.Core;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for PreviewInfoBubble.xaml
    /// </summary>
    public partial class InfoBubbleView : UserControl
    {
        #region Properties

        private bool isResizing = false;
        private bool isResizeHeight = false;
        private bool isResizeWidth = false;

        public InfoBubbleViewModel ViewModel { get { return GetViewModel(); } }

        // When a NodeModel is removed, WPF places the dynNodeView into a "disconnected"
        // state (i.e. dynNodeView.DataContext becomes "DisconnectedItem") before 
        // eventually removing the view. This is the result of the host canvas being 
        // virtualized. This property is used by InfoBubbleView to determine if it should 
        // still continue to access the InfoBubbleViewModel that it is bound to.
        private bool IsDisconnected { get { return (this.ViewModel == null); } }

        #endregion

        #region Storyboards
        private Storyboard fadeInStoryBoard;
        private Storyboard fadeOutStoryBoard;
        #endregion

        /// <summary>
        /// Used to present useful/important information to user
        /// Known usages (when this summary is written): DynamoView and dynNodeView (via DataTemplates.xaml)
        /// Till date there are 5 major types of info bubble
        /// 1. LibraryItemPreview:  Displayed when mouse hover over an item in the search view
        /// 2. NodeTooltip:         Displayed when mouse hover over the title area of a node
        /// 3. PreviewCondensed:    This is the default state when preview is shown.
        ///                         Displayed when mouse hover over the little triangle at the bottom of a node
        ///                         or
        ///                         when user chooses to show the preview
        /// 4. Preview:             Displayed when the node has a preview and mouse hover over the condensed preview
        /// 5. Error:               Displayed when errors exist for the node
        /// </summary>
        public InfoBubbleView()
        {
            InitializeComponent();

            // Setup storyboard used for animating fading in and fading out of info bubble
            SetupFadeInStoryBoard();
            SetupFadeOutStoryBoard();

            this.DataContextChanged += InfoBubbleView_DataContextChanged;
        }

        private void InfoBubbleWindowUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                switch (ViewModel.InfoBubbleState)
                {
                    case InfoBubbleViewModel.State.Minimized:
                        mainGrid.Visibility = Visibility.Collapsed;
                        mainGrid.Opacity = 0;
                        break;
                    case InfoBubbleViewModel.State.Pinned:
                        mainGrid.Visibility = Visibility.Visible;
                        mainGrid.Opacity = Configurations.MaxOpacity;
                        break;
                }
            }
        }

        #region Setup animation storyboard
        private void SetupFadeInStoryBoard()
        {
            DoubleAnimation countUpDoubleAnimation = new DoubleAnimation();
            countUpDoubleAnimation.From = 0.0;
            countUpDoubleAnimation.To = Configurations.MaxOpacity;
            countUpDoubleAnimation.Duration =
                new Duration(TimeSpan.FromMilliseconds(Configurations.FadeInDurationInMilliseconds));
            countUpDoubleAnimation.FillBehavior = FillBehavior.HoldEnd;
            countUpDoubleAnimation.Completed += CountUpDoubleAnimation_Completed;

            fadeInStoryBoard = new Storyboard();
            fadeInStoryBoard.Children.Add(countUpDoubleAnimation);
            Storyboard.SetTargetName(countUpDoubleAnimation, mainGrid.Name);
            Storyboard.SetTargetProperty(countUpDoubleAnimation, new PropertyPath(Grid.OpacityProperty));
        }

        private void SetupFadeOutStoryBoard()
        {
            DoubleAnimation countDownDoubleAnimation = new DoubleAnimation();
            countDownDoubleAnimation.From = Configurations.MaxOpacity;
            countDownDoubleAnimation.To = 0.0;
            countDownDoubleAnimation.Duration =
                new Duration(TimeSpan.FromMilliseconds(Configurations.FadeOutDurationInMilliseconds));
            countDownDoubleAnimation.FillBehavior = FillBehavior.HoldEnd;
            countDownDoubleAnimation.Completed += CountDownDoubleAnimation_Completed;

            fadeOutStoryBoard = new Storyboard();
            fadeOutStoryBoard.Children.Add(countDownDoubleAnimation);
            Storyboard.SetTargetName(countDownDoubleAnimation, mainGrid.Name);
            Storyboard.SetTargetProperty(countDownDoubleAnimation, new PropertyPath(Grid.OpacityProperty));
        }
        #endregion

        #region FadeIn FadeOut Event Handling

        private void CountDownDoubleAnimation_Completed(object sender, EventArgs e)
        {
            //Console.WriteLine("FadeOut done");
            fadeInStoryBoard.Stop(this);
            fadeOutStoryBoard.Stop(this);

            mainGrid.Opacity = 0;
            mainGrid.Visibility = Visibility.Collapsed;
        }

        private void CountUpDoubleAnimation_Completed(object sender, EventArgs e)
        {
            //Console.WriteLine("FadeIn done");
            mainGrid.Opacity = Configurations.MaxOpacity;
            mainGrid.Visibility = Visibility.Visible;
        }
        #endregion

        private void InfoBubbleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is InfoBubbleViewModel)
            {
                (DataContext as InfoBubbleViewModel).PropertyChanged += ViewModel_PropertyChanged;
                (DataContext as InfoBubbleViewModel).RequestAction += InfoBubbleRequestAction;
            }
            UpdateContent();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Content":
                    UpdateContent();
                    break;
            }
        }

        private void UpdateContent()
        {
            //The reason of changing the content from the code behind like this is due to a bug of WPF
            //  The bug if when you set the max width of an existing text box and then try to get the 
            //  expected size of it by using TextBox.Measure(..) method it will return the wrong value.
            //  The only solution that I can come up for now is clean the StackPanel content and 
            //  then add a new TextBox to it

            ContentContainer.Children.Clear();

            if (ViewModel == null) return;


            if (ViewModel.Content == "...")
            {
                #region Draw Icon
                Rectangle r1 = new Rectangle();
                r1.Fill = Brushes.Black;
                r1.Height = 1;
                r1.Width = 16;
                r1.UseLayoutRounding = true;

                Rectangle r2 = new Rectangle();
                r2.Fill = Brushes.Black;
                r2.Height = 1;
                r2.Width = 16;
                r2.UseLayoutRounding = true;

                Rectangle r3 = new Rectangle();
                r3.Fill = Brushes.Black;
                r3.Height = 1;
                r3.Width = 10;
                r3.UseLayoutRounding = true;
                r3.HorizontalAlignment = HorizontalAlignment.Left;

                Grid myGrid = new Grid();
                myGrid.Background = Brushes.Transparent;
                myGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                myGrid.VerticalAlignment = VerticalAlignment.Stretch;

                // Create row definitions.
                RowDefinition rowDefinition1 = new RowDefinition();
                RowDefinition rowDefinition2 = new RowDefinition();
                RowDefinition rowDefinition3 = new RowDefinition();
                rowDefinition1.Height = new GridLength(3);
                rowDefinition2.Height = new GridLength(3);
                rowDefinition3.Height = new GridLength(3);

                myGrid.RowDefinitions.Add(rowDefinition1);
                myGrid.RowDefinitions.Add(rowDefinition2);
                myGrid.RowDefinitions.Add(rowDefinition3);
                myGrid.Children.Add(r1);
                Grid.SetRow(r1, 0);
                myGrid.Children.Add(r2);
                Grid.SetRow(r2, 1);
                myGrid.Children.Add(r3);
                Grid.SetRow(r3, 2);
                myGrid.UseLayoutRounding = true;

                ContentContainer.Children.Add(myGrid);
                #endregion
            }
            else
            {
                TextBox textBox = GetNewTextBox(ViewModel.Content);
                ContentContainer.Children.Add(textBox);
            }

            ContentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            ViewModel.EstimatedWidth = ContentContainer.DesiredSize.Width;
            ViewModel.EstimatedHeight = ContentContainer.DesiredSize.Height;
        }

        private TextBox GetNewTextBox(string text)
        {
            TextBox textBox = new TextBox();
            textBox.Text = text;
            return textBox;
        }

        private void ShowPreviewBubbleFullContent()
        {
            if (this.IsDisconnected)
                return;
            
            InfoBubbleDataPacket data = new InfoBubbleDataPacket();
            data.Style = InfoBubbleViewModel.Style.Preview;
            data.ConnectingDirection = InfoBubbleViewModel.Direction.Top;

            this.ViewModel.ShowFullContentCommand.Execute(data);
        }

        private void ShowPreviewBubbleCondensedContent()
        {
            if (this.IsDisconnected)
                return;

            InfoBubbleDataPacket data = new InfoBubbleDataPacket();
            data.Style = InfoBubbleViewModel.Style.PreviewCondensed;
            data.ConnectingDirection = InfoBubbleViewModel.Direction.Top;

            this.ViewModel.ShowCondensedContentCommand.Execute(data);
        }

        private void ShowErrorBubbleFullContent()
        {
            if (this.IsDisconnected)
                return;

            InfoBubbleDataPacket data = new InfoBubbleDataPacket();
            data.Style = InfoBubbleViewModel.Style.Error;
            data.ConnectingDirection = InfoBubbleViewModel.Direction.Bottom;

            this.ViewModel.ShowFullContentCommand.Execute(data);
        }

        private void ShowErrorBubbleCondensedContent()
        {
            if (this.IsDisconnected)
                return;

            InfoBubbleDataPacket data = new InfoBubbleDataPacket();
            data.Style = InfoBubbleViewModel.Style.ErrorCondensed;
            data.ConnectingDirection = InfoBubbleViewModel.Direction.Bottom;

            this.ViewModel.ShowCondensedContentCommand.Execute(data);
        }

        private InfoBubbleViewModel GetViewModel()
        {
            if (this.DataContext is InfoBubbleViewModel)
                return this.DataContext as InfoBubbleViewModel;
            else
                return null;
        }

        private void InfoBubbleRequestAction(object sender, InfoBubbleEventArgs e)
        {
            switch (e.RequestType)
            {
                case InfoBubbleEventArgs.Request.Show:
                    ShowInfoBubble();
                    break;
                case InfoBubbleEventArgs.Request.Hide:
                    HideInfoBubble();
                    break;
                case InfoBubbleEventArgs.Request.FadeIn:
                    FadeInInfoBubble();
                    break;
                case InfoBubbleEventArgs.Request.FadeOut:
                    FadeOutInfoBubble();
                    break;
            }
        }

        private void ShowInfoBubble()
        {
            if (mainGrid.Visibility == System.Windows.Visibility.Collapsed)
            {
                mainGrid.Visibility = Visibility.Visible;
                // Run animation and skip it to end state i.e. MaxOpacity
                fadeInStoryBoard.Begin(this);
                fadeInStoryBoard.SkipToFill(this);
            }
        }

        // Hide bubble instantly
        private void HideInfoBubble()
        {
            if (mainGrid.Visibility == System.Windows.Visibility.Visible)
            {
                mainGrid.Visibility = Visibility.Collapsed;
                fadeOutStoryBoard.Begin(this);
                fadeOutStoryBoard.SkipToFill(this);
            }
        }

        private void FadeInInfoBubble()
        {
            if (this.IsDisconnected)
                return;

            if (dynSettings.Controller.DynamoViewModel.IsMouseDown ||
                !dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.CanShowInfoBubble)
                return;

            fadeOutStoryBoard.Stop(this);
            mainGrid.Visibility = Visibility.Visible;
            fadeInStoryBoard.Begin(this);
        }

        private void FadeOutInfoBubble()
        {
            if (this.IsDisconnected || (this.ViewModel.InfoBubbleState == InfoBubbleViewModel.State.Pinned))
                return;

            fadeInStoryBoard.Stop(this);
            mainGrid.Visibility = Visibility.Collapsed;
            fadeOutStoryBoard.Begin(this);
        }

        private void ContentContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.IsDisconnected)
                return;
                
            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.PreviewCondensed)
                ShowPreviewBubbleFullContent();
            else if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.ErrorCondensed)
                ShowErrorBubbleFullContent();
            
            FadeInInfoBubble();

            this.Cursor = CursorLibrary.GetCursor(CursorSet.Pointer);
        }

        private void InfoBubble_MouseLeave(object sender, MouseEventArgs e)
        {
            // It is possible for MouseLeave message (that was scheduled earlier) to reach
            // InfoBubbleView when it becomes disconnected from InfoBubbleViewModel (i.e. 
            // when the NodeModel it belongs is deleted by user). In this case, InfoBubbleView
            // should simply ignore the message, since the node is no longer valid.
            if (this.IsDisconnected)
                return;

            switch (ViewModel.InfoBubbleStyle)
            {
                case InfoBubbleViewModel.Style.Preview:
                    if (ViewModel.InfoBubbleState == InfoBubbleViewModel.State.Pinned)
                        ShowPreviewBubbleCondensedContent();
                    else
                        goto default;
                    break;

                case InfoBubbleViewModel.Style.Error:
                case InfoBubbleViewModel.Style.ErrorCondensed:
                    ShowErrorBubbleCondensedContent();
                    break;

                default:
                    FadeOutInfoBubble();
                    break;
            }

            this.Cursor = CursorLibrary.GetCursor(CursorSet.Pointer);
        }

        private void InfoBubble_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = CursorLibrary.GetCursor(CursorSet.Condense);
        }

        private void InfoBubble_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.IsDisconnected)
                return;

            if (ViewModel.InfoBubbleStyle != InfoBubbleViewModel.Style.Preview && ViewModel.InfoBubbleStyle != InfoBubbleViewModel.Style.PreviewCondensed)
                return;

            switch (ViewModel.InfoBubbleState)
            {
                case InfoBubbleViewModel.State.Minimized:
                    ViewModel.ChangeInfoBubbleStateCommand.Execute(InfoBubbleViewModel.State.Pinned);
                    ShowPreviewBubbleCondensedContent();
                    break;

                case InfoBubbleViewModel.State.Pinned:
                    ViewModel.ChangeInfoBubbleStateCommand.Execute(InfoBubbleViewModel.State.Minimized);
                    this.HideInfoBubble();
                    break;
            }
        }

        private void ResizeObject_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = null;
        }

        private void ResizeObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            isResizing = false;
            isResizeHeight = false;
            isResizeWidth = false;
        }

        private void MainGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsDisconnected)
                return;

            if (!isResizing)
                return;

            Point mouseLocation = Mouse.GetPosition(mainGrid);
            if (!isResizeHeight)
                mouseLocation.Y = double.MaxValue;
            if (!isResizeWidth)
                mouseLocation.X = double.MaxValue;

            ViewModel.ResizeCommand.Execute(mouseLocation);
        }

        private void HorizontalResizeBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = CursorLibrary.GetCursor(CursorSet.ResizeVertical);
        }

        private void HorizontalResizeBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(sender as UIElement);
            e.Handled = true;

            isResizing = true;
            isResizeHeight = true;
        }

        private void ConnerResizePoint_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = CursorLibrary.GetCursor(CursorSet.ResizeDiagonal);
        }

        private void ConnerResizePoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(sender as UIElement);
            e.Handled = true;

            isResizing = true;
            isResizeWidth = true;
            isResizeHeight = true;
        }

        private void VerticalResizeBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(sender as UIElement);
            e.Handled = true;

            isResizing = true;
            isResizeWidth = true;
        }

        private void VerticalResizeBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = CursorLibrary.GetCursor(CursorSet.ResizeHorizontal);
        }

        private void InfoBubble_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void InfoBubble_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsDisconnected)
                return;

            Point mousePosition = e.GetPosition(this);

            double offsetX = this.ActualWidth - ViewModel.EstimatedWidth;
            double offsetY = this.ActualHeight - ViewModel.EstimatedHeight;
            if (Math.Abs(mousePosition.X - offsetX - ViewModel.EstimatedWidth / 2) < 25
                && (mousePosition.Y - offsetY < 20)
                && (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.Preview))
            {
                this.Cursor = CursorLibrary.GetCursor(CursorSet.Expand);
            }
            else if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.PreviewCondensed)
                this.Cursor = CursorLibrary.GetCursor(CursorSet.Condense);
            else
                this.Cursor = null;
        }

    }
}