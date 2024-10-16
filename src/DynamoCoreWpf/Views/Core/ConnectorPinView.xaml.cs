using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Configuration;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for ConnectorPinView.xaml
    /// </summary>
    public partial class ConnectorPinView : IViewModelView<ConnectorPinViewModel>
    {
        public ConnectorPinViewModel ViewModel { get; private set; }

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

        /// <summary>
        /// Sets ZIndex of the particular pin to be the highest in the workspace
        /// This brings the pin to the forefront of the workspace when clicked
        /// </summary>
        private void BringToFront()
        {
            if (ConnectorPinViewModel.StaticZIndex == int.MaxValue)
            {
                PrepareZIndex();
            }

            //Set all pins to -1 the current index
            foreach (var pin in ViewModel.WorkspaceViewModel.Pins)
            {
                pin.ZIndex = ConnectorPinViewModel.StaticZIndex-1;
            }
            //Sets active pin to an index higher
            ViewModel.ZIndex = ConnectorPinViewModel.StaticZIndex;
        }

        /// <summary>
        /// If ZIndex is more then max value of int, it should be set back to the initial ZIndex to all elements.
        /// </summary>
        private void PrepareZIndex()
        {
            ConnectorPinViewModel.StaticZIndex = Configurations.NodeStartZIndex;

            var parent = TemplatedParent as ContentPresenter;
            if (parent == null) return;

            foreach (var child in parent.ChildrenOfType<ConnectorPinView>())
            {
                child.ViewModel.ZIndex = Configurations.NodeStartZIndex;
            }
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

        private void OnPinRightMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.IsHoveredOver = false;
        }
    }
}
