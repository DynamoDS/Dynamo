using System;
using System.Windows.Controls;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for OutportContextMenu.xaml
    /// </summary>
    public partial class OutPortContextMenu : UserControl
    {
        internal event Action<ShowHideFlags> RequestShowOutPortContextMenu;
        
        public OutPortContextMenu()
        {
            InitializeComponent();
        }
    }
}
