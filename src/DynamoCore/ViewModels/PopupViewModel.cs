using Dynamo.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
        public enum ConnectingDirection
        {
            Left,
            Top,
            Right,
            Bottom
        }

        #region Properties

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

        private bool _isInUse;
        public bool IsInUse
        {
            get { return _isInUse; }
            private set { _isInUse = value; }
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
        private string _itemDescription;
        public string ItemDescription
        {
            get { return _itemDescription; }
            set { _itemDescription = value; RaisePropertyChanged("ItemDescription"); }
        }

        private PopupView _view;

        private Timer fadeInTimer;
        private Timer fadeOutTimer;

        #endregion

        #region Public Methods

        public PopupViewModel()
        {
            fadeInTimer = new Timer(20);
            fadeInTimer.Elapsed += fadeInTimer_Elapsed;
            fadeInTimer.Enabled = true;

            fadeOutTimer = new Timer(20);
            fadeOutTimer.Elapsed += fadeOutTimer_Elapsed;
            fadeOutTimer.Enabled = true;
        }

        public void UpdateView(Style style, PopupView view)
        {
            UpdateStyle(style);
            _view = view;
        }

        #endregion

        #region Command Methods

        private void UpdatePopup(object parameter)
        {
            KeyValuePair<double, string> positionContentPair = (KeyValuePair<double, string>)parameter;
            UpdateContent(positionContentPair.Value);
            UpdateShape();
            UpdatePosition(0, positionContentPair.Key);
        }

        private bool CanUpdatePopup(object parameter)
        {
            return !IsInUse;
        }

        private void FadeIn(object parameter)
        {
            fadeOutTimer.Stop();
            fadeInTimer.Start();
        }

        private bool CanFadeIn(object parameter)
        {
            return true;
        }

        private void FadeOut(object parameter)
        {
            fadeInTimer.Stop();
            fadeOutTimer.Start();
        }

        private bool CanFadeOut(object parameter)
        {
            return true;
        }

        #endregion

        #region Private Helper Method

        private void UpdateContent(string text)
        {
            ItemDescription = text;

            _view.contentContainer.Children.Clear();
            _view.contentContainer.Children.Add(GetStyledTextBox(ItemDescription));
        }

        private void UpdatePosition(double horizontalOffset, double verticalOffset)
        {
            double top = verticalOffset - (_view.contentContainer.DesiredSize.Height / 2);
            double bottom = 0;
            double left = horizontalOffset;
            double right = 0;
            Thickness updatedMargin = new Thickness(left, top, right, bottom);
            Margin = updatedMargin;
        }

        private void UpdateStyle(PopupViewModel.Style style)
        {
            PopupStyle = style;
            switch (style)
            {
                case Style.LibraryItemPreview:
                    SetStyle_LibraryItemPreview();
                    break;
                case Style.NodeTooltip:
                    SetStyle_NodeTooltip();
                    break;
                case Style.Error:
                    SetStyle_Error();
                    break;
                case Style.None:
                    throw new ArgumentException("PopupWindow didn't have a style (456B24E0F400)");
            }
        }

        private void UpdateShape()
        {
            switch (PopupStyle)
            {
                case Style.LibraryItemPreview:
                    FramePoints = GetFramePoints_LibraryItemPreview();
                    break;
                case Style.NodeTooltip:
                    FramePoints = GetFramePoints_NodeTooltip();
                    break;
                case Style.Error:
                    FramePoints = GetFramePoints_Error();
                    break;
                case Style.None:
                    break;
            }
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
            ContentMargin = new Thickness(12, 5, 5, 5);
        }

        private void SetStyle_NodeTooltip()
        {
            FrameFill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            FrameStrokeThickness = 1;
            FrameStrokeColor = new SolidColorBrush(Color.FromRgb(165, 209, 226));

            MaxWidth = 200;

            TextFontSize = 12;
            TextFontWeight = FontWeights.Light;
            TextForeground = new SolidColorBrush(Color.FromRgb(98, 140, 153));
        }

        private void SetStyle_Error()
        {

        }

        private TextBox GetStyledTextBox(string text)
        {
            TextBox textBox = new TextBox();
            textBox.Text = text;
            textBox.IsReadOnly = true;
            textBox.BorderThickness = new Thickness(0);
            textBox.Background = Brushes.Transparent;
            textBox.FontSize = 13;
            textBox.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.Margin = new Thickness(12, 5, 5, 5);
            return textBox;
        }

        private PointCollection GetFramePoints_LibraryItemPreview()
        {
            _view.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(_view.contentContainer.DesiredSize.Width, 0));
            pointCollection.Add(new Point(7, 0));
            pointCollection.Add(new Point(7, _view.contentContainer.DesiredSize.Height / 2 - 7));
            pointCollection.Add(new Point(0, _view.contentContainer.DesiredSize.Height / 2));
            pointCollection.Add(new Point(7, _view.contentContainer.DesiredSize.Height / 2 + 7));
            pointCollection.Add(new Point(7, _view.contentContainer.DesiredSize.Height));
            pointCollection.Add(new Point(_view.contentContainer.DesiredSize.Width, _view.contentContainer.DesiredSize.Height));

            return pointCollection;
        }

        private PointCollection GetFramePoints_NodeTooltip()
        {
            _view.contentContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(_view.contentContainer.DesiredSize.Width, 0));
            pointCollection.Add(new Point(6, 0));
            pointCollection.Add(new Point(6, _view.contentContainer.DesiredSize.Height / 2 - 6));
            pointCollection.Add(new Point(0, _view.contentContainer.DesiredSize.Height / 2));
            pointCollection.Add(new Point(6, _view.contentContainer.DesiredSize.Height / 2 + 6));
            pointCollection.Add(new Point(6, _view.contentContainer.DesiredSize.Height));
            pointCollection.Add(new Point(_view.contentContainer.DesiredSize.Width, _view.contentContainer.DesiredSize.Height));
            return pointCollection;
        }

        private PointCollection GetFramePoints_Error()
        {
            PointCollection pointCollection = new PointCollection();

            return pointCollection;
        }

        private void fadeInTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Opacity < 0.9)
                Opacity += 0.9 / 10;
            else
                fadeInTimer.Stop();
        }

        private void fadeOutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Opacity > 0)
                Opacity -= 0.9 / 10;
            else
                fadeOutTimer.Stop();
        }

        #endregion
    }

    public struct PopupDataPacket
    {
        public object TargetObject;
        public double HorizontalOffset;
        public double VerticalOffset;
        public string Text;

        public PopupDataPacket(object targetObject, double horizontalOffset, double verticalOffset, string text)
        {
            TargetObject = targetObject;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            Text = text;
        }
    }
}
