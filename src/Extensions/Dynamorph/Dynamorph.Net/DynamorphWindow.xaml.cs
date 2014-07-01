using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dynamorph
{
    public partial class DynamorphWindow : Window
    {
        public DynamorphWindow()
        {
            InitializeComponent();
            this.Closing += OnDynamorphWindowClosing;
        }

        public DynamorphControl Control { get { return TheDynamorphControl; } }

        private void OnDynamorphWindowClosing(object sender, CancelEventArgs e)
        {
            TheDynamorphControl.DestroyVisualizer();
        }
    }
}
