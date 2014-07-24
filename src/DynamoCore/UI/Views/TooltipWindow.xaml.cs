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

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for TooltipWindow.xaml
    /// </summary>
    public partial class TooltipWindow : UserControl
    {
        #region DependencyProperties
        public static DependencyProperty NameProperty = DependencyProperty.Register
            (
                 "NodeName",
                 typeof(string),
                 typeof(TooltipWindow),
                 new UIPropertyMetadata(String.Empty, OnPropertyNameChanged)
            );

        public static DependencyProperty DescriptionProperty = DependencyProperty.Register
            (
                 "Description",
                 typeof(string),
                 typeof(TooltipWindow),
                 new UIPropertyMetadata(String.Empty, OnPropertyDescriptionChanged)
            );

        public static DependencyProperty InputParametrsProperty = DependencyProperty.Register
            (
                 "InputParametrs",
                 typeof(string),
                 typeof(TooltipWindow),
                 new UIPropertyMetadata(String.Empty, OnPropertyInputParametrsChanged)
            );
        public static DependencyProperty OutputParametrsProperty = DependencyProperty.Register
           (
                "OutputParametrs",
                typeof(string),
                typeof(TooltipWindow),
                new UIPropertyMetadata(String.Empty, OnPropertyOutputParametrsChanged)
           );

        public static readonly DependencyProperty AttachmentSideProperty =
            DependencyProperty.Register("AttachmentSide",
            typeof(TooltipWindow.Side), typeof(TooltipWindow),
            new PropertyMetadata(TooltipWindow.Side.Left));
        #endregion

        #region Properties
        public string NodeName
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public string InputParametrs
        {
            get { return (string)GetValue(InputParametrsProperty); }
            set { SetValue(InputParametrsProperty, value); }
        }
        public string OutputParametrs
        {
            get { return (string)GetValue(OutputParametrsProperty); }
            set { SetValue(OutputParametrsProperty, value); }
        }
        public enum Side
        {
            Left, Top, Right, Bottom
        }
        #endregion

        #region OnPropertyChanged
        private static void OnPropertyNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var node = sender as TooltipWindow;

            if (node != null)
            {
                node.name.Text = (string)e.NewValue;
            }
        }
        private static void OnPropertyDescriptionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var node = sender as TooltipWindow;

            if (node != null)
            {
                node.description.Text = (string)e.NewValue;
            }
        }
        private static void OnPropertyInputParametrsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var node = sender as TooltipWindow;

            if (node != null)
            {
                node.input.Text = (string)e.NewValue;
            }
        }
        private static void OnPropertyOutputParametrsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var node = sender as TooltipWindow;

            if (node != null)
            {
                node.output.Text = (string)e.NewValue;
            }
        }
        #endregion

        public TooltipWindow()
        {
            InitializeComponent();
        }

        public void CopyIconMouseClick(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(NodeName);
        }

        public void MouseClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

    }
}
