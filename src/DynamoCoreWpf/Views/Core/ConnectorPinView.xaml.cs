using System.Windows;
using System.Windows.Input;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.ViewModels;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for ConnectorPinView.xaml
    /// </summary>
    public partial class ConnectorPinView : IViewModelView<ConnectorPinViewModel>
    {
        public ConnectorPinViewModel ViewModel { get; private set; }
        /// <summary>
        /// Old ZIndex of node. It's set, when mouse leaves node.
        /// </summary>
        private int oldZIndex;

        public ConnectorPinView()
        {
            // Add DynamoConverters - currently using the InverseBoolToVisibilityCollapsedConverter
            // to be able to collapse pins
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);

            InitializeComponent();
            ViewModel = null;

            Loaded += OnPinViewLoaded;
        }
        private void OnPinViewLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as ConnectorPinViewModel;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BringToFront();
        }
        private void OnPinViewMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.ZIndex = oldZIndex;
        }

        /// <summary>
        /// Sets ZIndex of the particular note to be the highest in the workspace
        /// This brings the note to the forefront of the workspace when clicked
        /// </summary>
        private void BringToFront()
        {
            var index = ConnectorPinViewModel.StaticZIndex + 1;

            //Set all pins to -1 the current index
            foreach (var pin in ViewModel.WorkspaceViewModel.Pins)
            {
                pin.ZIndex = index - 1;
            }

            oldZIndex = index;
            ViewModel.ZIndex = index;
        }

        private void OnPinMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ViewModel.Model.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.ClearSelection();
                }
                DynamoSelection.Instance.Selection.AddUnique(ViewModel.Model);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.Selection.Remove(ViewModel.Model);
                }
            }
        }
        private void OnPinRightMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.IsHoveredOver = true;
        }

        private void OnPinRightMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.IsHoveredOver = false;
        }
    }
}
