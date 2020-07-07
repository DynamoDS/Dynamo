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
            var customNodesWithPythonDependencies = GraphPythonDependencies.CustomNodePythonDependency
                                                            .Where(x => x.Value.Equals(GraphPythonDependencies.CNPythonDependency.DirectDependency));

            if (customNodesWithPythonDependencies.Any())
            {
                foreach (var entry in customNodesWithPythonDependencies)
                {
                    ViewModel.CustomNodeManager.TryGetNodeInfo(entry.Key, out CustomNodeInfo customNodeInfo);

                    CustomNodeItem customNodeItem = new CustomNodeItem
                    {
                        Name = customNodeInfo.Name,
                        FunctionId = customNodeInfo.FunctionId,
                        PackageName = customNodeInfo.PackageInfo != null ? customNodeInfo.PackageInfo.Name : "Loose",
                    };

                    if (customNodeInfo.PackageInfo != null)
                    {
                        PackagedCustomNodesContainingPython.Items.Add(customNodeItem);
                    }
                    else
                    {
                        LooseCustomNodesContainingPython.Items.Add(customNodeItem);
                    }
                }

                if (PackagedCustomNodesContainingPython.Items.Count > 0)
                {
                    ExpanderForPackagedDependencies.Visibility = Visibility.Visible;
                }

                if (LooseCustomNodesContainingPython.Items.Count > 0)
                {
                    ExpanderForLooseDependencies.Visibility = Visibility.Visible;
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

        private void OnCustomNodeClick(object sender, RoutedEventArgs e)
        {
            if (LooseCustomNodesContainingPython.SelectedItems.Count == 1)
            {
                CustomNodeItem selectedCNItem = LooseCustomNodesContainingPython.SelectedItem as CustomNodeItem;
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
                collectedText += item.ToString() + "\r\n";
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
        public string Name { get; set; }
        public string PackageName { get; set; }
        internal Guid FunctionId { get; set; }

        public override string ToString()
        {
            if (PackageName.Equals("Loose"))
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
