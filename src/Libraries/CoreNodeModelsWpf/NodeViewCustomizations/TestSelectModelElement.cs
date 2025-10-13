using System;
using System.Windows;
using CoreNodeModels.Input;
using Dynamo.Wpf;
using Prism.Commands;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// Node view customization for TestSelectModelElement node.
    /// Provides the UI with a clickable button for element selection.
    /// </summary>
    public class TestSelectModelElementNodeViewCustomization : INodeViewCustomization<TestSelectModelElement>
    {
        public TestSelectModelElement Model { get; set; }
        public DelegateCommand SelectCommand { get; set; }

        public void CustomizeView(TestSelectModelElement model, Dynamo.Controls.NodeView nodeView)
        {
            Model = model;
            SelectCommand = new DelegateCommand(() => Model.SelectElement(), () => Model.CanSelect);
            
            // Update command state when CanSelect property changes
            Model.PropertyChanged += (s, e) => {
                nodeView.Dispatcher.Invoke(new Action(() =>
                {
                    if (e.PropertyName == "CanSelect")
                    {
                        SelectCommand.RaiseCanExecuteChanged();
                    }
                }));                      
            };

            // Create and add the selection control
            var selectionControl = new TestElementSelectionControl { DataContext = this };
            nodeView.inputGrid.Children.Add(selectionControl);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
