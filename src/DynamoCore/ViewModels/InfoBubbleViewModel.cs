using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Dynamo.Utilities;
using System.Windows.Threading;
using Dynamo.Controls;
using System.Collections.ObjectModel;

namespace Dynamo.ViewModels
{
    public partial class InfoBubbleViewModel : ViewModelBase
    {
        public enum Style
        {
            LibraryItemPreview,
            NodeTooltip,
            Error,
            None
        }
        public enum Direction
        {
            None,
            Left,
            Top,
            Right,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        #region Properties

        public double ZIndex
        {
            get
            {
                return 5;
            }
        }
        public Guid TargetGUID { get; set; }

        private Style infoBubbleStyle;
        public Style InfoBubbleStyle
        {
            get { return infoBubbleStyle; }
            set { infoBubbleStyle = value; RaisePropertyChanged("InfoBubbleStyle"); }
        }

        public double EstimatedWidth;
        public double EstimatedHeight;
        private PointCollection framePoints;
        public PointCollection FramePoints
        {
            get { return framePoints; }
            set { framePoints = value; RaisePropertyChanged("FramePoints"); }
        }
        private SolidColorBrush frameFill;
        public SolidColorBrush FrameFill
        {
            get
            {
                return frameFill;
            }
            set
            {
                frameFill = value; RaisePropertyChanged("FrameFill");
            }
        }
        private double frameStrokeThickness;
        public double FrameStrokeThickness
        {
            get { return frameStrokeThickness; }
            set { frameStrokeThickness = value; RaisePropertyChanged("FrameStrokeThickness"); }
        }
        private SolidColorBrush frameStrokeColor;
        public SolidColorBrush FrameStrokeColor
        {
            get { return frameStrokeColor; }
            set { frameStrokeColor = value; RaisePropertyChanged("FrameStrokeColor"); }
        }

        private Thickness margin;
        public Thickness Margin
        {
            get { return margin; }
            set { margin = value; RaisePropertyChanged("Margin"); }
        }
        private Thickness contentmargin;
        public Thickness ContentMargin
        {
            get { return contentmargin; }
            set { contentmargin = value; RaisePropertyChanged("ContentMargin"); }
        }
        private double maxWidth;
        public double MaxWidth
        {
            get { return maxWidth; }
            set { maxWidth = value; RaisePropertyChanged("MaxWidth"); }
        }

        private double opacity = 0;
        public double Opacity
        {
            get { return opacity; }
            set { opacity = value; RaisePropertyChanged("Opacity"); }
        }
        private Visibility infoBubbleVisibility;
        public Visibility InfoBubbleVisibility
        {
            get { return infoBubbleVisibility; }
            set { infoBubbleVisibility = value; RaisePropertyChanged("InfoBubbleVisibility"); }
        }

        private double textFontSize;
        public double TextFontSize
        {
            get { return textFontSize; }
            set { textFontSize = value; RaisePropertyChanged("TextFontSize"); }
        }
        private SolidColorBrush textForeground;
        public SolidColorBrush TextForeground
        {
            get { return textForeground; }
            set { textForeground = value; RaisePropertyChanged("TextForeground"); }
        }
        private FontWeight textFontWeight;
        public FontWeight TextFontWeight
        {
            get { return textFontWeight; }
            set { textFontWeight = value; RaisePropertyChanged("TextFontWeight"); }
        }
        private TextWrapping contentWrapping;
        public TextWrapping ContentWrapping
        {
            get { return contentWrapping; }
            set { contentWrapping = value; RaisePropertyChanged("TextWrapping"); }
        }
        private string itemDescription;
        public string ItemDescription
        {
            get { return itemDescription; }
            set { itemDescription = value; RaisePropertyChanged("ItemDescription"); }
        }

        private Timer fadeInTimer;
        private Timer fadeOutTimer;

        public Direction ConnectingDirection
        {
            get;
            set;
        }
        private Direction limitedDirection;
        private bool alwaysVisible;

        #endregion

        #region Public Methods

        public InfoBubbleViewModel()
        {
            fadeInTimer = new Timer(20);
            fadeInTimer.Elapsed += fadeInTimer_Elapsed;
            fadeInTimer.Enabled = true;

            fadeOutTimer = new Timer(20);
            fadeOutTimer.Elapsed += fadeOutTimer_Elapsed;
            fadeOutTimer.Enabled = true;

            alwaysVisible = false;
            limitedDirection = Direction.None;
            Opacity = 0;
            InfoBubbleStyle = Style.None;
        }

