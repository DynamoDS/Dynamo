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

            if (customNodesWithPythonDependencies.Any())
            {
                foreach (var entry in customNodesWithPythonDependencies)
                {
                    ViewModel.CustomNodeManager.TryGetNodeInfo(entry.Key, out CustomNodeInfo customNodeInfo);

                    var packageName = customNodeInfo.PackageInfo != null ? customNodeInfo.PackageInfo.Name : CustomNodeItem.UserDefinitions;

                    CustomNodeItem customNodeItem = new CustomNodeItem(customNodeInfo.Name, customNodeInfo.FunctionId, packageName);

                    if (customNodeInfo.PackageInfo != null)
                    {
                        PackagedCustomNodesContainingPython.Items.Add(customNodeItem);
                    }
                    else
                    {
                        UserDefinitionCustomNodesContainingPython.Items.Add(customNodeItem);
                    }
                }

                if (PackagedCustomNodesContainingPython.Items.Count > 0)
                {
                    ExpanderForPackagedDependencies.Visibility = Visibility.Visible;
                }

                if (UserDefinitionCustomNodesContainingPython.Items.Count > 0)
                {
                    ExpanderForUserDefinitionDependencies.Visibility = Visibility.Visible;
                }
                MainExpander.Visibility = Visibility.Visible;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnMoreInformationButtonClicked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.OpenPythonMigrationWarningDocumentation();
            this.Close();
        }

        private void ToggleIronPythonDialog(object sender, RoutedEventArgs e)
        {
            if (DisableIronPythonDialogCheck.IsChecked.HasValue && DisableIronPythonDialogCheck.IsChecked.Value)
                ViewModel.DynamoViewModel.IsIronPythonDialogDisabled = true;
            else
                ViewModel.DynamoViewModel.IsIronPythonDialogDisabled = false;
        }

        private void OnCustomNodeClick(object sender, RoutedEventArgs e)
        {
            if (UserDefinitionCustomNodesContainingPython.SelectedItems.Count == 1)
            {
                CustomNodeItem selectedCNItem = UserDefinitionCustomNodesContainingPython.SelectedItem as CustomNodeItem;
                Guid functionId = selectedCNItem.FunctionId;
                ViewModel.DynamoViewModel.Model.OpenCustomNodeWorkspace(functionId);
            }
        }

        private void ExecuteCtrlCCopyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            string collectedText = "";
            ListBox lb = (ListBox) sender;
            
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
    }

    // Represents the data for each Custom Node item in the listbox. 
    internal class CustomNodeItem
    {
        internal CustomNodeItem(string name, Guid functionId, string packageName) 
        {
            Name = name;
            FunctionId = functionId;
            PackageName = packageName;
        }

        public string Name { get; private set; }
        public string PackageName { get; private set; }
        internal Guid FunctionId { get; private set; }

        internal const string UserDefinitions = "Definitions";

        public override string ToString()
        {
            if (PackageName.Equals(UserDefinitions))
            {
                return Name;
            }
            else
            {
                return PackageName + " : " + Name;
            }
        }
    }
}
