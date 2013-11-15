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

            mainGrid.Opacity = Configurations.MaxOpacity;

            this.DataContextChanged += InfoBubbleView_DataContextChanged;
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
        private void FadeInInfoBubble(object sender, EventArgs e)
        {
            //Console.WriteLine("FadeIn start");

            fadeOutStoryBoard.Stop(this);
            mainGrid.Visibility = Visibility.Visible;
            fadeInStoryBoard.Begin(this);
        }

        private void FadeOutInfoBubble(object sender, EventArgs e)
        {
            //Console.WriteLine("FadeOut start");

            fadeInStoryBoard.Stop(this);
            mainGrid.Visibility = Visibility.Collapsed;
            fadeOutStoryBoard.Begin(this);
        }

        private void CountDownDoubleAnimation_Completed(object sender, EventArgs e)
        {
            fadeInStoryBoard.Stop(this);
            fadeOutStoryBoard.Stop(this);

            //Console.WriteLine("FadeOut done");
        }

        private void CountUpDoubleAnimation_Completed(object sender, EventArgs e)
        {
            //Console.WriteLine("FadeIn done");
        }
        #endregion

        #region Show/Hide Info Bubble
        // Show bubble instantly
        private void ShowInfoBubble(object sender, EventArgs e)
        {            
            mainGrid.Visibility = Visibility.Visible;
            // Run animation and skip it to end state i.e. MaxOpacity
            fadeInStoryBoard.Begin(this);
            fadeInStoryBoard.SkipToFill(this);
        }

        // Hide bubble instantly
        private void HideInfoBubble(object sender, EventArgs e)
        {
            mainGrid.Visibility = Visibility.Collapsed;
            fadeOutStoryBoard.Begin(this);
            fadeOutStoryBoard.SkipToFill(this);
        }
        #endregion

        private void InfoBubbleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is InfoBubbleViewModel)
            {
                (DataContext as InfoBubbleViewModel).PropertyChanged += ViewModel_PropertyChanged;
                (DataContext as InfoBubbleViewModel).FadeInInfoBubble += FadeInInfoBubble;
                (DataContext as InfoBubbleViewModel).FadeOutInfoBubble += FadeOutInfoBubble;
                (DataContext as InfoBubbleViewModel).ShowInfoBubble += ShowInfoBubble;
                (DataContext as InfoBubbleViewModel).HideInfoBubble += HideInfoBubble;
            }
            UpdateContent();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Content")
            {
                UpdateContent();
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

            TextBox textBox = GetNewTextBox(ViewModel.Content);
            ContentContainer.Children.Add(textBox);

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

            string content = ViewModel.FullContent;
            InfoBubbleViewModel.Style style = InfoBubbleViewModel.Style.Preview;
            InfoBubbleViewModel.Direction connectingDirection = InfoBubbleViewModel.Direction.Top;
            Point topLeft = ViewModel.TargetTopLeft;
            Point botRight = ViewModel.TargetBotRight;
            InfoBubbleDataPacket data = new InfoBubbleDataPacket(style, topLeft, botRight, content, connectingDirection);
            this.ViewModel.UpdateContentCommand.Execute(data);
            this.ViewModel.ZIndex = 5;
        }

        private void ShowPreviewBubbleCondensedContent()
        {
            if (this.IsDisconnected)
                return;

            string content = ViewModel.FullContent;
            InfoBubbleViewModel.Style style = InfoBubbleViewModel.Style.PreviewCondensed;
            InfoBubbleViewModel.Direction connectingDirection = InfoBubbleViewModel.Direction.Top;
            Point topLeft = ViewModel.TargetTopLeft;
            Point botRight = ViewModel.TargetBotRight;
            InfoBubbleDataPacket data = new InfoBubbleDataPacket(style, topLeft, botRight, content, connectingDirection);
            this.ViewModel.UpdateContentCommand.Execute(data);
            this.ViewModel.ZIndex = 3;
        }

        private InfoBubbleViewModel GetViewModel()
        {
            if (this.DataContext is InfoBubbleViewModel)
                return this.DataContext as InfoBubbleViewModel;
            else
                return null;
        }

        private void FadeInInfoBubble()
        {
            if (this.IsDisconnected)
                return;
                
            ViewModel.FadeInCommand.Execute(null);
        }

        private void FadeOutInfoBubble()
        {
            if (this.IsDisconnected)
                return;
                
            ViewModel.FadeOutCommand.Execute(null);
        }

        private void ContentContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.IsDisconnected)
                return;
                
            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.PreviewCondensed)
                ShowPreviewBubbleFullContent();
            
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

            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.Preview && ViewModel.IsShowPreviewByDefault)
                ShowPreviewBubbleCondensedContent();
            else
                FadeOutInfoBubble();

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

            if (ViewModel.IsShowPreviewByDefault)
            {
                ViewModel.IsShowPreviewByDefault = false;
                ViewModel.SetAlwaysVisibleCommand.Execute(false);
                ViewModel.InstantCollapseCommand.Execute(null);
            }
            else
            {
                ViewModel.IsShowPreviewByDefault = true;
                ViewModel.SetAlwaysVisibleCommand.Execute(true);
                ShowPreviewBubbleCondensedContent();
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