using System.Windows.Controls;
using CoreNodeModels;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for DefineDataControl.xaml
    /// </summary>
    public partial class DefineDataControl : UserControl
    {
        public DefineDataControl(DefineData model)
        {
            DataContext = model;

            InitializeComponent();
        }
    }
}
