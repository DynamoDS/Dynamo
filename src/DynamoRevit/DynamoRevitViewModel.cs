
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

using Autodesk.Revit.DB;
using Dynamo.Applications;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Selection;
using Dynamo.UpdateManager;
using Dynamo.Utilities;

using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Dynamo
{
    public class DynamoRevitViewModel : ViewModels.DynamoViewModel
    {

        /// <summary>
        ///     The Synchronication Context from the current thread.  This is expected to be the
        ///     Revit UI thread SynchronizationContext
        /// </summary>
        public Dispatcher RevitSyncContext { get; set; }

        public DynamoRevitViewModel(DynamoModel dynamoModel, IWatchHandler watchHandler, IVisualizationManager vizManager, string commandFilePath) : 
            base(dynamoModel, watchHandler, vizManager, commandFilePath)
        {

        }

        internal void FindNodesFromSelection()
        {
            IEnumerable<ElementId> selectedIds =
                DocumentManager.Instance.CurrentUIDocument.Selection.Elements.Cast<Element>()
                    .Select(x => x.Id);
            IEnumerable<RevitTransactionNode> transNodes =
                this.Model.CurrentWorkspace.Nodes.OfType<RevitTransactionNode>();
            List<RevitTransactionNode> foundNodes =
                transNodes.Where(x => x.AllElements.Intersect(selectedIds).Any()).ToList();

            if (foundNodes.Any())
            {
                this.CurrentSpaceViewModel.OnRequestCenterViewOnElement(
                    this,
                    new ModelEventArgs(foundNodes.First()));

                DynamoSelection.Instance.ClearSelection();
                foundNodes.ForEach(DynamoSelection.Instance.Selection.Add);
            }
        }


    }
}