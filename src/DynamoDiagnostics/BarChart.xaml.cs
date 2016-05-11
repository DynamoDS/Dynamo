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
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Dynamo.Diagnostics
{
    [System.ComponentModel.DefaultProperty("Legends")]
    public partial class BarChart : UserControl
    {
        public BarChart()
        {
            InitializeComponent();
            //this.DataContext = this;
        }

        #region Dependency properties
        public static readonly DependencyProperty VerticalPropertyNameProperty = DependencyProperty.Register("VerticalPropertyName", typeof(string), typeof(BarChart));
        public string VerticalPropertyName
        {
            get { return GetValue(VerticalPropertyNameProperty) == null ? string.Empty : GetValue(VerticalPropertyNameProperty).ToString(); }
            set
            {
                SetValue(VerticalPropertyNameProperty, value);
            }
        }

        public static readonly DependencyProperty HorizontalPropertyNameProperty = DependencyProperty.Register("HorizontalPropertyName", typeof(string), typeof(BarChart));
        public string HorizontalPropertyName
        {
            get { return GetValue(HorizontalPropertyNameProperty) == null ? string.Empty : GetValue(HorizontalPropertyNameProperty).ToString(); }
            set
            {
                SetValue(HorizontalPropertyNameProperty, value);
            }
        }

        public static readonly DependencyProperty LegendPropertyNameProperty = DependencyProperty.Register("LegendPropertyName", typeof(string), typeof(BarChart));
        public string LegendPropertyName
        {
            get { return GetValue(LegendPropertyNameProperty) == null ? string.Empty : GetValue(LegendPropertyNameProperty).ToString(); }
            set
            {
                SetValue(LegendPropertyNameProperty, value);
            }
        }

        public static DependencyProperty ValueVisibilityProperty = DependencyProperty.Register("ValueVisibility", typeof(Visibility), typeof(BarChart),
            new FrameworkPropertyMetadata(Visibility.Visible, ValueVisibilityChangedCallBack));
        public Visibility ValueVisibility
        {
            get { return (Visibility)GetValue(ValueVisibilityProperty); }
            set
            {
                SetValue(ValueVisibilityProperty, value);
                Notify("ValueVisibility");
            }
        }
        protected static void ValueVisibilityChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as BarChart).ValueVisibility = (Visibility)e.NewValue;
        }

        public static DependencyProperty LegendsVisibilityProperty = DependencyProperty.Register("LegendsVisibility", typeof(Visibility), typeof(BarChart),
            new PropertyMetadata(Visibility.Visible));
        public Visibility LegendsVisibility
        {
            get
            {
                return GetValue(LegendsVisibilityProperty) == null ? Visibility.Visible : (Visibility)GetValue(LegendsVisibilityProperty);
            }
            set
            {
                SetValue(LegendsVisibilityProperty, value);
                Notify("LegendsVisibility");
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(BarChart),
            new FrameworkPropertyMetadata() { PropertyChangedCallback = ItemsSourceChangedCallBack });
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
                IsDataSourceBinded = value != null;
                Refresh();
            }
        }

        protected static void ItemsSourceChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as BarChart).ItemsSource = (IEnumerable)e.NewValue;
        }

        public static readonly DependencyProperty LegendsProperty = DependencyProperty.Register("Legends", typeof(ObservableCollection<Legend>), typeof(BarChart),
            new PropertyMetadata(new ObservableCollection<Legend>() { }));
        public ObservableCollection<Legend> Legends
        {
            get
            {
                object res = GetValue(LegendsProperty);
                if (res == null)
                    res = new ObservableCollection<Legend>();

                (res as ObservableCollection<Legend>).CollectionChanged += (s, e) => { DrawLegends(); };
                return GetValue(LegendsProperty) == new ObservableCollection<Legend>() ? null : (ObservableCollection<Legend>)GetValue(LegendsProperty);
            }
            set
            {
                SetValue(LegendsProperty, value);
                Notify("Legends");
            }
        }

        public static readonly DependencyProperty CanChangeLegendVisibilityProperty = DependencyProperty.Register("CanChangeLegendVisibility", typeof(bool), typeof(BarChart),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (s, e) => { (s as BarChart).CanChangeLegendVisibility = (bool)e.NewValue; }));
        public bool CanChangeLegendVisibility
        {
            get
            {
                return (bool)GetValue(CanChangeLegendVisibilityProperty);
            }
            set
            {
                SetValue(CanChangeLegendVisibilityProperty, value);
                Notify("CanChangeLegendVisibility");
                Draw();
            }
        }
        #endregion Dependency properties

        #region Properties
        private bool _isDataSourceBinded = false;
        public bool IsDataSourceBinded
        {
            get { return _isDataSourceBinded; }
            private set { _isDataSourceBinded = value; }
        }

        private IEnumerable _items = null;
        public IEnumerable Items
        {
            get
            {
                if (IsDataSourceBinded)
                    return ItemsSource;
                else
                    return _items;
            }
            set
            {
                if (_items != value)
                {
                    if (IsDataSourceBinded)
                        throw new Exception("Control is in DataSource mode.");

                    _items = value;
                    Notify("Items");
                    GetLegends();
                    Draw();
                }
            }
        }

        private Border brdLegends;
        #endregion Properties

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Methods
        private void Notify(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private void Draw()
        {
            Draw(true);
        }

        private void Draw(bool UpdateLegends)
        {
            cnvMain.Children.Clear();

            double VertLineHorMargin = 40;
            double HorLineVertMargin = 25;
            double LegendsHorMargin = 10;

            // Drawing Main lines
            Line vLine = new Line();
            vLine.X1 = VertLineHorMargin;
            vLine.X2 = vLine.X1;
            vLine.Y1 = 10;
            vLine.Y2 = cnvMain.ActualHeight - 10;

            Line hLine = new Line();
            hLine.X1 = 10;
            hLine.X2 = cnvMain.ActualWidth - 10;
            hLine.Y1 = cnvMain.ActualHeight - HorLineVertMargin;
            hLine.Y2 = hLine.Y1;

            cnvMain.Children.Add(vLine);
            cnvMain.Children.Add(hLine);
            /////////////////////
            ArrayList tmpItems = new ArrayList();
            if (Items != null)
                foreach (object item in Items)
                    tmpItems.Add(item);

            if (tmpItems.Count == 0 ||
                String.IsNullOrEmpty(VerticalPropertyName) ||
                String.IsNullOrEmpty(HorizontalPropertyName) ||
                String.IsNullOrEmpty(LegendPropertyName))
                return;

            if (tmpItems[0].GetType().GetProperty(VerticalPropertyName) == null)
                throw new ArgumentException("VerticalPropertyName is not correct.");

            if (tmpItems[0].GetType().GetProperty(HorizontalPropertyName) == null)
                throw new ArgumentException("HorizontalPropertyName is not correct.");

            if (tmpItems[0].GetType().GetProperty(LegendPropertyName) == null)
                throw new ArgumentException("LegendPropertyName is not correct.");

            tmpItems.Sort(new ItemsComparer(HorizontalPropertyName));

            List<double> HorValues = new List<double>();
            double MaxValue = 0;

            foreach (var item in tmpItems)
            {
                var VertValue = item.GetType().GetProperty(VerticalPropertyName).GetValue(item, null);
                var HorValue = item.GetType().GetProperty(HorizontalPropertyName).GetValue(item, null);

                if (!HorValues.Exists(i => i == Convert.ToDouble(HorValue)))
                    HorValues.Add(Convert.ToDouble(HorValue));

                if (Convert.ToDouble(VertValue) > MaxValue)
                    MaxValue = Convert.ToDouble(VertValue);
            }

            if (cnvMain.ActualWidth == 0)
                return;

            double DrawingAreaWidth = (cnvMain.ActualWidth - VertLineHorMargin);
            double MaxValueTopMargin = 10 + 20;

            Line lMax = new Line();
            lMax.StrokeDashArray = new DoubleCollection() { 2 };
            lMax.X1 = VertLineHorMargin - 5;
            lMax.X2 = hLine.X2;
            lMax.Y1 = MaxValueTopMargin;
            lMax.Y2 = lMax.Y1;
            cnvMain.Children.Add(lMax);

            Line lAvg = new Line();
            lAvg.StrokeDashArray = new DoubleCollection() { 2 };
            lAvg.X1 = lMax.X1;
            lAvg.X2 = lMax.X2;
            lAvg.Y1 = (hLine.Y1 - lMax.Y1) / 2 + MaxValueTopMargin;
            lAvg.Y2 = lAvg.Y1;
            cnvMain.Children.Add(lAvg);

            TextBlock tbMax = new TextBlock();
            tbMax.Text = MaxValue.ToString();
            var formattedMaxText = new FormattedText(tbMax.Text,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(tbMax.FontFamily, tbMax.FontStyle, tbMax.FontWeight, tbMax.FontStretch),
                    tbMax.FontSize,
                    Brushes.Black);
            Canvas.SetLeft(tbMax, VertLineHorMargin - formattedMaxText.Width - 10);
            Canvas.SetTop(tbMax, lMax.Y1 - formattedMaxText.Height / 2.0);
            cnvMain.Children.Add(tbMax);

            TextBlock tbAvg = new TextBlock();
            tbAvg.Text = (MaxValue / 2.0).ToString();
            var formattedAvgText = new FormattedText(tbAvg.Text,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(tbAvg.FontFamily, tbAvg.FontStyle, tbAvg.FontWeight, tbAvg.FontStretch),
                    tbAvg.FontSize,
                    Brushes.Black);
            Canvas.SetLeft(tbAvg, VertLineHorMargin - formattedAvgText.Width - 10);
            Canvas.SetTop(tbAvg, lAvg.Y1 - formattedAvgText.Height / 2.0);
            cnvMain.Children.Add(tbAvg);

            int legendsCount = Legends.Where(f => f.IsVisible || !CanChangeLegendVisibility).Count();
            double BarsWidth = (DrawingAreaWidth - (HorValues.Count * LegendsHorMargin)) / HorValues.Count / legendsCount - LegendsHorMargin / 2.0;
            if (Double.IsInfinity(BarsWidth) || Double.IsNaN(BarsWidth))
                BarsWidth = 0;

            double HorItemWidth = Math.Ceiling((DrawingAreaWidth - (HorValues.Count * LegendsHorMargin)) / HorValues.Count);

            for (int i = 0; i < HorValues.Count; i++)
            {
                Line l = new Line();
                l.X1 = (HorItemWidth * i) + VertLineHorMargin + ((legendsCount * BarsWidth) / 2.0) + LegendsHorMargin;
                l.X2 = l.X1;
                l.Y1 = hLine.Y1;
                l.Y2 = l.Y1 + 5;
                cnvMain.Children.Add(l);

                TextBlock tb = new TextBlock();
                tb.Text = HorValues[i].ToString();
                var formattedText = new FormattedText(tb.Text,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                    tb.FontSize,
                    Brushes.Black);
                Canvas.SetLeft(tb, l.X1 - (formattedText.Width / 2));
                Canvas.SetTop(tb, l.Y2 + 5);
                cnvMain.Children.Add(tb);
            }

            foreach (double HorizontalIndex in HorValues)
            {
                var items = from object item in tmpItems
                            where Convert.ToDouble(item.GetType().GetProperty(HorizontalPropertyName).GetValue(item, null)) == HorizontalIndex
                            orderby item.GetType().GetProperty(LegendPropertyName).GetValue(item, null)
                            select item;

                int LegendValueIndex = 0;
                foreach (object item in items)
                {
                    var VerValue = item.GetType().GetProperty(VerticalPropertyName).GetValue(item, null);
                    var HorValue = item.GetType().GetProperty(HorizontalPropertyName).GetValue(item, null);
                    var LegValue = item.GetType().GetProperty(LegendPropertyName).GetValue(item, null);

                    object currentLegend = null;
                    try
                    {
                        currentLegend = Legends.Where(i => i.LegendType.Equals(LegValue)).First();
                    }
                    catch
                    {
                    }

                    if (currentLegend == null || (CanChangeLegendVisibility && !(currentLegend as Legend).IsVisible))
                        continue;

                    int HorizontalValueIndex = HorValues.IndexOf(Convert.ToDouble(HorValue));

                    double BarLeft = (HorItemWidth * HorizontalValueIndex) + LegendsHorMargin +
                        VertLineHorMargin +
                        (LegendValueIndex * BarsWidth);

                    Border b = new Border();
                    b.Style = (Style)cnvMain.FindResource("BarStyle");
                    b.Width = BarsWidth;
                    b.Height = Convert.ToDouble(VerValue) * (hLine.Y1 - lMax.Y1) / MaxValue;

                    b.Background = (currentLegend as Legend).Color;
                    Canvas.SetLeft(b, BarLeft);
                    Canvas.SetTop(b, hLine.Y1 - b.Height);
                    cnvMain.Children.Add(b);

                    TextBlock tbValue = new TextBlock();
                    Panel.SetZIndex(tbValue, 100);
                    tbValue.Text = VerValue.ToString();
                    Binding binding = new Binding("ValueVisibility");
                    binding.Source = this;
                    tbValue.SetBinding(TextBlock.VisibilityProperty, binding);
                    var formattedText = new FormattedText(tbValue.Text,
                        CultureInfo.CurrentUICulture,
                        FlowDirection.LeftToRight,
                        new Typeface(tbValue.FontFamily, tbValue.FontStyle, tbValue.FontWeight, tbValue.FontStretch),
                        tbValue.FontSize,
                        Brushes.Black);
                    Canvas.SetLeft(tbValue, BarLeft + (((BarLeft + BarsWidth) - BarLeft) / 2 - formattedText.Width / 2));
                    Canvas.SetTop(tbValue, hLine.Y1 - b.Height - formattedText.Height - 5);
                    cnvMain.Children.Add(tbValue);

                    LegendValueIndex++;
                }
            }

            if (UpdateLegends)
                DrawLegends();
            else
                cnvMain.Children.Add(brdLegends);
        }

        private void DrawLegends()
        {
            FrameworkElement legendbrd = null;
            foreach (FrameworkElement elem in cnvMain.Children)
            {
                if (elem is Border && elem.Name == "brdLegends")
                {
                    legendbrd = elem;
                    break;
                }
            }

            if (legendbrd != null)
                cnvMain.Children.Remove(legendbrd);

            // Creating Legends List
            brdLegends = new Border();
            brdLegends.Name = "brdLegends";
            brdLegends.CornerRadius = new CornerRadius(5);
            brdLegends.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
            Binding legVisibilityBinding = new Binding("LegendsVisibility");
            legVisibilityBinding.Source = this;
            brdLegends.SetBinding(Border.VisibilityProperty, legVisibilityBinding);

            brdLegends.BorderBrush = Brushes.DarkGray;
            brdLegends.BorderThickness = new Thickness(1);

            ListBox lstLegends = new ListBox();
            lstLegends.Margin = new Thickness(5);
            lstLegends.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            lstLegends.SelectionChanged += (slst, elst) =>
            {
                (slst as ListBox).UnselectAll();
            };
            lstLegends.BorderThickness = new Thickness(0);

            Binding legendsBinding = new Binding("Legends");
            legendsBinding.Source = this;
            lstLegends.SetBinding(ListBox.ItemsSourceProperty, legendsBinding);

            if (CanChangeLegendVisibility)
                lstLegends.ItemTemplate = (DataTemplate)this.FindResource("LegendsItemTemplate2");
            else
                lstLegends.ItemTemplate = (DataTemplate)this.FindResource("LegendsItemTemplate1");

            brdLegends.Child = lstLegends;
            cnvMain.Children.Add(brdLegends);

            brdLegends.UpdateLayout();
            Canvas.SetLeft(brdLegends, cnvMain.ActualWidth - brdLegends.ActualWidth - 10);
            Canvas.SetTop(brdLegends, 10);
            ////////////////////////
        }

        public void Refresh()
        {
            GetLegends();
            Draw();
        }

        private void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        private void GetLegends()
        {
            if (Legends == null)
                Legends = new ObservableCollection<Legend>();

            if (Items == null)
                return;

            Random rand = new Random(DateTime.Now.Millisecond);
            foreach (var item in Items)
            {
                var LegValue = item.GetType().GetProperty(LegendPropertyName).GetValue(item, null);

                var leg = from Legend lc in Legends where lc.LegendType.Equals(LegValue) select lc;
                if (leg.Count() == 0)
                {
                    Legend legend = new Legend();
                    legend.LegendType = LegValue;
                    Color c = Color.FromRgb(Convert.ToByte(rand.Next(256)), Convert.ToByte(rand.Next(256)), Convert.ToByte(rand.Next(256)));
                    legend.Color = new SolidColorBrush(c);
                    legend.IsVisibleChanged += (s, e) =>
                    {
                        Draw(false);
                    };

                    Legends.Add(legend);
                }
                else
                    leg.First().IsVisibleChanged += (s, e) =>
                    {
                        Draw(false);
                    };
            }
        }
        #endregion Methods

        #region Comparer for sorting items
        class ItemsComparer : IComparer
        {
            #region Constructors
            public ItemsComparer(string HorPropertyName)
                : base()
            {
                HorizontalPropertyName = HorPropertyName;
            }
            #endregion Constructors

            #region Properties
            private string HorizontalPropertyName = String.Empty;
            private string LegendPropertyName = String.Empty;
            #endregion Properties

            #region Methods
            public int Compare(object x, object y)
            {
                if (String.IsNullOrEmpty(HorizontalPropertyName))
                    throw new ArgumentException();

                var xHorValue = Convert.ToDouble(x.GetType().GetProperty(HorizontalPropertyName).GetValue(x, null));
                var yHorValue = Convert.ToDouble(y.GetType().GetProperty(HorizontalPropertyName).GetValue(y, null));

                return xHorValue.CompareTo(yHorValue);
            }
            #endregion Methods
        }
        #endregion Comparer for sorting items
    }

    [System.ComponentModel.DefaultEvent("IsVisibleChanged")]
    [System.ComponentModel.DefaultProperty("LegendType")]
    public class Legend : DependencyObject, INotifyPropertyChanged
    {
        #region Constructors
        public Legend()
        {
        }
        #endregion Constructors

        #region Properties
        private object _legend = null;
        public object LegendType
        {
            get { return _legend; }
            set
            {
                _legend = value;
                Notify("Legend");
            }
        }

        private string _displayName = String.Empty;
        public string DisplayName
        {
            get
            {
                if (String.IsNullOrEmpty(_displayName))
                    if (LegendType == null)
                        return String.Empty;
                    else
                        return LegendType.ToString();
                else
                    return _displayName;
            }
            set
            {
                _displayName = value;
                Notify("DisplayName");
            }
        }

        private Brush _color = null;
        public Brush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                Notify("Color");
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    Notify("IsVisible");
                    if (IsVisibleChanged != null)
                        IsVisibleChanged(this, new RoutedEventArgs());
                }
            }
        }
        #endregion Properties

        #region Events
        public event RoutedEventHandler IsVisibleChanged;
        #endregion Events

        #region Methods
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion Methods
    }
}