        public InfoBubbleViewModel(Guid guid)
        {
            fadeInTimer = new Timer(20);
            fadeInTimer.Elapsed += fadeInTimer_Elapsed;
            fadeInTimer.Enabled = true;

            fadeOutTimer = new Timer(20);
            fadeOutTimer.Elapsed += fadeOutTimer_Elapsed;
            fadeOutTimer.Enabled = true;

            alwaysVisible = false;
            limitedDirection = Direction.None;
            Opacity = 0;
            InfoBubbleStyle = Style.None;
            this.TargetGUID = guid;
        }

        #endregion

        #region Command Methods

        private void UpdateInfoBubbleContent(object parameter)
        {
            if (dynSettings.Controller.DynamoViewModel.IsMouseDown)
                return;
            InfoBubbleDataPacket data = (InfoBubbleDataPacket)parameter;
            UpdateStyle(data.Style, data.ConnectingDirection);
            UpdateContent(data.Text);
            UpdateShape(data.TopLeft, data.BotRight);
            UpdatePosition(data.TopLeft, data.BotRight);
        }

        private bool CanUpdateInfoBubbleCommand(object parameter)
        {
            return true;
        }

        private void UpdatePosition(object parameter)
        {
            InfoBubbleDataPacket data = (InfoBubbleDataPacket)parameter;
            UpdatePosition(data.TopLeft, data.BotRight);
        }

        private bool CanUpdatePosition(object parameter)
        {
            return true;
        }

        private void FadeIn(object parameter)
        {
            if (dynSettings.Controller.DynamoViewModel.IsMouseDown)
                return;
            fadeOutTimer.Stop();
            fadeInTimer.Start();
        }

        private bool CanFadeIn(object parameter)
        {
            return true;
        }

        private void FadeOut(object parameter)
        {
            if (alwaysVisible)
                return;
            fadeInTimer.Stop();
            fadeOutTimer.Start();
        }

        private bool CanFadeOut(object parameter)
        {
            return true;
        }

        private void InstantCollapse(object parameter)
        {
            Opacity = 0;
        }

        private bool CanInstantCollapse(object parameter)
        {
            return true;
        }

        private void SetAlwaysVisible(object parameter)
        {
            alwaysVisible = (bool)parameter;
        }

        private bool CanSetAlwaysVisible(object parameter)
        {
            return true;
        }

        #endregion

        #region Private Helper Method

        private void UpdateContent(string text)
        {
            ItemDescription = text;
        }

        private void UpdatePosition(Point topLeft, Point botRight)
        {
            switch (InfoBubbleStyle)
            {
                case Style.LibraryItemPreview:
                    Margin = GetMargin_LibraryItemPreview(topLeft, botRight);
                    MakeFitInView();
                    break;
                case Style.NodeTooltip:
                    Margin = GetMargin_NodeTooltip(topLeft, botRight);
                    MakeFitInView();
                    break;
                case Style.Error:
                    Margin = GetMargin_Error(topLeft, botRight);
                    break;
            }
        }

        private void UpdateStyle(InfoBubbleViewModel.Style style, Direction connectingDirection)
        {
            InfoBubbleStyle = style;
            this.ConnectingDirection = connectingDirection;
            switch (style)
            {
                case Style.LibraryItemPreview:
                    SetStyle_LibraryItemPreview();
                    break;
                case Style.NodeTooltip:
                    SetStyle_NodeTooltip(connectingDirection);
                    break;
                case Style.Error:
                    SetStyle_Error();
                    break;
                case Style.None:
                    throw new ArgumentException("InfoWindow didn't have a style (456B24E0F400)");
            }
        }

        private void UpdateShape(Point topLeft, Point botRight)
        {
            switch (InfoBubbleStyle)
            {
                case Style.LibraryItemPreview:
                    FramePoints = GetFramePoints_LibraryItemPreview();
                    break;
                case Style.NodeTooltip:
                    FramePoints = GetFramePoints_NodeTooltip(topLeft, botRight);
                    break;
                case Style.Error:
                    FramePoints = GetFramePoints_Error();
                    break;
                case Style.None:
                    break;
            }
        }

        private Thickness GetMargin_LibraryItemPreview(Point topLeft, Point botRight)
        {
            Thickness margin = new Thickness();
            margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
            return margin;
        }

