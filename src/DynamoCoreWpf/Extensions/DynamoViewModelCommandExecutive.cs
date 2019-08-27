using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// The DynamoViewModelCommandExecutive provides access to DynamoViewModel commands
    /// </summary>
    public class DynamoViewModelCommandExecutive
    {
        private DynamoViewModel dynamoViewModel;

        /// <summary>
        /// Create a Command Executive for a DynamoViewModel
        /// </summary>
        /// <param name="model"></param>
        internal DynamoViewModelCommandExecutive(DynamoViewModel model)
        {
            dynamoViewModel = model;
        }

        /// <summary>
        /// Fit the current workspace view to the current selection
        /// </summary>
        public void FitViewCommand()
        {
            dynamoViewModel.FitViewCommand.Execute(null);
        }
    }
}
