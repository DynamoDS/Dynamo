using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Data;
using System;
using System.Windows.Media;

namespace Dynamo.UI.Controls
{
    public partial class HeaderStrip : UserControl
    {
        #region Properties

        public IEnumerable<HeaderStripItem> HeadersList
        {
            get { return (IEnumerable<HeaderStripItem>)GetValue(HeadersListProperty); }
            set { SetValue(HeadersListProperty, value); }
        }

        public HeaderStripItem SelectedItem
        {
            get { return (HeaderStripItem)GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemProperty, value); }
        }

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty HeadersListProperty =
            DependencyProperty.Register("HeadersList", typeof(IEnumerable<HeaderStripItem>),
              typeof(HeaderStrip), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(HeaderStripItem),
              typeof(HeaderStrip), new PropertyMetadata(null));

        #endregion

        public HeaderStrip()
        {
            HeaderStripItem item1 = new HeaderStripItem() { Index = 12, Text = "CRE_A_TE1" };
            HeaderStripItem item2 = new HeaderStripItem() { Index = 12, Text = "CRE_A_TE2" };
            HeadersList = new List<HeaderStripItem> { item1, item2 };
            SelectedItem = HeadersList.First();

            InitializeComponent();

            this.DataContext = this;
        }
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
            throw new System.NotImplementedException();
        }
    }
}
