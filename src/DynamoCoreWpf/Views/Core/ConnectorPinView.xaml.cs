using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Connectors;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Controls;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;
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
            if (ConnectorPinViewModel.StaticZIndex == int.MaxValue)
            {
                PrepareZIndex();
            }

            ViewModel.ZIndex = ++ConnectorPinViewModel.StaticZIndex;
        }

        /// <summary>
        /// If ZIndex is more then max value of int, it should be set back to 0 for all elements.
        /// The ZIndex for ConnectorPins is set to match that of nodes.
        /// </summary>
        private void PrepareZIndex()
        {
            var parent = TemplatedParent as ContentPresenter;
            if (parent == null) return;

            // reset the ZIndex for all ConnectorPins
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
    }
}
