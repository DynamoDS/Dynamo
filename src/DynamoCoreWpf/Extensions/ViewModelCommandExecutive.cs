using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// The ViewModelCommandExecutive provides access to DynamoViewModel and WorkspaceViewModel commands
    /// </summary>
    public class ViewModelCommandExecutive
    {
        private DynamoViewModel dynamoViewModel;

        /// <summary>
        /// Create a Command Executive for a DynamoViewModel
        /// </summary>
        /// <param name="viewModel"></param>
        internal ViewModelCommandExecutive(DynamoViewModel viewModel)
        {
            dynamoViewModel = viewModel;
        }

        /// <summary>
        /// Fit the current workspace view to the current selection
        /// </summary>
        public void FitViewCommand()
        {
            dynamoViewModel.FitViewCommand.Execute(null);
        }

        /// <summary>
        /// Search for an element by its ID and focus the view on it
        /// </summary>
        /// <param name="objectID"></param>
        public void FindByIdCommand(string objectID)
        {
            dynamoViewModel.CurrentSpaceViewModel.FindByIdCommand.Execute(objectID);
        }

        /// <summary>
        /// Force re-execute all nodes in the current workspace
        /// </summary>
        /// <param name="showErrors">Should errors be shown?</param>
        public void ForceRunExpressionCommand(bool showErrors = true)
        {
            dynamoViewModel.ForceRunExpressionCommand.Execute(showErrors);
        }
    }
}
