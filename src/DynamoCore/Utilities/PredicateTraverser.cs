using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    public class PredicateTraverser
    {
        private readonly Predicate<NodeModel> _predicate;

        private readonly Dictionary<NodeModel, bool> _resultDict = new Dictionary<NodeModel, bool>();

        private bool _inProgress;

        public PredicateTraverser(Predicate<NodeModel> p)
        {
            _predicate = p;
        }

        public bool TraverseUntilAny(NodeModel entry)
        {
            _inProgress = true;
            bool result = TraverseAny(entry);
            _resultDict.Clear();
            _inProgress = false;
            return result;
        }

        public bool ContinueTraversalUntilAny(NodeModel entry)
        {
            if (_inProgress)
                return TraverseAny(entry);
            throw new Exception("ContinueTraversalUntilAny cannot be used except in a traversal predicate.");
        }

        private bool TraverseAny(NodeModel entry)
        {
            bool result;
            if (_resultDict.TryGetValue(entry, out result))
                return result;

            result = _predicate(entry);
            _resultDict[entry] = result;
            if (result)
                return true;

            if (entry is CustomNodeInstance)
            {
                Guid symbol = (entry as CustomNodeInstance).Definition.FunctionId;
                if (!DynamoSettings.Controller.CustomNodeManager.Contains(symbol))
                {
                    DynamoLogger.Instance.Log("WARNING -- No implementation found for node: " + symbol);
                    entry.Error("Could not find .dyf definition file for this node.");
                    return false;
                }

                result =
                    DynamoSettings.Controller.CustomNodeManager.GetFunctionDefinition(symbol)
                        .WorkspaceModel.GetTopMostNodes()
                        .Any(ContinueTraversalUntilAny);
            }
            _resultDict[entry] = result;
            return result || entry.Inputs.Values.Any(x => x != null && TraverseAny(x.Item2));
        }
    }
}