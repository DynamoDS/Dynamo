using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Autodesk.Revit.DB;

using DSNodeServices;

using Dynamo.Models;
using Dynamo.Nodes;

using ProtoCore;

using RevitServices.Persistence;

namespace Dynamo.Applications
{
    public class Reactor
    {
        private readonly DynamoRevitModel dynamoRevitModel;

        internal Reactor(DynamoRevitModel dynamoRevitModel)
        {
            dynamoRevitModel.RevitUpdater.ElementsDeleted += OnElementsDeleted;
        }

        private void OnElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            if (!deleted.Any())
                return;

            var workspace = dynamoRevitModel.CurrentWorkspace;

            ProtoCore.Core core = null;
            var engine = dynamoRevitModel.EngineController;
            if (engine != null && (engine.LiveRunnerCore != null))
                core = engine.LiveRunnerCore;

            if (core == null) // No execution yet as of this point.
                return;

            // Selecting all nodes that are either a DSFunction,
            // a DSVarArgFunction or a CodeBlockNodeModel into a list.
            var nodeGuids = workspace.Nodes.Where((n) =>
            {
                return (n is DSFunction 
                    || (n is DSVarArgFunction)
                    || (n is CodeBlockNodeModel));
            }).Select((n) => n.GUID);

            var nodeTraceDataList = core.GetCallsitesForNodes(nodeGuids);// core.GetTraceDataForNodes(nodeGuids);

            foreach (Guid guid in nodeTraceDataList.Keys)
            {
                foreach (CallSite cs in nodeTraceDataList[guid])
                {
                    foreach (CallSite.SingleRunTraceData srtd in cs.TraceData)
                    {
                        List<ISerializable> traceData = srtd.RecursiveGetNestedData();

                        foreach (ISerializable thingy in traceData)
                        {
                            SerializableId sid = thingy as SerializableId;

                            foreach (ElementId eid in deleted)
                            {

                                if (sid != null)
                                {
                                    if (sid.IntID == eid.IntegerValue)
                                    {
                                        NodeModel inm =
                                            workspace.Nodes.Where((n) => n.GUID == guid).FirstOrDefault();

                                        Validity.Assert(inm != null, "The bound node has disappeared");

                                        inm.RequiresRecalc = true;
                                        inm.ForceReExecuteOfNode = true;

                                        //FOUND IT!
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}