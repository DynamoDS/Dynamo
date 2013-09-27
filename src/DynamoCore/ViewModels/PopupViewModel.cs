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
    public partial class PopupViewModel : ViewModelBase
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
        private ObservableCollection<UIElement> _contentCollection = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> ContentCollection
        {
            get { return _contentCollection; }
            set { _contentCollection = value; RaisePropertyChanged("ContentCollection"); }
        }

        private Style _popupStyle;
        public Style PopupStyle
        {
            get { return _popupStyle; }
            set { _popupStyle = value; RaisePropertyChanged("PopupStyle"); }
        }

        private PointCollection _framePoints;
        public PointCollection FramePoints
        {
            get { return _framePoints; }
            set { _framePoints = value; RaisePropertyChanged("FramePoints"); }
        }
        private SolidColorBrush _frameFill;
        public SolidColorBrush FrameFill
        {
            get
            {
                return _frameFill;
            }
            set
            {
                _frameFill = value; RaisePropertyChanged("FrameFill");
            }
        }
        private double _frameStrokeThickness;
        public double FrameStrokeThickness
        {
            get { return _frameStrokeThickness; }
            set { _frameStrokeThickness = value; RaisePropertyChanged("FrameStrokeThickness"); }
        }
        private SolidColorBrush _frameStrokeColor;
        public SolidColorBrush FrameStrokeColor
        {
            get { return _frameStrokeColor; }
            set { _frameStrokeColor = value; RaisePropertyChanged("FrameStrokeColor"); }
        }

        private Thickness _margin;
        public Thickness Margin
        {
            get { return _margin; }
            set { _margin = value; RaisePropertyChanged("Margin"); }
        }
        private Thickness _contentmargin;
        public Thickness ContentMargin
        {
            get { return _contentmargin; }
            set { _contentmargin = value; RaisePropertyChanged("ContentMargin"); }
        }
        private double _maxWidth;
        public double MaxWidth
        {
            get { return _maxWidth; }
            set { _maxWidth = value; RaisePropertyChanged("MaxWidth"); }
        }

        private double _opacity = 0;
        public double Opacity
        {
            get { return _opacity; }
            set { _opacity = value; RaisePropertyChanged("Opacity"); }
        }
        private Visibility _popupVisibility;
        public Visibility PopupVisibility
        {
            get { return _popupVisibility; }
            set { _popupVisibility = value; RaisePropertyChanged("PopupVisibility"); }
        }

        private double _textFontSize;
        public double TextFontSize
        {
            get { return _textFontSize; }
            set { _textFontSize = value; RaisePropertyChanged("TextFontSize"); }
        }
        private SolidColorBrush _textForeground;
        public SolidColorBrush TextForeground
        {
            get { return _textForeground; }
            set { _textForeground = value; RaisePropertyChanged("TextForeground"); }
        }
        private FontWeight _textFontWeight;
        public FontWeight TextFontWeight
        {
            get { return _textFontWeight; }
            set { _textFontWeight = value; RaisePropertyChanged("TextFontWeight"); }
        }
        private TextWrapping _contentWrapping;
        public TextWrapping ContentWrapping
        {
            get { return _contentWrapping; }
            set { _contentWrapping = value; RaisePropertyChanged("TextWrapping"); }
        }
        private string _itemDescription;
        public string ItemDescription
        {
            get { return _itemDescription; }
            set { _itemDescription = value; RaisePropertyChanged("ItemDescription"); }
        }

        private Timer _fadeInTimer;
        private Timer _fadeOutTimer;

        private Direction _connectingDirection;
        private Direction _limitedDirection;
        private bool _alwaysVisible;

        #endregion

        #region Public Methods

        public PopupViewModel()
        {
            _fadeInTimer = new Timer(20);
            _fadeInTimer.Elapsed += fadeInTimer_Elapsed;
            _fadeInTimer.Enabled = true;

            _fadeOutTimer = new Timer(20);
            _fadeOutTimer.Elapsed += fadeOutTimer_Elapsed;
            _fadeOutTimer.Enabled = true;

            _alwaysVisible = false;
            _limitedDirection = Direction.None;
            Opacity = 0;
            PopupStyle = Style.None;

            InitiateContentCollection();
        }

        #endregion

        #region Command Methods

        private void UpdatePopup(object parameter)
        {
            PopupDataPacket data = (PopupDataPacket)parameter;
            UpdateStyle(data.Style, data.ConnectingDirection);
            UpdateContent(data.Text);
            UpdateShape(data.TopLeft, data.BotRight);
            UpdatePosition(data.TopLeft, data.BotRight);
        }

        private bool CanUpdatePopup(object parameter)
        {
            return true;
        }

        private void FadeIn(object parameter)
        {
            _fadeOutTimer.Stop();
            _fadeInTimer.Start();
        }

        private bool CanFadeIn(object parameter)
        {
            return true;
        }

        private void FadeOut(object parameter)
        {
            if (_alwaysVisible)
                return;
            _fadeInTimer.Stop();
            _fadeOutTimer.Start();
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
            _alwaysVisible = (bool)parameter;
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
            ContentCollection.Clear();
            TextBox textBox = GetStyledTextBox(ItemDescription);
            ContentCollection.Add(textBox);
        }

        private void UpdatePosition(Point topLeft, Point botRight)
        {
            ContentCollection[0].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            switch (PopupStyle)
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

        private void UpdateStyle(PopupViewModel.Style style, Direction connectingDirection)
        {
            PopupStyle = style;
            this._connectingDirection = connectingDirection;
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
                    throw new ArgumentException("PopupWindow didn't have a style (456B24E0F400)");
            }
        }

        private void UpdateShape(Point topLeft, Point botRight)
        {
            ContentCollection[0].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            switch (PopupStyle)
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
            margin.Top = (topLeft.Y + botRight.Y) / 2 - (ContentCollection[0].DesiredSize.Height / 2);
            return margin;
        }

        private Thickness GetMargin_NodeTooltip(Point topLeft, Point botRight)
        {
            Thickness margin = new Thickness();
            switch (_connectingDirection)
            {
                case Direction.Bottom:
                    if (_limitedDirection == Direction.TopRight)
                    {
                        margin.Top = botRight.Y - (botRight.Y - topLeft.Y) / 2;
                        margin.Left = topLeft.X - ContentCollection[0].DesiredSize.Width;
                    }
                    else if (_limitedDirection == Direction.Top)
                    {
                        margin.Top = botRight.Y - (botRight.Y - topLeft.Y) / 2;
                        margin.Left = botRight.X;
                    }
                    else
                    {
                        margin.Top = topLeft.Y - ContentCollection[0].DesiredSize.Height;
                        margin.Left = (topLeft.X + botRight.X) / 2 - (ContentCollection[0].DesiredSize.Width / 2);
                    }
                    break;
                case Direction.Left:
                    if (_limitedDirection == Direction.Right)
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (ContentCollection[0].DesiredSize.Height / 2);
                        margin.Left = topLeft.X - ContentCollection[0].DesiredSize.Width;
                    }
                    else
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (ContentCollection[0].DesiredSize.Height / 2);
                        margin.Left = botRight.X;
                    }
                    break;
                case Direction.Right:
                    if (_limitedDirection == Direction.Left)
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (ContentCollection[0].DesiredSize.Height / 2);
                        margin.Left = botRight.X;
                    }
                    else
                    {
                        margin.Top = (topLeft.Y + botRight.Y) / 2 - (ContentCollection[0].DesiredSize.Height / 2);
                        margin.Left = topLeft.X - ContentCollection[0].DesiredSize.Width;
                    }
                    break;
            }
            return margin;
        }

        private Thickness GetMargin_Error(Point topLeft, Point botRight)
        {
            Thickness margin = new Thickness();
            double nodeWidth = botRight.X - topLeft.X;
            margin.Top = -(ContentCollection[0].DesiredSize.Height);
            if (ContentCollection[0].DesiredSize.Width > nodeWidth)
                margin.Left = -((ContentCollection[0].DesiredSize.Width - nodeWidth) / 2);
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

        private TextBox GetStyledTextBox(string text)
        {
            TextBox textBox = new TextBox();
            textBox.TextWrapping = ContentWrapping;
            textBox.Text = text;
            textBox.IsReadOnly = true;
            textBox.BorderThickness = new Thickness(0);
            textBox.Background = Brushes.Transparent;
            textBox.Foreground = TextForeground;
            textBox.FontWeight = TextFontWeight;
            textBox.FontSize = TextFontSize;
            return textBox;
        }

        private PointCollection GetFramePoints_LibraryItemPreview()
        {
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, 0));
            pointCollection.Add(new Point(7, 0));
            pointCollection.Add(new Point(7, ContentCollection[0].DesiredSize.Height / 2 - 7));
            pointCollection.Add(new Point(0, ContentCollection[0].DesiredSize.Height / 2));
            pointCollection.Add(new Point(7, ContentCollection[0].DesiredSize.Height / 2 + 7));
            pointCollection.Add(new Point(7, ContentCollection[0].DesiredSize.Height));
            pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, ContentCollection[0].DesiredSize.Height));

            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltip(Point topLeft, Point botRight)
        {
            switch (_connectingDirection)
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
            ContentCollection[0].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            PointCollection pointCollection = new PointCollection();
            Grid container = VisualTreeHelper.GetParent(_view) as Grid;
            container = VisualTreeHelper.GetParent(container) as Grid;

            if (topLeft.Y - ContentCollection[0].DesiredSize.Height >= 40)
            {
                _limitedDirection = Direction.None;
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, ContentCollection[0].DesiredSize.Height - 6));
                pointCollection.Add(new Point((ContentCollection[0].DesiredSize.Width / 2) - 6, ContentCollection[0].DesiredSize.Height - 6));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width / 2, ContentCollection[0].DesiredSize.Height));
                pointCollection.Add(new Point((ContentCollection[0].DesiredSize.Width / 2) + 6, ContentCollection[0].DesiredSize.Height - 6));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, ContentCollection[0].DesiredSize.Height - 6));
            }
            else if (botRight.X + ContentCollection[0].DesiredSize.Width <= container.ActualWidth)
            {
                _limitedDirection = Direction.Top;
                ContentMargin = new Thickness(11, 5, 5, 5);
                ContentCollection[0].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(6, 6));
                pointCollection.Add(new Point(6, ContentCollection[0].DesiredSize.Height));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, ContentCollection[0].DesiredSize.Height));
            }
            else
            {
                _limitedDirection = Direction.TopRight;
                ContentMargin = new Thickness(5, 5, 11, 5);
                ContentCollection[0].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, ContentCollection[0].DesiredSize.Height));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width - 6, ContentCollection[0].DesiredSize.Height));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width - 6, 6));

            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltipConnectLeft(Point topLeft, Point botRight)
        {
            ContentCollection[0].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            PointCollection pointCollection = new PointCollection();
            Grid container = VisualTreeHelper.GetParent(_view) as Grid;
            container = VisualTreeHelper.GetParent(container) as Grid;

            if (botRight.X + ContentCollection[0].DesiredSize.Width > container.ActualWidth)
            {
                _limitedDirection = Direction.Right;
                ContentMargin = new Thickness(5, 5, 11, 5);
                pointCollection = GetFramePoints_NodeTooltipConnectRight(topLeft, botRight);
            }
            else
            {
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, 0));
                pointCollection.Add(new Point(6, 0));
                pointCollection.Add(new Point(6, ContentCollection[0].DesiredSize.Height / 2 - 6));
                pointCollection.Add(new Point(0, ContentCollection[0].DesiredSize.Height / 2));
                pointCollection.Add(new Point(6, ContentCollection[0].DesiredSize.Height / 2 + 6));
                pointCollection.Add(new Point(6, ContentCollection[0].DesiredSize.Height));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, ContentCollection[0].DesiredSize.Height));
            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltipConnectRight(Point topLeft, Point botRight)
        {
            ContentCollection[0].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            PointCollection pointCollection = new PointCollection();

            if (topLeft.X - ContentCollection[0].DesiredSize.Width < 0)
            {
                _limitedDirection = Direction.Left;
                ContentMargin = new Thickness(11, 5, 5, 5);
                pointCollection = GetFramePoints_NodeTooltipConnectLeft(topLeft, botRight);
            }
            else
            {
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width - 6, 0));
                pointCollection.Add(new Point(0, 0));
                pointCollection.Add(new Point(0, ContentCollection[0].DesiredSize.Height));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width - 6, ContentCollection[0].DesiredSize.Height));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width - 6, ContentCollection[0].DesiredSize.Height / 2 + 6));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, ContentCollection[0].DesiredSize.Height / 2));
                pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width - 6, ContentCollection[0].DesiredSize.Height / 2 - 6));
            }
            return pointCollection;
        }

        private PointCollection GetFramePoints_Error()
        {
            ContentCollection[0].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, 0));
            pointCollection.Add(new Point(0, 0));
            pointCollection.Add(new Point(0, ContentCollection[0].DesiredSize.Height - 6));
            pointCollection.Add(new Point((ContentCollection[0].DesiredSize.Width / 2) - 6, ContentCollection[0].DesiredSize.Height - 6));
            pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width / 2, ContentCollection[0].DesiredSize.Height));
            pointCollection.Add(new Point((ContentCollection[0].DesiredSize.Width / 2) + 6, ContentCollection[0].DesiredSize.Height - 6));
            pointCollection.Add(new Point(ContentCollection[0].DesiredSize.Width, ContentCollection[0].DesiredSize.Height - 6));
            return pointCollection;
        }

        private void MakeFitInView()
        {
            ContentCollection[0].Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Grid container = VisualTreeHelper.GetParent(_view) as Grid;
            container = VisualTreeHelper.GetParent(container) as Grid;
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
            if (Margin.Top + ContentCollection[0].DesiredSize.Height > container.ActualHeight)
            {
                Thickness newMargin = Margin;
                newMargin.Top = container.ActualHeight - ContentCollection[0].DesiredSize.Height;
                Margin = newMargin;
            }
            //right
            if (Margin.Left + ContentCollection[0].DesiredSize.Width > container.ActualWidth)
            {
                Thickness newMargin = Margin;
                newMargin.Left = container.ActualWidth - ContentCollection[0].DesiredSize.Width;
                Margin = newMargin;
            }

        }

        private void fadeInTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Opacity <= 0.85)
                Opacity += 0.85 / 10;
            else
                _fadeInTimer.Stop();
        }

        private void fadeOutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Opacity >= 0)
                Opacity -= 0.85 / 10;
            else
                _fadeOutTimer.Stop();
        }

        #endregion
    }

    public struct PopupDataPacket
    {
        public PopupViewModel.Style Style;
        public Point TopLeft;
        public Point BotRight;
        public string Text;
        public PopupViewModel.Direction ConnectingDirection;
        public Guid TargetGUID;

        public PopupDataPacket(PopupViewModel.Style style, Point topLeft, Point botRight,
            string text, PopupViewModel.Direction connectingDirection, Guid targetGUID)
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
