using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Dynamo.DSEngine;
using Dynamo.Models;

using Microsoft.Practices.Prism.ViewModel;
using ProtoScript.Runners;

namespace Dynamo.ViewModels
{
    public class LiveRunnerViewModel : NotificationObject
    {
        private EngineController controller;
        private DynamoViewModel vm;

        internal ChangeSetComputer CSComputer
        {
            get
            {
                return controller.LiveRunnerServices.LiveRunner.ChangeSetComputer;
            }
        }

        internal SyncDataManager SyncDataManager
        {
            get { return controller.SyncDataManager; }
        }

        public LiveRunnerViewModel(DynamoViewModel vm)
        {
            this.vm = vm;
            controller = vm.model.EngineController;

#if DEBUG
            vm.Model.EngineReset += Model_EngineReset;
            CSComputer.PropertyChanged += ChangeSetComputerOnPropertyChanged;
            SyncDataManager.PropertyChanged += SyncDataManager_PropertyChanged;
#endif
        }

        void Model_EngineReset(EngineResetEventArgs args)
        {
            controller = args.Controller;
            CSComputer.PropertyChanged += ChangeSetComputerOnPropertyChanged;
            SyncDataManager.PropertyChanged += SyncDataManager_PropertyChanged;
        }

        void SyncDataManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "States")
                return;

            var vmNodes = vm.CurrentSpaceViewModel.Nodes.ToList();

            foreach (var kvp in SyncDataManager.States)
            {
                var nodeVM = vmNodes.FirstOrDefault(n => n.NodeModel.GUID == kvp.Key);
                if (nodeVM == null)
                    continue;

                nodeVM.SyncState = kvp.Value;
            }
        }

        private void ChangeSetComputerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName != "ASTCache") 
                return;

            var vmNodes = vm.CurrentSpaceViewModel.Nodes.ToList();

            //Update all nodes depending on if they are in the change set computer.
            foreach (var kvp in CSComputer.ASTCache)
            {
                var guid = kvp.Key;
                var nodes = kvp.Value;

                var sb = new StringBuilder();
                //sb.AppendLine(string.Format("{0} AST:", kvp.Key));

                foreach (var assocNode in nodes)
                {
                    var pretty = assocNode.ToString();

                    //shorten the guids
                    var strRegex = @"([0-9a-f-]{32}).*?";
                    var myRegex = new Regex(strRegex, RegexOptions.None);
                    string strTargetString = assocNode.ToString();

                    foreach (Match myMatch in myRegex.Matches(strTargetString))
                    {
                        if (myMatch.Success)
                        {
                            pretty = pretty.Replace(myMatch.Value, "..." + myMatch.Value.Substring(myMatch.Value.Length - 7));
                        }
                    }
                    sb.AppendLine(pretty);
                }

                // Find the matching node
                var nodeVM = vmNodes.FirstOrDefault(n => n.NodeModel.GUID == kvp.Key);
                if (nodeVM == null)
                    continue;

                nodeVM.ASTText = sb.ToString();
            }
        }
    }
}
