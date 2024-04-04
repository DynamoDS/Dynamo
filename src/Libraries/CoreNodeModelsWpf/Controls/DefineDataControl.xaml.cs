using System.Windows.Controls;
using CoreNodeModels;

namespace CoreNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for DefineDataControl.xaml
    /// </summary>
    public partial class DefineDataControl : UserControl
    {
        private readonly DefineData model;

        internal ComboBox BaseComboBox { get; set; }

        public DefineDataControl(DefineDataViewModel viewModel)
        {
            DataContext = viewModel;
            model = viewModel.Model;

            InitializeComponent();
        }
    }
}