        private Thickness GetMargin_NodeTooltip(Point topLeft, Point botRight)
        {
            Thickness margin = new Thickness();
            switch (ConnectingDirection)
            {
                case Direction.Bottom:
                    if (limitedDirection == Direction.TopRight)
                    {
                        margin.Top = botRight.Y - (botRight.Y - topLeft.Y) / 2;
                        margin.Left = topLeft.X - EstimatedWidth;
                    }
                    else if (limitedDirection == Direction.Top)
                    {
                        margin.Top = botRight.Y - (botRight.Y - topLeft.Y) / 2;
                        margin.Left = botRight.X;
                    }
                    else
                    {
                        margin.Top = topLeft.Y - EstimatedHeight;
                        margin.Left = (topLeft.X + botRight.X) / 2 - (EstimatedWidth / 2);
                    }
                    break;
                case Direction.Left:
                    if (limitedDirection == Direction.Right)
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
                        margin.Left = topLeft.X - EstimatedWidth;
                    }
                    else
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
                        margin.Left = botRight.X;
                    }
                    break;
                case Direction.Right:
                    if (limitedDirection == Direction.Left)
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
                        margin.Left = botRight.X;
                    }
                    else
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (EstimatedHeight / 2);
                        margin.Left = topLeft.X - EstimatedWidth;
                    }
                    break;
            }
            return margin;
        }

        private Thickness GetMargin_Error(Point topLeft, Point botRight)
        {
            Thickness margin = new Thickness();
            double nodeWidth = botRight.X - topLeft.X;
            margin.Top = -(EstimatedHeight) + topLeft.Y;
            if (EstimatedWidth > nodeWidth)
                margin.Left = -((EstimatedWidth - nodeWidth) / 2) + topLeft.X;
            return margin;
        }

        private void SetStyle_LibraryItemPreview()
        {
            FrameFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            FrameStrokeThickness = 1;
            FrameStrokeColor = new SolidColorBrush(Color.FromRgb(10, 93, 30));

            MaxWidth = 400;

            TextFontSize = 13;
            TextForeground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            TextFontWeight = FontWeights.Normal;
            ContentWrapping = TextWrapping.Wrap;
            ContentMargin = new Thickness(12, 5, 5, 5);
        }

        private void SetStyle_NodeTooltip(Direction connectingDirection)
        {
            FrameFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            FrameStrokeThickness = 1;
            FrameStrokeColor = new SolidColorBrush(Color.FromRgb(165, 209, 226));

            MaxWidth = 200;

            TextFontSize = 12;
            TextFontWeight = FontWeights.Light;
            TextForeground = new SolidColorBrush(Color.FromRgb(98, 140, 153));
            ContentWrapping = TextWrapping.Wrap;

            switch (connectingDirection)
            {
                case Direction.Left:
                    ContentMargin = new Thickness(11, 5, 5, 5);
                    break;
                case Direction.Right:
                    ContentMargin = new Thickness(5, 5, 11, 5);
                    break;
                case Direction.Bottom:
                    ContentMargin = new Thickness(5, 5, 5, 11);
                    break;
            }
        }

        private void SetStyle_Error()
        {
            FrameFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            FrameStrokeThickness = 1;
            FrameStrokeColor = new SolidColorBrush(Color.FromRgb(190, 70, 70));

            MaxWidth = 300;

            TextFontSize = 13;
            TextFontWeight = FontWeights.Light;
            TextForeground = new SolidColorBrush(Color.FromRgb(190, 70, 70));
            ContentWrapping = TextWrapping.Wrap;
            ContentMargin = new Thickness(5, 5, 5, 12);
        }

        private PointCollection GetFramePoints_LibraryItemPreview()
        {
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(EstimatedWidth, 0));
            pointCollection.Add(new Point(7, 0));
            pointCollection.Add(new Point(7, EstimatedHeight / 2 - 7));
            pointCollection.Add(new Point(0, EstimatedHeight / 2));
            pointCollection.Add(new Point(7, EstimatedHeight / 2 + 7));
            pointCollection.Add(new Point(7, EstimatedHeight));
            pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight));

            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltip(Point topLeft, Point botRight)
        {
            switch (ConnectingDirection)
            {
                case Direction.Bottom:
                    return GetFramePoints_NodeTooltipConnectBottom(topLeft, botRight);
                case Direction.Left:
                    return GetFramePoints_NodeTooltipConnectLeft(topLeft, botRight);
                case Direction.Right:
                    return GetFramePoints_NodeTooltipConnectRight(topLeft, botRight);
            }
            return new PointCollection();
        }

        private PointCollection GetFramePoints_NodeTooltipConnectBottom(Point topLeft, Point botRight)
        {
            PointCollection pointCollection = new PointCollection();

            if (topLeft.Y - EstimatedHeight >= 40)
            {
                limitedDirection = Direction.None;
                pointCollection.Add(new Point(EstimatedWidth, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, EstimatedHeight - 6));
                pointCollection.Add(new Point((EstimatedWidth / 2) - 6, EstimatedHeight - 6));
                pointCollection.Add(new Point(EstimatedWidth / 2, EstimatedHeight));
                pointCollection.Add(new Point((EstimatedWidth / 2) + 6, EstimatedHeight - 6));
                pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight - 6));
            }
            else if (botRight.X + EstimatedWidth <= dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth)
            {
                limitedDirection = Direction.Top;
                ContentMargin = new Thickness(11, 5, 5, 5);
                UpdateContent(ItemDescription);

                pointCollection.Add(new Point(EstimatedWidth, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(6, 6));
                pointCollection.Add(new Point(6, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight));
            }
            else
            {
                limitedDirection = Direction.TopRight;
                ContentMargin = new Thickness(5, 5, 11, 5);
                UpdateContent(ItemDescription);

                pointCollection.Add(new Point(EstimatedWidth, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth - 6, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth - 6, 6));

            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltipConnectLeft(Point topLeft, Point botRight)
        {
            PointCollection pointCollection = new PointCollection();

            if (botRight.X + EstimatedWidth > dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth)
            {
                limitedDirection = Direction.Right;
                ContentMargin = new Thickness(5, 5, 11, 5);
                UpdateContent(ItemDescription);
                pointCollection = GetFramePoints_NodeTooltipConnectRight(topLeft, botRight);
            }
            else
            {
                pointCollection.Add(new Point(EstimatedWidth, 0));
                pointCollection.Add(new Point(6, 0));
                pointCollection.Add(new Point(6, EstimatedHeight / 2 - 6));
                pointCollection.Add(new Point(0, EstimatedHeight / 2));
                pointCollection.Add(new Point(6, EstimatedHeight / 2 + 6));
                pointCollection.Add(new Point(6, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight));
            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltipConnectRight(Point topLeft, Point botRight)
        {
            PointCollection pointCollection = new PointCollection();

            if (topLeft.X - EstimatedWidth < 0)
            {
                limitedDirection = Direction.Left;
                ContentMargin = new Thickness(11, 5, 5, 5);
                UpdateContent(ItemDescription);
                pointCollection = GetFramePoints_NodeTooltipConnectLeft(topLeft, botRight);
            }
            else
            {
                pointCollection.Add(new Point(EstimatedWidth - 6, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth - 6, EstimatedHeight));
                pointCollection.Add(new Point(EstimatedWidth - 6, EstimatedHeight / 2 + 6));
                pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight / 2));
                pointCollection.Add(new Point(EstimatedWidth - 6, EstimatedHeight / 2 - 6));
            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_Error()
        {
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(EstimatedWidth, 0));
            pointCollection.Add(new Point(0, 0));
            pointCollection.Add(new Point(0, EstimatedHeight - 6));
            pointCollection.Add(new Point((EstimatedWidth / 2) - 6, EstimatedHeight - 6));
            pointCollection.Add(new Point(EstimatedWidth / 2, EstimatedHeight));
            pointCollection.Add(new Point((EstimatedWidth / 2) + 6, EstimatedHeight - 6));
            pointCollection.Add(new Point(EstimatedWidth, EstimatedHeight - 6));
            return pointCollection;
        }

        private void MakeFitInView()
        {
            //top
            if (Margin.Top < 30)
            {
                Thickness newMargin = Margin;
                newMargin.Top = 40;
                Margin = newMargin;
            }
            //left
            if (Margin.Left < 0)
            {
                Thickness newMargin = Margin;
                newMargin.Left = 0;
                this.Margin = newMargin;
            }
            //botton
            if (Margin.Top + EstimatedHeight > dynSettings.Controller.DynamoViewModel.WorkspaceActualHeight)
            {
                Thickness newMargin = Margin;
                newMargin.Top = dynSettings.Controller.DynamoViewModel.WorkspaceActualHeight - EstimatedHeight;
                Margin = newMargin;
            }
            //right
            if (Margin.Left + EstimatedWidth > dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth)
            {
                Thickness newMargin = Margin;
                newMargin.Left = dynSettings.Controller.DynamoViewModel.WorkspaceActualWidth - EstimatedWidth;
                Margin = newMargin;
            }

        }

        private void fadeInTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Opacity <= 0.85)
                Opacity += 0.85 / 10;
            else
                fadeInTimer.Stop();
        }

        private void fadeOutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Opacity >= 0)
                Opacity -= 0.85 / 10;
            else
                fadeOutTimer.Stop();
        }

        #endregion
    }

    public struct InfoBubbleDataPacket
    {
        public InfoBubbleViewModel.Style Style;
        public Point TopLeft;
        public Point BotRight;
        public string Text;
        public InfoBubbleViewModel.Direction ConnectingDirection;
        public Guid TargetGUID;

        public InfoBubbleDataPacket(InfoBubbleViewModel.Style style, Point topLeft, Point botRight,
            string text, InfoBubbleViewModel.Direction connectingDirection, Guid targetGUID)
        {
            Style = style;
            TopLeft = topLeft;
            BotRight = botRight;
            Text = text;
            ConnectingDirection = connectingDirection;
            TargetGUID = targetGUID;
        }
    }
}
