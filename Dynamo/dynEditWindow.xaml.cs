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
using System.Windows.Shapes;

namespace Dynamo.Elements
{
    /// <summary>
    /// Interaction logic for dynEditWindow.xaml
    /// </summary>
    public partial class dynEditWindow : Window
    {
        public dynEditWindow()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //cance = return false
            this.DialogResult = false;
        }
    }
}
