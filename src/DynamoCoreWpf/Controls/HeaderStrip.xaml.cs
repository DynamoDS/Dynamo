using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Dynamo.UI.Controls
{
    public partial class HeaderStrip : UserControl
    {
        #region Event

        public event RoutedEventHandler HeaderActivated;
        private void OnHeaderActivated(object source, RoutedEventArgs e)
        {
            if (HeaderActivated != null)
                HeaderActivated(source, e);
        }

        #endregion

        #region Properties

        public IEnumerable<HeaderStripItem> HeaderStripItems
        {
            get
            {
                return (IEnumerable<HeaderStripItem>)GetValue(HeaderStripItemsProperty);
            }
            set
            {
                if (value == null)
                    return;

                SetValue(HeaderStripItemsProperty, value);
                SelectedItem = value.FirstOrDefault();
            }
        }

        public HeaderStripItem SelectedItem
        {
            get { return (HeaderStripItem)GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemProperty, value); }
        }

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty HeaderStripItemsProperty =
            DependencyProperty.Register("HeadersList", typeof(IEnumerable<HeaderStripItem>),
              typeof(HeaderStrip), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(HeaderStripItem),
              typeof(HeaderStrip), new PropertyMetadata(null));

        #endregion

        public HeaderStrip()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void OnHeaderButtonClick(object sender, RoutedEventArgs e)
        {
            var clickedStripItem = (sender as Button).DataContext as HeaderStripItem;
            if (clickedStripItem != SelectedItem)
            {
                SelectedItem = clickedStripItem;
                OnHeaderActivated(sender, e);
            }
        }
    }

    public class HeaderStripItem
    {
        public string Text { get; set; }
    }
}

namespace Dynamo.Controls
{
    public class SelectedItemToActiveConverter : IMultiValueConverter
    {
        public SolidColorBrush NormalColor { get; set; }
        public SolidColorBrush ActiveColor { get; set; }

        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ArgumentException();

            if (values[0] == values[1])
                return ActiveColor;
            else
                return NormalColor;
        }

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
