using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

using Autodesk.Revit.DB;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

using Dynamo.Utilities;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Threading;
using RevitServices.Transactions;
using Value = Dynamo.FScheme.Value;
using RevThread = RevitServices.Threading;

namespace Dynamo.Revit
{
    public abstract partial class RevitTransactionNode : NodeModel
    {
        protected object DrawableObject = null;
        //protected Func<object, RenderDescription> DrawMethod = null;

        //private Type base_type = null;

        //TODO: Move from dynElementSettings to another static area in DynamoRevit
        protected Autodesk.Revit.UI.UIDocument UIDocument
        {
            get { return DocumentManager.Instance.CurrentUIDocument; }
        }

        // this contains a list of all the elements created over all previous
        // recursive runs over the node. subsequest runs executed via the 'Run'
        // button or 'Run Automatically' are stored in an external map
        // To get all the Elements associated with this node, flatten this list
        private List<List<ElementId>> elements
        {
            get
            {
                return dynRevitSettings.ElementsContainers.Peek()[GUID];
            }
        }

        // This list contains the elements of the current recurvise execution
        public List<ElementId> Elements
        {
            get
            {
                while (elements.Count <= _runCount)
                    elements.Add(new List<ElementId>());
                return elements[_runCount];
            }
        }

        public IEnumerable<ElementId> AllElements
        {
            get
            {
                return elements.SelectMany(x => x);
            }
        }

        protected RevitTransactionNode()
        {
            ArgumentLacing = LacingStrategy.Longest;
            //RegisterAllElementsDeleteHook();

            dynRevitSettings.Controller.RevitDocumentChanged += Controller_RevitDocumentChanged;
        }

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            Elements.Clear();
        }

        protected override void 
            SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Don't copy over stored references
            if (context == SaveContext.Copy)
                return;

            //Only save elements in the home workspace
            if (WorkSpace is CustomNodeWorkspaceModel)
                return;

            foreach (var run in elements)
            {
                var outEl = xmlDoc.CreateElement("Run");

                foreach (var id in run)
                {
                    Element e;
                    if (dynUtils.TryGetElement(id, out e))
                    {
                        var elementStore = xmlDoc.CreateElement("Element");
                        elementStore.InnerText = e.UniqueId;
                        outEl.AppendChild(elementStore);
                    }
                }
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            elements.Clear();

            var sb = new StringBuilder();
            
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Run")
                {
                    var runElements = new List<ElementId>();
                    elements.Add(runElements);

                    foreach (var eid in from XmlNode element in subNode.ChildNodes where element.Name == "Element" select element.InnerText) 
                    {
                        try
                        {
                            var id = UIDocument.Document.GetElement(eid).Id;
                            runElements.Add(id);
                        }
                        catch (NullReferenceException)
                        {
                            //DynamoLogger.Instance.Log("Element with UID \"" + eid + "\" not found in Document.");
                            sb.AppendLine("Element with UID \"" + eid + "\" not found in Document.");
                        }
                    }
                }
            }

            DynamoLogger.Instance.Log(sb.ToString());
        }

        //TODO: Move handling of increments to wrappers for eval. Should never have to touch this in subclasses.
        /// <summary>
        /// Implementation detail, records how many times this Element has been executed during this run.
        /// </summary>
        private int _runCount;

        internal void ResetRuns()
        {
            if (_runCount <= 0) 
                return;

            PruneRuns(_runCount);
            _runCount = 0;
        }

        //protected override void OnEvaluate()
        //{
        //    base.OnEvaluate();

        //    #region Register Elements w/ DMU

        //    var del = new ElementUpdateDelegate(onDeleted);

        //    foreach (ElementId id in Elements)
        //        dynRevitSettings.Controller.RegisterDMUHooks(id, del);

        //    #endregion

        //    _runCount++;
        //}

        private void PruneRuns(int numRuns)
        {
            Debug.WriteLine(string.Format("Pruning runs from {0} to {1}", elements.Count, numRuns));

            for (int i = elements.Count - 1; i >= numRuns; i--)
            {
                var elems = elements[i];
                var query = from e in elems
                            let el = UIDocument.Document.GetElement(e)
                            where el != null
                            select e;

                foreach (var e in query)
                    UIDocument.Document.Delete(e);

                elems.Clear();
            }

            if (elements.Count > numRuns)
            {
                elements.RemoveRange(
                   numRuns,
                   elements.Count - numRuns);
            }
        }
    }

    public abstract class RevitTransactionNodeWithOneOutput : RevitTransactionNode
    {
        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            outPuts[OutPortData[0]] = Evaluate(args);
        }

        public abstract Value Evaluate(FSharpList<Value> args);
    }
    /*
    namespace SyncedNodeExtensions
    {
        public static class ElementSync
        {
            /// <summary>
            /// Registers the given element id with the DMU such that any change in the element will
            /// trigger a workspace modification event (dynamic running and saving).
            /// </summary>
            public static void RegisterEvalOnModified(this NodeModel node, Document doc, ElementId id, Action modAction=null, Action delAction=null)
            {
                var u = dynRevitSettings.Controller.Updater;
                u.RegisterModifyCallback(doc.GetElement(id).UniqueId, ReEvalOnModified(node, modAction));
                u.RegisterDeleteCallback(id, UnRegOnDelete(delAction));
            }

            /// <summary>
            /// Unregisters the given element id with the DMU. Should not be called unless it has already
            /// been registered with RegisterEvalOnModified
            /// </summary>
            public static void UnregisterEvalOnModified(this NodeModel node, Document doc, ElementId id)
            {
                var u = dynRevitSettings.Controller.Updater;
                u.UnRegisterModifyCallback(doc.GetElement(id).UniqueId);
                u.UnRegisterDeleteCallback(id);
            }

            static ElementDeleteDelegate UnRegOnDelete(Action deleteAction)
            {
                return delegate(Document doc, IEnumerable<ElementId> deleted)
                {
                    foreach (var d in deleted)
                    {
                        var u = dynRevitSettings.Controller.Updater;
                        u.UnRegisterDeleteCallback(d);
                    }
                    if (deleteAction != null)
                        deleteAction();
                };
            }

            static ElementUpdateDelegate ReEvalOnModified(NodeModel node, Action modifiedAction)
            {
                return delegate
                {
                    if (!node.RequiresRecalc && !dynRevitSettings.Controller.Running)
                    {
                        if (modifiedAction != null)
                            modifiedAction();
                        node.RequiresRecalc = true;
                    }
                };
            }
        }
    }
     */
}
