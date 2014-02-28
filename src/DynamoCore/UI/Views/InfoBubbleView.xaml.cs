﻿using System;
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
using Dynamo.UI;

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

        private Thickness contentMargin;
        public Thickness ContentMargin
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

            if (dynSettings.Controller != null &&
                dynSettings.Controller.UIDispatcher != null)
            {
                if (dynSettings.Controller.UIDispatcher.CheckAccess())
                    propertyChanged();
                else
                    dynSettings.Controller.UIDispatcher.BeginInvoke(propertyChanged);
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
                case InfoBubbleViewModel.Style.LibraryItemPreview:
                    SetStyle_LibraryItemPreview();
                    break;
                case InfoBubbleViewModel.Style.NodeTooltip:
                    SetStyle_NodeTooltip(ViewModel.ConnectingDirection);
                    break;
                case InfoBubbleViewModel.Style.Error:
                    SetStyle_Error();
                    break;
                case InfoBubbleViewModel.Style.ErrorCondensed:
                    SetStyle_ErrorCondensed();
                    break;
                case InfoBubbleViewModel.Style.Preview:
                    SetStyle_Preview();
                    break;
                case InfoBubbleViewModel.Style.PreviewCondensed:
                    SetStyle_PreviewCondensed();
                    break;
                case InfoBubbleViewModel.Style.None:
                    throw new ArgumentException("InfoWindow didn't have a style (456B24E0F400)");
            }
        }

        private void SetStyle_LibraryItemPreview()
        {
            backgroundPolygon.Fill = Configurations.LibraryTooltipFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.LibraryTooltipFrameStrokeThickness;
            backgroundPolygon.Stroke = Configurations.LibraryTooltipFrameStrokeColor;

            ContentContainer.MaxWidth = Configurations.LibraryTooltipMaxWidth;
            ContentContainer.MaxHeight = Configurations.LibraryTooltipMaxHeight;

            ContentMargin = Configurations.LibraryTooltipContentMargin;
            ContentMaxWidth = Configurations.LibraryTooltipContentMaxWidth;
            ContentMaxHeight = Configurations.LibraryTooltipContentMaxHeight;

            ContentFontSize = Configurations.LibraryTooltipTextFontSize;
            ContentForeground = Configurations.LibraryTooltipTextForeground;
            ContentFontWeight = Configurations.LibraryTooltipTextFontWeight;
        }

        private void SetStyle_NodeTooltip(InfoBubbleViewModel.Direction connectingDirection)
        {
            backgroundPolygon.Fill = Configurations.NodeTooltipFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.NodeTooltipFrameStrokeThickness;
            backgroundPolygon.Stroke = Configurations.NodeTooltipFrameStrokeColor;

            ContentContainer.MaxWidth = Configurations.NodeTooltipMaxWidth;
            ContentContainer.MaxHeight = Configurations.NodeTooltipMaxHeight;

            ContentMaxWidth = Configurations.NodeTooltipContentMaxWidth;
            ContentMaxHeight = Configurations.NodeTooltipContentMaxHeight;

            ContentFontSize = Configurations.NodeTooltipTextFontSize;
            ContentForeground = Configurations.NodeTooltipTextForeground;
            ContentFontWeight = Configurations.NodeTooltipTextFontWeight;

            switch (connectingDirection)
            {
                case InfoBubbleViewModel.Direction.Left:
                    ContentMargin = Configurations.NodeTooltipContentMarginLeft;
                    break;
                case InfoBubbleViewModel.Direction.Right:
                    ContentMargin = Configurations.NodeTooltipContentMarginRight;
                    break;
                case InfoBubbleViewModel.Direction.Bottom:
                    ContentMargin = Configurations.NodeTooltipContentMarginBottom;
                    break;
            }
        }

        private void SetStyle_Error()
        {
            backgroundPolygon.Fill = Configurations.ErrorFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.ErrorFrameStrokeThickness;
            backgroundPolygon.Stroke = Configurations.ErrorFrameStrokeColor;

            ContentContainer.MaxWidth = Configurations.ErrorMaxWidth;
            ContentContainer.MaxHeight = Configurations.ErrorMaxHeight;

            ContentMargin = Configurations.ErrorContentMargin;
            ContentMaxWidth = Configurations.ErrorContentMaxWidth;
            ContentMaxHeight = Configurations.ErrorContentMaxHeight;

            ContentFontSize = Configurations.ErrorTextFontSize;
            ContentForeground = Configurations.ErrorTextForeground;
            ContentFontWeight = Configurations.ErrorTextFontWeight;
        }

        private void SetStyle_ErrorCondensed()
        {
            backgroundPolygon.Fill = Configurations.ErrorFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.ErrorFrameStrokeThickness;
            backgroundPolygon.Stroke = Configurations.ErrorFrameStrokeColor;

            ContentContainer.MaxWidth = Configurations.ErrorCondensedMaxWidth;
            ContentContainer.MinWidth = Configurations.ErrorCondensedMinWidth;
            ContentContainer.MaxHeight = Configurations.ErrorCondensedMaxHeight;
            ContentContainer.MinHeight = Configurations.ErrorCondensedMinHeight;

            ContentMargin = Configurations.ErrorContentMargin;
            ContentMaxWidth = Configurations.ErrorCondensedContentMaxWidth;
            ContentMaxHeight = Configurations.ErrorCondensedContentMaxHeight;

            ContentFontSize = Configurations.ErrorTextFontSize;
            ContentForeground = Configurations.ErrorTextForeground;
            ContentFontWeight = Configurations.ErrorTextFontWeight;
        }

        private void SetStyle_Preview()
        {
            backgroundPolygon.Fill = Configurations.PreviewFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.PreviewFrameStrokeThickness;
            backgroundPolygon.Stroke = Configurations.PreviewFrameStrokeColor;

            ContentFontSize = Configurations.PreviewTextFontSize;
            ContentForeground = Configurations.PreviewTextForeground;
            ContentFontWeight = Configurations.PreviewTextFontWeight;

            ContentMargin = Configurations.PreviewContentMargin;

            if (this.preview_LastMaxHeight == double.MaxValue)
            {
                ContentContainer.MaxHeight = Configurations.PreviewDefaultMaxHeight;
                ContentMaxHeight = Configurations.PreviewDefaultMaxHeight - 17;
            }
            else
            {
                ContentContainer.MaxHeight = this.preview_LastMaxHeight;
                ContentMaxHeight = this.preview_LastMaxHeight - 17;
            }

            if (this.preview_LastMaxWidth == double.MaxValue)
            {
                ContentContainer.MaxWidth = Configurations.PreviewDefaultMaxWidth;
                ContentMaxWidth = Configurations.PreviewDefaultMaxWidth - 10;
            }
            else
            {
                ContentContainer.MaxWidth = this.preview_LastMaxWidth;
                ContentMaxWidth = this.preview_LastMaxWidth - 10;
            }

            ContentContainer.MinHeight = Configurations.PreviewMinHeight;
            ContentContainer.MinWidth = Configurations.PreviewMinWidth;
        }

        private void SetStyle_PreviewCondensed()
        {
            backgroundPolygon.Fill = Configurations.PreviewFrameFill;
            backgroundPolygon.StrokeThickness = Configurations.PreviewFrameStrokeThickness;
            backgroundPolygon.Stroke = Configurations.PreviewFrameStrokeColor;

            ContentContainer.MaxWidth = Configurations.PreviewCondensedMaxWidth;
            ContentContainer.MinWidth = Configurations.PreviewCondensedMinWidth;
            ContentContainer.MaxHeight = Configurations.PreviewCondensedMaxHeight;
            ContentContainer.MinHeight = Configurations.PreviewCondensedMinHeight;

            ContentMargin = Configurations.PreviewContentMargin;            
            ContentMaxWidth = Configurations.PreviewCondensedContentMaxWidth;
            ContentMaxHeight = Configurations.PreviewCondensedContentMaxHeight;

            ContentFontSize = Configurations.PreviewTextFontSize;
            ContentForeground = Configurations.PreviewTextForeground;
            ContentFontWeight = Configurations.PreviewTextFontWeight;
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
                TextBox textBox = GetNewTextBox(ViewModel.Content);
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

            textBox.Background      = Brushes.Transparent;
            textBox.IsReadOnly      = true;
            textBox.BorderThickness = new Thickness(0);

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
                case InfoBubbleViewModel.Style.LibraryItemPreview:
                    framePoints = GetFramePoints_LibraryItemPreview(estimatedHeight, estimatedWidth);
                    break;
                case InfoBubbleViewModel.Style.NodeTooltip:
                    framePoints = GetFramePoints_NodeTooltip(estimatedHeight, estimatedWidth);
                    break;
                case InfoBubbleViewModel.Style.Error:
                case InfoBubbleViewModel.Style.ErrorCondensed:
                    framePoints = GetFramePoints_Error(estimatedHeight, estimatedWidth);
                    break;
                case InfoBubbleViewModel.Style.Preview:
                case InfoBubbleViewModel.Style.PreviewCondensed:
                    framePoints = GetFramePoints_Preview(estimatedHeight, estimatedWidth);
                    break;
                case InfoBubbleViewModel.Style.None:
                    break;
            }

            if (framePoints != null)
                backgroundPolygon.Points = framePoints;
        }

        private PointCollection GetFramePoints_LibraryItemPreview(double estimatedHeight, double estimatedWidth)
        {
            double arrowHeight = Configurations.LibraryTooltipArrowHeight;
            double arrowWidth = Configurations.LibraryTooltipArrowWidth;

            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(estimatedWidth, 0));
            pointCollection.Add(new Point(arrowWidth, 0));
            pointCollection.Add(new Point(arrowWidth, estimatedHeight / 2 - arrowHeight / 2));
            pointCollection.Add(new Point(0, estimatedHeight / 2));
            pointCollection.Add(new Point(arrowWidth, estimatedHeight / 2 + arrowHeight / 2));
            pointCollection.Add(new Point(arrowWidth, estimatedHeight));
            pointCollection.Add(new Point(estimatedWidth, estimatedHeight));

            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltip(double estimatedHeight, double estimatedWidth)
        {
            switch (ViewModel.ConnectingDirection)
            {
                case InfoBubbleViewModel.Direction.Bottom:
                    return GetFramePoints_NodeTooltipConnectBottom(estimatedHeight, estimatedWidth);
                case InfoBubbleViewModel.Direction.Left:
                    return GetFramePoints_NodeTooltipConnectLeft(estimatedHeight, estimatedWidth);
                case InfoBubbleViewModel.Direction.Right:
                    return GetFramePoints_NodeTooltipConnectRight(estimatedHeight, estimatedWidth);
            }
            return new PointCollection();
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
            else if (ViewModel.TargetBotRight.X + estimatedWidth <= dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth)
            {
                ViewModel.LimitedDirection = InfoBubbleViewModel.Direction.Top;
                contentMargin = Configurations.NodeTooltipContentMarginLeft;
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
                contentMargin = Configurations.NodeTooltipContentMarginRight;
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
            if (ViewModel.TargetBotRight.X + estimatedWidth > dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth)
            {
                ViewModel.LimitedDirection = InfoBubbleViewModel.Direction.Right;
                contentMargin = Configurations.NodeTooltipContentMarginRight;

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
                contentMargin = Configurations.NodeTooltipContentMarginLeft;

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

        private PointCollection GetFramePoints_Preview(double estimatedHeight, double estimatedWidth)
        {
            double arrowHeight = Configurations.PreviewArrowHeight;
            double arrowWidth = Configurations.PreviewArrowWidth;

            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(estimatedWidth, arrowHeight));
            pointCollection.Add(new Point(estimatedWidth / 2 + arrowWidth / 2, arrowHeight));
            pointCollection.Add(new Point(estimatedWidth / 2, 0));
            pointCollection.Add(new Point(estimatedWidth / 2 - arrowWidth / 2, arrowHeight));
            pointCollection.Add(new Point(0, arrowHeight));
            pointCollection.Add(new Point(0, estimatedHeight));
            pointCollection.Add(new Point(estimatedWidth, estimatedHeight));
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
                case InfoBubbleViewModel.Style.LibraryItemPreview:
                    mainGrid.Margin = GetMargin_LibraryItemPreview(estimatedHeight, estimatedWidth);
                    //MakeFitInView(estimatedHeight, estimatedWidth);
                    break;
                case InfoBubbleViewModel.Style.NodeTooltip:
                    mainGrid.Margin = GetMargin_NodeTooltip(estimatedHeight, estimatedWidth);
                    MakeFitInView(estimatedHeight, estimatedWidth);
                    break;
                case InfoBubbleViewModel.Style.Error:
                case InfoBubbleViewModel.Style.ErrorCondensed:
                    mainGrid.Margin = GetMargin_Error(estimatedHeight, estimatedWidth);
                    break;
                case InfoBubbleViewModel.Style.Preview:
                case InfoBubbleViewModel.Style.PreviewCondensed:
                    mainGrid.Margin = GetMargin_Preview(estimatedHeight, estimatedWidth);
                    break;
            }
        }

        private Thickness GetMargin_LibraryItemPreview(double estimatedHeight, double estimatedWidth)
        {
            Thickness margin = new Thickness();
            margin.Top = (ViewModel.TargetTopLeft.Y + ViewModel.TargetBotRight.Y) / 2 - (estimatedHeight / 2);
            return margin;
        }

        private Thickness GetMargin_NodeTooltip(double estimatedHeight, double estimatedWidth)
        {
            Thickness margin = new Thickness();
            switch (ViewModel.ConnectingDirection)
            {
                case InfoBubbleViewModel.Direction.Bottom:
                    if (ViewModel.LimitedDirection == InfoBubbleViewModel.Direction.TopRight)
                    {
                        margin.Top = ViewModel.TargetBotRight.Y - (ViewModel.TargetBotRight.Y - ViewModel.TargetTopLeft.Y) / 2;
                        margin.Left = ViewModel.TargetTopLeft.X - estimatedWidth;
                    }
                    else if (ViewModel.LimitedDirection == InfoBubbleViewModel.Direction.Top)
                    {
                        margin.Top = ViewModel.TargetBotRight.Y - (ViewModel.TargetBotRight.Y - ViewModel.TargetTopLeft.Y) / 2;
                        margin.Left = ViewModel.TargetBotRight.X;
                    }
                    else
                    {
                        margin.Top = ViewModel.TargetTopLeft.Y - estimatedHeight;
                        margin.Left = (ViewModel.TargetTopLeft.X + ViewModel.TargetBotRight.X) / 2 - (estimatedWidth / 2);
                    }
                    break;
                case InfoBubbleViewModel.Direction.Left:
                    if (ViewModel.LimitedDirection == InfoBubbleViewModel.Direction.Right)
                    {
                        margin.Top = (ViewModel.TargetTopLeft.Y + ViewModel.TargetBotRight.Y) / 2 - (estimatedHeight / 2);
                        margin.Left = ViewModel.TargetTopLeft.X - estimatedWidth;
                    }
                    else
                    {
                        margin.Top = (ViewModel.TargetTopLeft.Y + ViewModel.TargetBotRight.Y) / 2 - (estimatedHeight / 2);
                        margin.Left = ViewModel.TargetBotRight.X;
                    }
                    break;
                case InfoBubbleViewModel.Direction.Right:
                    if (ViewModel.LimitedDirection == InfoBubbleViewModel.Direction.Left)
                    {
                        margin.Top = (ViewModel.TargetTopLeft.Y + ViewModel.TargetBotRight.Y) / 2 - (estimatedHeight / 2);
                        margin.Left = ViewModel.TargetBotRight.X;
                    }
                    else
                    {
                        margin.Top = (ViewModel.TargetTopLeft.Y + ViewModel.TargetBotRight.Y) / 2 - (estimatedHeight / 2);
                        margin.Left = ViewModel.TargetTopLeft.X - estimatedWidth;
                    }
                    break;
            }
            return margin;
        }

        private Thickness GetMargin_Error(double estimatedHeight, double estimatedWidth)
        {
            Thickness margin = new Thickness();
            double nodeWidth = ViewModel.TargetBotRight.X - ViewModel.TargetTopLeft.X;
            margin.Top = -(estimatedHeight) + ViewModel.TargetTopLeft.Y;
            margin.Left = -((estimatedWidth - nodeWidth) / 2) + ViewModel.TargetTopLeft.X;
            return margin;
        }

        private Thickness GetMargin_Preview(double estimatedHeight, double estimatedWidth)
        {
            Thickness margin = new Thickness();
            double nodeWidth = ViewModel.TargetBotRight.X - ViewModel.TargetTopLeft.X;
            margin.Top = ViewModel.TargetBotRight.Y;
            margin.Left = -((estimatedWidth - nodeWidth) / 2) + ViewModel.TargetTopLeft.X;

            if (ViewModel.InfoBubbleState == InfoBubbleViewModel.State.Minimized)
                margin.Top -= Configurations.PreviewArrowHeight;
            return margin;
        }

        private void MakeFitInView(double estimatedHeight, double estimatedWidth)
        {
            //top
            if (mainGrid.Margin.Top <= 30)
            {
                Thickness newMargin = mainGrid.Margin;
                newMargin.Top = 40;
                mainGrid.Margin = newMargin;
            }
            //left
            if (mainGrid.Margin.Left <= 0)
            {
                Thickness newMargin = mainGrid.Margin;
                newMargin.Left = 0;
                mainGrid.Margin = newMargin;
            }
            //botton
            if (mainGrid.Margin.Top + estimatedHeight >= dynSettings.Controller.DynamoViewModel.WorkspaceActualHeight)
            {
                Thickness newMargin = mainGrid.Margin;
                newMargin.Top = dynSettings.Controller.DynamoViewModel.WorkspaceActualHeight - estimatedHeight - 1;
                mainGrid.Margin = newMargin;
            }
            //right
            if (mainGrid.Margin.Left + estimatedWidth >= dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth)
            {
                Thickness newMargin = mainGrid.Margin;
                newMargin.Left = dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth - estimatedWidth - 1;
                mainGrid.Margin = newMargin;
            }

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
                
            if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.PreviewCondensed)
                ShowPreviewBubbleFullContent();
            else if (ViewModel.InfoBubbleStyle == InfoBubbleViewModel.Style.ErrorCondensed)
                ShowErrorBubbleFullContent();
            
            //FadeInInfoBubble();
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
                case InfoBubbleViewModel.Style.Preview:
                case InfoBubbleViewModel.Style.PreviewCondensed:
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
            e.Handled = true;
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

            //ViewModel.ResizeCommand.Execute(mouseLocation);
            Resize(mouseLocation);
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

            double offsetX = this.ActualWidth - ContentContainer.ActualWidth;
            double offsetY = this.ActualHeight - ContentContainer.ActualHeight;
            if (Math.Abs(mousePosition.X - offsetX - ContentContainer.ActualWidth / 2) < 25
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

        #endregion
    }
}