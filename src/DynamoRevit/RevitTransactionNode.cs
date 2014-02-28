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
using ChangeType = RevitServices.Elements.ChangeType;
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
            get { return DocumentManager.GetInstance().CurrentUIDocument; }
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
            RegisterAllElementsDeleteHook();

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
            var del = new ElementUpdateDelegate(onDeleted);

            elements.Clear();

            var sb = new StringBuilder();
            
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Run")
                {
                    var runElements = new List<ElementId>();
                    elements.Add(runElements);

                    foreach (XmlNode element in subNode.ChildNodes)
                    {
                        if (element.Name == "Element")
                        {
                            var eid = element.InnerText;
                            try
                            {
                                var id = UIDocument.Document.GetElement(eid).Id;
                                runElements.Add(id);
                                dynRevitSettings.Controller.RegisterDMUHooks(id, del);
                            }
                            catch (NullReferenceException)
                            {
                                //DynamoLogger.Instance.Log("Element with UID \"" + eid + "\" not found in Document.");
                                sb.AppendLine("Element with UID \"" + eid + "\" not found in Document.");
                            }
                        }
                    }
                }
            }

            DynamoLogger.Instance.Log(sb.ToString());
        }

        internal void RegisterAllElementsDeleteHook()
        {
            var del = new ElementUpdateDelegate(onDeleted);

            foreach (var id in elements.SelectMany(eList => eList)) 
            {
                dynRevitSettings.Controller.RegisterDMUHooks(id, del);
            }
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

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            #region Register Elements w/ DMU

            var del = new ElementUpdateDelegate(onDeleted);

            foreach (ElementId id in Elements)
                dynRevitSettings.Controller.RegisterDMUHooks(id, del);

            #endregion

            _runCount++;
        }

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

        protected override void __eval_internal(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var controller = dynRevitSettings.Controller;

            if (controller.TransMode != TransactionMode.Debug)
            {
                #region no debug

                if (controller.TransMode == TransactionMode.Manual && !controller.TransactionManager.TransactionActive)
                {
                    throw new Exception("A Revit transaction is required in order evaluate this element.");
                }

                controller.InitTransaction();

                base.__eval_internal(args, outPuts);

                foreach (ElementId eid in _deletedIds)
                {
                    controller.RegisterSuccessfulDeleteHook(
                       eid,
                       onSuccessfulDelete);
                }
                _deletedIds.Clear();

                #endregion
            }
            else
            {
                #region debug

                DynamoLogger.Instance.Log("Starting a debug transaction for element: " + NickName);

                RevThread.IdlePromise.ExecuteOnIdleSync(
                   delegate
                   {
                       controller.InitTransaction();

                       try
                       {
                           base.__eval_internal(args, outPuts);

                           foreach (ElementId eid in _deletedIds)
                           {
                               controller.RegisterSuccessfulDeleteHook(
                                  eid,
                                  onSuccessfulDelete);
                           }
                           _deletedIds.Clear();

                           controller.EndTransaction();

                           ValidateConnections();
                       }
                       catch (Exception)
                       {
                           controller.CancelTransaction();
                           throw;
                       }
                   });

                #endregion
            }
        }

        private readonly List<ElementId> _deletedIds = new List<ElementId>();

        /// <summary>
        /// Deletes an Element from the Document and removes all Dynamo regen hooks. If the second
        /// argument is true, then it will not delete from the Document, but will still remove all
        /// regen hooks.
        /// </summary>
        /// <param name="id">ID belonging to the element to be deleted.</param>
        /// <param name="hookOnly">Whether or not to only remove the regen hooks.</param>
        protected void DeleteElement(ElementId id, bool hookOnly=false)
        {
            if (!hookOnly)
                UIDocument.Document.Delete(id);
            _deletedIds.Add(id);
        }

        /// <summary>
        /// Destroy all elements belonging to this dynElement
        /// </summary>
        public override void Destroy()
        {
            var controller = dynRevitSettings.Controller;

            RevThread.IdlePromise.ExecuteOnIdleAsync(
               delegate
               {
                   controller.InitTransaction();
                   try
                   {
                       _runCount = 0;

                       //TODO: Re-enable once similar functionality is fleshed out for dynFunctionWithRevit
                       //var query = controller.DynamoViewModel.Model.HomeSpace.Nodes
                       //    .OfType<dynFunctionWithRevit>()
                       //    .Select(x => x.ElementsContainer)
                       //    .Where(c => c.HasElements(GUID))
                       //    .SelectMany(c => c[GUID]);

                       foreach (var els in elements)
                       {
                           foreach (ElementId e in els)
                           {
                               try
                               {
                                   DocumentManager.GetInstance().CurrentUIDocument.Document.Delete(e);
                               }
                               catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                               {
                                   //TODO: Flesh out?
                               }
                           }
                           els.Clear();
                       }
                   }
                   catch (Exception ex)
                   {
                       DynamoLogger.Instance.Log(
                          "Error deleting elements: "
                          + ex.GetType().Name
                          + " -- " + ex.Message
                       );
                   }
                   controller.EndTransaction();
                   WorkSpace.Modified();
               });
        }

        void onDeleted(HashSet<ElementId> deleted)
        {
            int count = elements.Sum(els => els.RemoveAll(deleted.Contains));

            if (!isDirty)
                isDirty = count > 0;
        }

        void onSuccessfulDelete(HashSet<ElementId> deleted)
        {
            foreach (var els in elements)
                els.RemoveAll(deleted.Contains);
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

    namespace SyncedNodeExtensions
    {
        public static class ElementSync
        {
            /// <summary>
            /// Registers the given element id with the DMU such that any change in the element will
            /// trigger a workspace modification event (dynamic running and saving).
            /// </summary>
            public static void RegisterEvalOnModified(this NodeModel node, ElementId id, Action modAction=null, Action delAction=null)
            {
                var u = dynRevitSettings.Controller.Updater;
                u.RegisterChangeHook(
                   id,
                   ChangeType.Modify,
                   ReEvalOnModified(node, modAction)
                );
                u.RegisterChangeHook(
                   id,
                   ChangeType.Delete,
                   UnRegOnDelete(delAction)
                );
            }

            /// <summary>
            /// Unregisters the given element id with the DMU. Should not be called unless it has already
            /// been registered with RegisterEvalOnModified
            /// </summary>
            public static void UnregisterEvalOnModified(this NodeModel node, ElementId id)
            {
                var u = dynRevitSettings.Controller.Updater;
                u.UnRegisterChangeHook(
                   id, ChangeType.Modify
                );
                u.UnRegisterChangeHook(
                   id, ChangeType.Delete
                );
            }

            static ElementUpdateDelegate UnRegOnDelete(Action deleteAction)
            {
                return delegate(HashSet<ElementId> deleted)
                {
                    foreach (var d in deleted)
                    {
                        var u = dynRevitSettings.Controller.Updater;
                        u.UnRegisterChangeHook(d, ChangeType.Delete);
                        u.UnRegisterChangeHook(d, ChangeType.Modify);
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
}
