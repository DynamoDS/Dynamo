using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.ViewModels
{
    public partial class PopupViewModel : ViewModelBase
    {
        public enum PopupStyle
        {
            LibraryItemPreview,
            NodeTooltip,
            Error,
            None
        }

        #region Properties

        private double _verticalOffset;
        public double VerticalOffset
        {
            get { return _verticalOffset; }
            set { _verticalOffset = value; RaisePropertyChanged("VerticalOffset"); }
        }

        private bool _isInUse;
        public bool IsInUse
        {
            get { return _isInUse; }
        }

        private double _maxWidth;
        public double MaxWidth
        {
            get { return _maxWidth; }
            set { _maxWidth = value; RaisePropertyChanged("MaxWidth"); }
        }

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set { _itemName = value; RaisePropertyChanged("ItemName"); }
        }

        private PopupStyle popupStyle;

        private string _itemDescription;
        public string ItemDescription
        {
            get { return _itemDescription; }
            set { _itemDescription = value; RaisePropertyChanged("ItemDescription"); }
        }

        private double _opacity;
        public double Opacity
        {
            get { return _opacity; }
            set { _opacity = value; RaisePropertyChanged("Opacity"); }
        }

        #endregion

        #region Public Methods

        public PopupViewModel(PopupStyle style)
        {
            _isInUse = false;
            popupStyle = style;
            InitializePopup();
        }

        #endregion

        #region Private Helper Method

        private void InitializePopup()
        {
        }

        private void UpdatePopup(object parameter)
        {
            KeyValuePair<double, string> positionContentPair = (KeyValuePair<double, string>)parameter;
            VerticalOffset = positionContentPair.Key;
            this.ItemDescription = positionContentPair.Key + '\n' + positionContentPair.Value;
        }

        private bool CanUpdatePopup(object parameter)
        {
            return !IsInUse;
        }

        private void FadeIn(object parameter)
        {
            Opacity = 1;
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
    }
}
