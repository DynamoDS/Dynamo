using System;
using System.Windows;
using CoreNodeModels.Input;
using Dynamo.Wpf;
using Prism.Commands;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// Node view customization for TestSelectModelElement node.
    /// Uses the SelectionBase pattern to provide UI with a clickable button for element selection.
    /// </summary>
    public class TestSelectModelElementNodeViewCustomization : SelectionBaseNodeViewCustomization<string, string>
    {
        // This class inherits from SelectionBaseNodeViewCustomization which provides
        // all the necessary UI setup for SelectionBase nodes, including:
        // - SelectCommand that calls Model.Select(null)
        // - ElementSelectionControl with proper binding
        // - CanSelect property binding
        // - Property change handling
    }
}
