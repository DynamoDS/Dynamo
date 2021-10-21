using System;
using System.Windows.Controls;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for InPortContextMenu.xaml
    /// </summary>
    public partial class InPortContextMenu : UserControl
    {
        internal event Action<ShowHideFlags> RequestShowInPortContextMenu;

        public InPortContextMenu()
        {
            InitializeComponent();
        }
    }
}
