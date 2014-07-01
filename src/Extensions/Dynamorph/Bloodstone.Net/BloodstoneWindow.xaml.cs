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

namespace Bloodstone
{
    public partial class BloodstoneWindow : Window
    {
        public BloodstoneWindow()
        {
            InitializeComponent();
            this.Closing += OnBloodstoneWindowClosing;
        }

        public BloodstoneControl Control { get { return TheBloodstoneControl; } }

        private void OnBloodstoneWindowClosing(object sender, CancelEventArgs e)
        {
            TheBloodstoneControl.DestroyVisualizer();
        }
    }
}
