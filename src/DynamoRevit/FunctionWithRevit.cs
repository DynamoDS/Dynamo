using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using Microsoft.FSharp.Collections;
using Dynamo.Utilities;
using Dynamo.Revit;
using System.Xml;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Threading;
using RevThread = RevitServices.Threading;

namespace Dynamo.Nodes
{
    public class FunctionWithRevit : Function
    {
        internal ElementsContainer ElementsContainer = new ElementsContainer();

        protected internal FunctionWithRevit(IEnumerable<string> inputs, IEnumerable<string> outputs, CustomNodeDefinition customNodeDefinition)
            : base(inputs, outputs, customNodeDefinition)
        { }

        public FunctionWithRevit() { }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            dynRevitSettings.ElementsContainers.Push(ElementsContainer);
            var result = base.Evaluate(args);
            dynRevitSettings.ElementsContainers.Pop();
            return result;
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);

            if (context == SaveContext.Copy)
                return;

            foreach (var node in ElementsContainer.Nodes)
            {
                var outEl = xmlDoc.CreateElement("InnerNode");
                outEl.SetAttribute("id", node.ToString());

                foreach (var run in ElementsContainer[node])
                {
                    var runEl = xmlDoc.CreateElement("Run");

                    foreach (var id in run)
                    {
                        Element e;
                        if (dynUtils.TryGetElement(id, out e))
                        {
                            var elementStore = xmlDoc.CreateElement("Element");
                            elementStore.InnerText = e.UniqueId;
                            runEl.AppendChild(elementStore);
                        }
                    }

                    outEl.AppendChild(runEl);
                }

                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);

            ElementsContainer.Clear();

            foreach (XmlNode node in nodeElement.ChildNodes)
            {
                if (node.Name == "InnerNode")
                {
                    var nodeId = new Guid(node.Attributes["id"].Value);
                    var runs = ElementsContainer[nodeId];
                    runs.Clear();

                    foreach (XmlNode run in node.ChildNodes)
                    {
                        if (run.Name == "Run")
                        {
                            var runElements = new List<ElementId>();
                            runs.Add(runElements);

                            var query = from XmlNode element in run.ChildNodes
                                        where element.Name == "Element"
                                        select element.InnerText;

                            foreach (var eid in query) 
                            {
                                try
                                {
                                    runElements.Add(DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(eid).Id);
                                }
                                catch (NullReferenceException)
                                {
                                    DynamoLogger.Instance.Log("Element with UID \"" + eid + "\" not found in Document.");
                                }
                            }
                        }
                    }
                    var rNode = Definition.WorkspaceModel.Nodes.FirstOrDefault(x => x.GUID == nodeId) as RevitTransactionNode;
                    if (rNode != null)
                        rNode.RegisterAllElementsDeleteHook();
                }
            }
        }

        public override void Destroy()
        {
            RevThread.IdlePromise.ExecuteOnIdleAsync(
               delegate
               {
                   dynRevitSettings.Controller.InitTransaction();
                   try
                   {
                       ElementsContainer.DestroyAll();
                   }
                   catch (Exception ex)
                   {
                       DynamoLogger.Instance.Log(
                          "Error deleting elements: "
                          + ex.GetType().Name
                          + " -- " + ex.Message);
                   }
                   dynRevitSettings.Controller.EndTransaction();
                   WorkSpace.Modified();
               });
        }
    }
}
