using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [NodeName("Boolean")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Selection between a true and false.")]
    [NodeSearchTags("true", "truth", "false")]
    [IsDesignScriptCompatible]
    public class BoolSelector : Bool
    {
        private DynamoViewModel dynamoViewModel;

        public BoolSelector(WorkspaceModel workspace) : base(workspace)
        {
            Value = false;
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            this.dynamoViewModel = nodeUI.ViewModel.DynamoViewModel;

            //add a text box to the input grid of the control
            var rbTrue = new RadioButton();
            var rbFalse = new RadioButton();
            rbTrue.VerticalAlignment = VerticalAlignment.Center;
            rbFalse.VerticalAlignment = VerticalAlignment.Center;

            //use a unique name for the button group
            //so other instances of this element don't get confused
            string groupName = Guid.NewGuid().ToString();
            rbTrue.GroupName = groupName;
            rbFalse.GroupName = groupName;

            rbTrue.Content = "True";
            rbTrue.Padding = new Thickness(0,0,12,0);
            rbFalse.Content = "False";
            rbFalse.Padding = new Thickness(0);
            var wp = new WrapPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(10,5,10,0),
                Orientation = Orientation.Horizontal
            };

            wp.Children.Add(rbTrue);
            wp.Children.Add(rbFalse);
            nodeUI.inputGrid.Children.Add(wp);

            //rbFalse.IsChecked = true;
            rbTrue.Checked += OnRadioButtonClicked;
            rbFalse.Checked += OnRadioButtonClicked;

            rbFalse.DataContext = this;
            rbTrue.DataContext = this;

            var rbTrueBinding = new Binding("Value") { Mode = BindingMode.TwoWay, };
            rbTrue.SetBinding(ToggleButton.IsCheckedProperty, rbTrueBinding);

            var rbFalseBinding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new InverseBoolDisplay()
            };
            rbFalse.SetBinding(ToggleButton.IsCheckedProperty, rbFalseBinding);
        }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }

        private void OnRadioButtonClicked(object sender, RoutedEventArgs e)
        {
            dynamoViewModel.ReturnFocusToSearch();
        }
    }
}
