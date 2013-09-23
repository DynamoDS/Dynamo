using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

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

        #endregion

        #region Public Methods

        public PopupViewModel(Style style)
        {
            UpdateStyle(style);
        }

        #endregion

        #region Command Methods

        private void UpdatePopup(object parameter)
        {
            KeyValuePair<double, string> positionContentPair = (KeyValuePair<double, string>)parameter;
            UpdatePosition(0, positionContentPair.Key);
            UpdateContent(positionContentPair.Value);
        }

        private bool CanUpdatePopup(object parameter)
        {
            return !IsInUse;
        }

        private void FadeIn(object parameter)
        {
            Opacity = 0.96;
        }

        private bool CanFadeIn(object parameter)
        {
            return true;
        }

        private void FadeOut(object parameter)
        {
            Opacity = 0;
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
        }

        private void UpdatePosition(double horizontalOffset, double verticalOffset)
        {
            Thickness updatedMargin = new Thickness(horizontalOffset, verticalOffset, 0, 0);
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
