using System;
using System.Windows.Controls;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for PortContextMenu.xaml
    /// </summary>
    public partial class PortContextMenu : UserControl
    {
        internal event Action<ShowHideFlags> RequestShowInPortContextMenu;

        public PortContextMenu()
        {
            InitializeComponent();
        }
    }
}
