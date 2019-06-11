using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Dynamo.PackageDependency
{
    /// <summary>
    /// Interaction logic for PackageDependencyView.xaml
    /// </summary>
    public partial class PackageDependencyView : UserControl
    {

        internal IEnumerable<Graph.Workspaces.PackageDependencyInfo> Packages;

        private DynamoModel dynamoModel;

        public void WorkspaceOpened(WorkspaceModel obj)
        {
            Packages = obj.PackageDependencies;
        }

        public PackageDependencyView(DynamoViewModel dynamoViewModel)
        {
            Packages = dynamoViewModel.Model.CurrentWorkspace.PackageDependencies;
            dynamoModel = dynamoViewModel.Model;
            dynamoViewModel.Model.WorkspaceAdded += WorkspaceOpened;
            InitializeComponent();
        }
    }

}
