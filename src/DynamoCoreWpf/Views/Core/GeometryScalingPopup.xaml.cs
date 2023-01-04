using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.ViewModels;
using ViewModels.Core;

namespace Dynamo.Views
{
    public partial class GeometryScalingPopup : Popup
    {
        GeometryScalingViewModel viewModel;

        public GeometryScalingPopup(DynamoViewModel dynViewModel)
        {
            InitializeComponent();

            if (viewModel == null && dynViewModel.GeoScalingViewModel != null)
                viewModel = dynViewModel.GeoScalingViewModel;
            DataContext = viewModel;
        }

        private void Geometry_Scaling_Checked(object sender, System.Windows.RoutedEventArgs e)
        {         
            var button = sender as Button;
            if (button == null) return;
            viewModel.UpdateGeometryScaling(button.Name);
            this.IsOpen = false;
        }

        /// <summary>
        /// Relocate the Popup when the Dynamo window is moved or resized
        /// </summary>
        internal void UpdatePopupLocation()
        {
            if (IsOpen)
            {
                var positionMethod = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                positionMethod.Invoke(this, null);
            }
        }
    }
}
