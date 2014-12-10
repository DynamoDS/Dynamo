using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

using Dynamo.UI;
using Dynamo.Wpf.UI;
using Dynamo.Wpf.Utilities;
using InfoBubbleViewModel = Dynamo.ViewModels.InfoBubbleViewModel;
using Dynamo.ViewModels;
using Dynamo.Utilities;

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

        private InfoBubbleViewModel viewModel = null;

        public InfoBubbleViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                if (viewModel == null)
                {
                    viewModel = value;
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;
                    viewModel.RequestAction += InfoBubbleRequestAction;
                }
            }
        }

        private double contentMaxWidth;
        public double ContentMaxWidth
        {
            get { return contentMaxWidth; }
            set { contentMaxWidth = value; }
        }

        private double contentMaxHeight;
        public double ContentMaxHeight
        {
            get { return contentMaxHeight; }
            set { contentMaxHeight = value; }
        }

        private System.Windows.Thickness contentMargin;
        public System.Windows.Thickness ContentMargin
        {
            get { return contentMargin; }
            set { contentMargin = value; }
        }

        private double contentFontSize;
        public double ContentFontSize
        {
            get { return contentFontSize; }
            set { contentFontSize = value; }
        }

        private FontWeight contentFontWeight;
        public FontWeight ContentFontWeight
        {
            get { return contentFontWeight; }
            set { contentFontWeight = value; }
        }

        private SolidColorBrush contentForeground;
        public SolidColorBrush ContentForeground
        {
            get { return contentForeground; }
            set { contentForeground = value; }
        }

        private double preview_LastMaxWidth;
        private double preview_LastMaxHeight;

        // When a NodeModel is removed, WPF places the NodeView into a "disconnected"
        // state (i.e. NodeView.DataContext becomes "DisconnectedItem") before 
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
        /// Known usages (when this summary is written): DynamoView and NodeView (via DataTemplates.xaml)
        /// Till date there are 5 major types of info bubble
        /// 1. LibraryItemPreview:  Displayed when mouse hover over an item in the search view
        /// 2. NodeTooltip:         Displayed when mouse hover over the title area of a node
        /// 3. PreviewCondensed:    This is the default state when preview is shown.
        ///                         Displayed when mouse hover over the little triangle at the bottom of a node
        ///                         or
        ///                         when user chooses to show the preview
        /// 4. Preview:             Displayed when the node has a preview and mouse hover over the condensed preview        
        /// 5. ErrorCondensed:      This is the default state when error is shown.
        ///                         Displayed when errors exist for the node
        /// 6. Error:               Displayed when errors exist for the node and mouse hover over the condensed
        ///                         error
        /// </summary>
        public InfoBubbleView()
        {
            InitializeComponent();

            fadeInStoryBoard = (Storyboard)FindResource("fadeInStoryBoard");
            fadeOutStoryBoard = (Storyboard)FindResource("fadeOutStoryBoard");

            ContentFontSize = Configurations.PreviewTextFontSize;
            preview_LastMaxWidth = double.MaxValue;
            preview_LastMaxHeight = double.MaxValue;

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
                        UpdateStyle();
                        UpdateContent();
                        UpdateShape();
                        UpdatePosition();
                        break;
                }
            }
        }

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
            ViewModel = e.NewValue as InfoBubbleViewModel;
            UpdateContent();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // The fix for the following issue was previously performed in 
            // NodeModel. This is shifted over to infobubble to centralize 
            // the issue until code restructuring is completed.
            //
            // This is a temporarily measure, it work by dispatching the 
            // work to UI thread when info bubble UI values need to be 
            // modified by background evaluation thread.
            // To completely solve this, changes affecting UI values should be 
            // restructured into UI Binding in order for things to be thread 
            // safe. 
            // The above mentioned issue is being documented in:
            //
            //      http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-847
            //
            Action propertyChanged = (() =>
            {
                switch (e.PropertyName)
                {
                    case "Content":
                        UpdateStyle();
                        UpdateContent();
                        UpdateShape();
                        UpdatePosition();
                        break;

                    case "TargetTopLeft":
                    case "TargetBotRight":
                        UpdatePosition();
                        break;

                    case "ConnectingDirection":
                        UpdateShape();
                        UpdatePosition();
                        break;

                    case "InfoBubbleState":
                        UpdateShape();
                        UpdatePosition();

                        HandleInfoBubbleStateChanged(ViewModel.InfoBubbleState);
                        break;

                    case "InfoBubbleStyle":
                        UpdateStyle();
                        break;
                }
            });

            if (this.ViewModel.DynamoViewModel.UIDispatcher != null &&
                this.ViewModel.DynamoViewModel.UIDispatcher != null)
            {
                if (this.ViewModel.DynamoViewModel.UIDispatcher.CheckAccess())
                    propertyChanged();
                else
                    this.ViewModel.DynamoViewModel.UIDispatcher.BeginInvoke(propertyChanged);
            }
        }

        private void HandleInfoBubbleStateChanged(InfoBubbleViewModel.State state)
        {
            switch (state)
            {
                case InfoBubbleViewModel.State.Minimized:
                    // Changing to Minimized
                    this.HideInfoBubble();
                    break;

                case InfoBubbleViewModel.State.Pinned:
                    // Changing to Pinned
                    this.ShowInfoBubble();
                    break;
            }
        }

        #region Update Style

        private void UpdateStyle()
        {
            InfoBubbleViewModel.Style style = ViewModel.InfoBubbleStyle;
            ViewModel.LimitedDirection = InfoBubbleViewModel.Direction.None;

            switch (style)
            {
                case InfoBubbleViewModel.Style.Warning:
                    SetStyle_Warning();
                    break;
                case InfoBubbleViewModel.Style.WarningCondensed:
                    SetStyle_WarningCondensed();
                    break;
                case InfoBubbleViewModel.Style.Error:
                    SetStyle_Error();
                    break;
                case InfoBubbleViewModel.Style.ErrorCondensed:
                    SetStyle_ErrorCondensed();
                    break;
                case InfoBubbleViewModel.Style.None:
                    throw new ArgumentException("InfoWindow didn't have a style (456B24E0F400)");
            }
        }

        private void SetStyle_Warning()
        {
            backgroundPolygon.Fill = FrozenResources.WarningFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.ErrorFrameStrokeThickness;
            backgroundPolygon.Stroke = FrozenResources.WarningFrameStrokeColor;

            ContentContainer.MaxWidth = Configurations.ErrorMaxWidth;
            ContentContainer.MaxHeight = Configurations.ErrorMaxHeight;

            ContentMargin = Configurations.ErrorContentMargin.AsWindowsType();
            ContentMaxWidth = Configurations.ErrorContentMaxWidth;
            ContentMaxHeight = Configurations.ErrorContentMaxHeight;

            ContentFontSize = Configurations.ErrorTextFontSize;
            ContentForeground = FrozenResources.WarningTextForeground;
            ContentFontWeight = VisualConfigurations.ErrorTextFontWeight;
        }

        private void SetStyle_WarningCondensed()
        {
            backgroundPolygon.Fill = FrozenResources.WarningFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.ErrorFrameStrokeThickness;
            backgroundPolygon.Stroke = FrozenResources.WarningFrameStrokeColor;

            ContentContainer.MaxWidth = Configurations.ErrorCondensedMaxWidth;
            ContentContainer.MinWidth = Configurations.ErrorCondensedMinWidth;
            ContentContainer.MaxHeight = Configurations.ErrorCondensedMaxHeight;
            ContentContainer.MinHeight = Configurations.ErrorCondensedMinHeight;

            ContentMargin = Configurations.ErrorContentMargin.AsWindowsType();
            ContentMaxWidth = Configurations.ErrorCondensedContentMaxWidth;
            ContentMaxHeight = Configurations.ErrorCondensedContentMaxHeight;

            ContentFontSize = Configurations.ErrorTextFontSize;
            ContentForeground = FrozenResources.WarningTextForeground;
            ContentFontWeight = VisualConfigurations.ErrorTextFontWeight;
        }

        private void SetStyle_Error()
        {
            backgroundPolygon.Fill = FrozenResources.ErrorFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.ErrorFrameStrokeThickness;
            backgroundPolygon.Stroke = FrozenResources.ErrorFrameStrokeColor;

            ContentContainer.MaxWidth = Configurations.ErrorMaxWidth;
            ContentContainer.MaxHeight = Configurations.ErrorMaxHeight;

            ContentMargin = Configurations.ErrorContentMargin.AsWindowsType();
            ContentMaxWidth = Configurations.ErrorContentMaxWidth;
            ContentMaxHeight = Configurations.ErrorContentMaxHeight;

            ContentFontSize = Configurations.ErrorTextFontSize;
            ContentForeground = FrozenResources.ErrorTextForeground;
            ContentFontWeight = VisualConfigurations.ErrorTextFontWeight;
        }

        private void SetStyle_ErrorCondensed()
        {
            backgroundPolygon.Fill = FrozenResources.ErrorFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.ErrorFrameStrokeThickness;
            backgroundPolygon.Stroke = FrozenResources.ErrorFrameStrokeColor;

            ContentContainer.MaxWidth = Configurations.ErrorCondensedMaxWidth;
            ContentContainer.MinWidth = Configurations.ErrorCondensedMinWidth;
            ContentContainer.MaxHeight = Configurations.ErrorCondensedMaxHeight;
            ContentContainer.MinHeight = Configurations.ErrorCondensedMinHeight;

            ContentMargin = Configurations.ErrorContentMargin.AsWindowsType();
            ContentMaxWidth = Configurations.ErrorCondensedContentMaxWidth;
            ContentMaxHeight = Configurations.ErrorCondensedContentMaxHeight;

            ContentFontSize = Configurations.ErrorTextFontSize;
            ContentForeground = FrozenResources.ErrorTextForeground;
            ContentFontWeight = VisualConfigurations.ErrorTextFontWeight;
        }

        #endregion

        #region Update Content

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
                myGrid.HorizontalAlignment  = HorizontalAlignment.Stretch;
                myGrid.VerticalAlignment    = VerticalAlignment.Stretch;
                myGrid.Background   = Brushes.Transparent;
                myGrid.Margin       = ContentMargin;
                myGrid.MaxHeight    = ContentMaxHeight;
                myGrid.MaxWidth     = contentMaxWidth;

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
                string content = ViewModel.Content;
                if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.Warning)
                {
                    content = "Warning: " + content;
                }
                else if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.Error)
                {
                    content = "Error: " + content;
                }
                TextBox textBox = GetNewTextBox(content);
                ContentContainer.Children.Add(textBox);
            }
        }

        private TextBox GetNewTextBox(string text)
        {
            TextBox textBox = new TextBox();
            textBox.Text = text;
            textBox.TextWrapping = TextWrapping.Wrap;

            textBox.Margin      = ContentMargin;
            textBox.MaxHeight   = ContentMaxHeight;
            textBox.MaxWidth    = ContentMaxWidth;

            textBox.Foreground  = ContentForeground;
            textBox.FontWeight  = ContentFontWeight;
            textBox.FontSize    = ContentFontSize;

            var font = SharedDictionaryManager.DynamoModernDictionary["OpenSansRegular"];
            textBox.FontFamily = font as FontFamily;

            textBox.Background      = Brushes.Transparent;
            textBox.IsReadOnly      = true;
            textBox.BorderThickness = new System.Windows.Thickness(0);

            textBox.HorizontalAlignment = HorizontalAlignment.Center;
            textBox.VerticalAlignment   = VerticalAlignment.Center;

            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            return textBox;
        }

        #endregion

        #region Update Shape

        private void UpdateShape()
        {
            ContentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double estimatedHeight = ContentContainer.DesiredSize.Height;
            double estimatedWidth = ContentContainer.DesiredSize.Width;

            PointCollection framePoints = new PointCollection();
            switch (ViewModel.InfoBubbleStyle)
            {
                case InfoBubbleViewModel.Style.Warning:
                case InfoBubbleViewModel.Style.WarningCondensed:
                case InfoBubbleViewModel.Style.Error:
                case InfoBubbleViewModel.Style.ErrorCondensed:
                    framePoints = GetFramePoints_Error(estimatedHeight, estimatedWidth);
                    break;
                case InfoBubbleViewModel.Style.None:
                    break;
            }

            if (framePoints != null)
                backgroundPolygon.Points = framePoints;
        }

        private PointCollection GetFramePoints_NodeTooltipConnectBottom(double estimatedHeight, double estimatedWidth)
        {
            PointCollection pointCollection = new PointCollection();

            double arrowHeight = Configurations.NodeTooltipArrowHeight_SideConnecting;
            double arrowWidth = Configurations.NodeTooltipArrowWidth_SideConnecting;

            if (ViewModel.TargetTopLeft.Y - estimatedHeight >= 40)
            {
                arrowHeight = Configurations.NodeTooltipArrowHeight_BottomConnecting;
                arrowWidth = Configurations.NodeTooltipArrowWidth_BottomConnecting;

                ViewModel.LimitedDirection = InfoBubbleViewModel.Direction.None;
                pointCollection.Add(new Point(estimatedWidth, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, estimatedHeight - arrowHeight));
                pointCollection.Add(new Point((estimatedWidth / 2) - arrowWidth / 2, estimatedHeight - arrowHeight));
                pointCollection.Add(new Point(estimatedWidth / 2, estimatedHeight));
                pointCollection.Add(new Point((estimatedWidth / 2) + (arrowWidth / 2), estimatedHeight - arrowHeight));
                pointCollection.Add(new Point(estimatedWidth, estimatedHeight - arrowHeight));
            }
            else if (ViewModel.TargetBotRight.X + estimatedWidth <= this.ViewModel.DynamoViewModel.WorkspaceActualWidth)
            {
                ViewModel.LimitedDirection = InfoBubbleViewModel.Direction.Top;
                contentMargin = Configurations.NodeTooltipContentMarginLeft.AsWindowsType();
                //UpdateContent(Content);

                pointCollection.Add(new Point(estimatedWidth, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(arrowWidth, arrowHeight / 2));
                pointCollection.Add(new Point(arrowWidth, estimatedHeight));
                pointCollection.Add(new Point(estimatedWidth, estimatedHeight));
            }
            else
            {
                ViewModel.LimitedDirection = InfoBubbleViewModel.Direction.TopRight;
                contentMargin = Configurations.NodeTooltipContentMarginRight.AsWindowsType();
                //UpdateContent(Content);

                pointCollection.Add(new Point(estimatedWidth, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, estimatedHeight));
                pointCollection.Add(new Point(estimatedWidth - arrowWidth, estimatedHeight));
                pointCollection.Add(new Point(estimatedWidth - arrowWidth, arrowHeight / 2));

            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltipConnectLeft(double estimatedHeight, double estimatedWidth)
        {
            if (ViewModel.TargetBotRight.X + estimatedWidth > this.ViewModel.DynamoViewModel.WorkspaceActualWidth)
            {
                ViewModel.LimitedDirection = InfoBubbleViewModel.Direction.Right;
                contentMargin = Configurations.NodeTooltipContentMarginRight.AsWindowsType();

                return GeneratePointCollection_TooltipConnectRight(estimatedHeight, estimatedWidth);
            }
            else
                return GeneratePointCollection_TooltipConnectLeft(estimatedHeight, estimatedWidth);
        }

        private PointCollection GetFramePoints_NodeTooltipConnectRight(double estimatedHeight, double estimatedWidth)
        {
            if (ViewModel.TargetTopLeft.X - estimatedWidth < 0)
            {
                ViewModel.LimitedDirection = InfoBubbleViewModel.Direction.Left;
                contentMargin = Configurations.NodeTooltipContentMarginLeft.AsWindowsType();

                return GeneratePointCollection_TooltipConnectLeft(estimatedHeight, estimatedWidth);
            }
            else
                return GeneratePointCollection_TooltipConnectRight(estimatedHeight, estimatedWidth);
        }

        private PointCollection GeneratePointCollection_TooltipConnectLeft(double estimatedHeight, double estimatedWidth)
        {
            PointCollection pointCollection = new PointCollection();

            double arrowHeight = Configurations.NodeTooltipArrowHeight_SideConnecting;
            double arrowWidth = Configurations.NodeTooltipArrowWidth_SideConnecting;

            pointCollection.Add(new Point(estimatedWidth, 0));
            pointCollection.Add(new Point(arrowWidth, 0));
            pointCollection.Add(new Point(arrowWidth, estimatedHeight / 2 - arrowHeight / 2));
            pointCollection.Add(new Point(0, estimatedHeight / 2));
            pointCollection.Add(new Point(arrowWidth, estimatedHeight / 2 + arrowHeight / 2));
            pointCollection.Add(new Point(arrowWidth, estimatedHeight));
            pointCollection.Add(new Point(estimatedWidth, estimatedHeight));

            return pointCollection;
        }

        private PointCollection GeneratePointCollection_TooltipConnectRight(double estimatedHeight, double estimatedWidth)
        {
            PointCollection pointCollection = new PointCollection();

            double arrowHeight = Configurations.NodeTooltipArrowHeight_SideConnecting;
            double arrowWidth = Configurations.NodeTooltipArrowWidth_SideConnecting;

            pointCollection.Add(new Point(estimatedWidth - arrowWidth, 0));
            pointCollection.Add(new Point(0, 0));
            pointCollection.Add(new Point(0, estimatedHeight));
            pointCollection.Add(new Point(estimatedWidth - arrowWidth, estimatedHeight));
            pointCollection.Add(new Point(estimatedWidth - arrowWidth, estimatedHeight / 2 + arrowHeight / 2));
            pointCollection.Add(new Point(estimatedWidth, estimatedHeight / 2));
            pointCollection.Add(new Point(estimatedWidth - arrowWidth, estimatedHeight / 2 - arrowHeight / 2));

            return pointCollection;
        }

        private PointCollection GetFramePoints_Error(double estimatedHeight, double estimatedWidth)
        {
            double arrowHeight = Configurations.ErrorArrowHeight;
            double arrowWidth = Configurations.ErrorArrowWidth;

            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(estimatedWidth, 0));
            pointCollection.Add(new Point(0, 0));
            pointCollection.Add(new Point(0, estimatedHeight - arrowHeight));
            pointCollection.Add(new Point((estimatedWidth / 2) - arrowWidth / 2, estimatedHeight - arrowHeight));
            pointCollection.Add(new Point(estimatedWidth / 2, estimatedHeight));
            pointCollection.Add(new Point((estimatedWidth / 2) + arrowWidth / 2, estimatedHeight - arrowHeight));
            pointCollection.Add(new Point(estimatedWidth, estimatedHeight - arrowHeight));
            return pointCollection;
        }

        #endregion

        #region Update Position

        private void UpdatePosition()
        {
            ContentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double estimatedHeight = ContentContainer.DesiredSize.Height;
            double estimatedWidth = ContentContainer.DesiredSize.Width;

            switch (ViewModel.InfoBubbleStyle)
            {
                case InfoBubbleViewModel.Style.Warning:
                case InfoBubbleViewModel.Style.WarningCondensed:
                case InfoBubbleViewModel.Style.Error:
                case InfoBubbleViewModel.Style.ErrorCondensed:
                    mainGrid.Margin = GetMargin_Error(estimatedHeight, estimatedWidth);
                    break;
            }
        }

        private System.Windows.Thickness GetMargin_Error(double estimatedHeight, double estimatedWidth)
        {
            System.Windows.Thickness margin = new System.Windows.Thickness();
            double nodeWidth = ViewModel.TargetBotRight.X - ViewModel.TargetTopLeft.X;
            margin.Top = -(estimatedHeight) + ViewModel.TargetTopLeft.Y;
            margin.Left = -((estimatedWidth - nodeWidth) / 2) + ViewModel.TargetTopLeft.X;
            return margin;
        }

        #endregion

        #region Resize

        private void Resize(object parameter)
        {
            Point deltaPoint = (Point)parameter;

            double newMaxWidth = deltaPoint.X;
            double newMaxHeight = deltaPoint.Y;

            if (deltaPoint.X != double.MaxValue && newMaxWidth >= Configurations.PreviewMinWidth && newMaxWidth <= Configurations.PreviewMaxWidth)
            {
                ContentContainer.MaxWidth = newMaxWidth;
                contentMaxWidth = newMaxWidth - 10;
            }
            if (deltaPoint.Y != double.MaxValue && newMaxHeight >= Configurations.PreviewMinHeight)
            {
                ContentContainer.MaxHeight = newMaxHeight;
                contentMaxHeight = newMaxHeight - 17;
            }

            UpdateContent();
            UpdateShape();
            UpdatePosition();

            this.preview_LastMaxWidth = ContentContainer.MaxWidth;
            this.preview_LastMaxHeight = ContentContainer.MaxHeight;
        }

        #endregion

        private void ShowErrorBubbleFullContent()
        {
            if (this.IsDisconnected)
                return;

            InfoBubbleDataPacket data = new InfoBubbleDataPacket();
            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.ErrorCondensed)
            {
                data.Style = InfoBubbleViewModel.Style.Error;
            }
            else if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.WarningCondensed)
            {
                data.Style = InfoBubbleViewModel.Style.Warning;
            }
            data.ConnectingDirection = InfoBubbleViewModel.Direction.Bottom;

            this.ViewModel.ShowFullContentCommand.Execute(data);
        }

        private void ShowErrorBubbleCondensedContent()
        {
            if (this.IsDisconnected)
                return;

            InfoBubbleDataPacket data = new InfoBubbleDataPacket();
            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.Error)
            {
                data.Style = InfoBubbleViewModel.Style.ErrorCondensed;
            }
            else if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.Warning)
            {
                data.Style = InfoBubbleViewModel.Style.WarningCondensed;
            }
            data.ConnectingDirection = InfoBubbleViewModel.Direction.Bottom;

            this.ViewModel.ShowCondensedContentCommand.Execute(data);
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

            if (this.ViewModel.DynamoViewModel.IsMouseDown ||
                !this.ViewModel.DynamoViewModel.CurrentSpaceViewModel.CanShowInfoBubble)
                return;

            fadeOutStoryBoard.Stop(this);
            mainGrid.Visibility = Visibility.Visible;
            fadeInStoryBoard.Begin(this);
        }

        private void FadeOutInfoBubble()
        {
            if (this.IsDisconnected)
                return;

            if (this.ViewModel.InfoBubbleState == InfoBubbleViewModel.State.Pinned)
                return;

            fadeInStoryBoard.Stop(this);
            fadeOutStoryBoard.Begin(this);
        }

        #region Mouse Event Handlers

        private void ContentContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.IsDisconnected)
                return;
                
            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.ErrorCondensed ||
                ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.WarningCondensed)
                ShowErrorBubbleFullContent();
            
            ShowInfoBubble();

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
                case InfoBubbleViewModel.Style.Warning:
                case InfoBubbleViewModel.Style.Error:
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

            //ViewModel.ResizeCommand.Execute(mouseLocation);
            Resize(mouseLocation);
        }

        private void InfoBubble_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        #endregion
    }
}