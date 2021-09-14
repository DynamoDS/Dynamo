using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.PythonMigration
{
    /// <summary>
    /// Interaction logic for IronPythonInfoDialog.xaml
    /// </summary>
    public partial class IronPythonInfoDialog : Window
    {
        PythonMigrationViewExtension ViewModel { get; set; }
        internal IronPythonInfoDialog(PythonMigrationViewExtension viewModel)
        {
            this.ViewModel = viewModel;
            InitializeComponent();
            SetPythonDepedenciesData();
        }

        private void SetPythonDepedenciesData()
        {
            // Filter the Custom Nodes that have a direct dependency on IronPython2. 
            var customNodesWithPythonDependencies = GraphPythonDependencies.CustomNodePythonDependencyMap
                                                            .Where(x => x.Value.Equals(GraphPythonDependencies.CNPythonDependencyType.DirectDependency));

            if (!customNodesWithPythonDependencies.Any()) return;

            foreach (var entry in customNodesWithPythonDependencies)
            {
                this.ViewModel.CustomNodeManager.TryGetNodeInfo(entry.Key, out CustomNodeInfo customNodeInfo);

                var packageName = customNodeInfo.PackageInfo != null ? customNodeInfo.PackageInfo.Name : CustomNodeItem.UserDefinitions;

                CustomNodeItem customNodeItem = new CustomNodeItem(customNodeInfo.Name, customNodeInfo.FunctionId, packageName);

                if (customNodeInfo.PackageInfo != null)
                    this.PackagedCustomNodesContainingPython.Items.Add(customNodeItem);
                else
                    this.UserDefinitionCustomNodesContainingPython.Items.Add(customNodeItem);
            }

            if (this.PackagedCustomNodesContainingPython.Items.Count > 0)
                this.ExpanderForPackagedDependencies.Visibility = Visibility.Visible;

            if (this.UserDefinitionCustomNodesContainingPython.Items.Count > 0)
                this.ExpanderForUserDefinitionDependencies.Visibility = Visibility.Visible;

            this.MainExpander.Visibility = Visibility.Visible;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnMoreInformationButtonClicked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.OpenPythonMigrationWarningDocumentation();
            Close();
        }

        private void ToggleIronPythonDialog(object sender, RoutedEventArgs e)
        {
            this.ViewModel.DynamoViewModel.IsIronPythonDialogDisabled = this.DisableIronPythonDialogCheck.IsChecked.Value;
        }

        private void OnCustomNodeClick(object sender, RoutedEventArgs e)
        {
            if (this.UserDefinitionCustomNodesContainingPython.SelectedItems.Count == 1)
            {
                CustomNodeItem selectedCNItem = this.UserDefinitionCustomNodesContainingPython.SelectedItem as CustomNodeItem;
                Guid functionId = selectedCNItem.FunctionId;
                this.ViewModel.DynamoViewModel.Model.OpenCustomNodeWorkspace(functionId);
            }
        }

        private void ExecuteCtrlCCopyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            string collectedText = "";
            ListBox lb = (ListBox)sender;

            foreach (var item in lb.SelectedItems)
            {
                collectedText += item.ToString() + System.Environment.NewLine;
            }

            if (lb.SelectedItems != null)
            {
                Clipboard.SetText(collectedText.ToString());
            }
        }

        private void CanExecuteCtrlCCopyCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Represents the data for each Custom Node item in the listbox. 
        private class CustomNodeItem
        {
            internal CustomNodeItem(string name, Guid functionId, string packageName)
            {
                this.Name = name;
                this.FunctionId = functionId;
                this.PackageName = packageName;
            }

            public string Name { get; private set; }
            public string PackageName { get; private set; }
            internal Guid FunctionId { get; private set; }

            internal const string UserDefinitions = "Definitions";

            public override string ToString()
            {
                if (this.PackageName.Equals(UserDefinitions))
                {
                    return this.Name;
                }
                else
                {
                    return this.PackageName + " : " + this.Name;
                }
            }
        }

    }
}
