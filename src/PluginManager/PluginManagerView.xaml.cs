using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dynamo.PluginManager.ViewModel;

namespace Dynamo.PluginManager.View
{
    public partial class PluginManagerView : Window
    {
        private PluginManagerViewModel ViewModel
        {
            get { return this.DataContext as PluginManagerViewModel; }
        }

        public PluginManagerView(PluginManagerViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
