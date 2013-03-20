using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Nodes;
using Dynamo.Utilities;
using Autodesk.Revit.DB;

using Value = Dynamo.FScheme.Value;
using Microsoft.FSharp.Collections;
using Dynamo.Connectors;

namespace Dynamo.Revit
{
    public abstract class dynRevitTransactionNode : dynNode
    {
        //TODO: Move from dynElementSettings to another static area in DynamoRevit
        protected Autodesk.Revit.UI.UIDocument UIDocument
        {
            get { return dynRevitSettings.Doc; }
        }

        private List<List<ElementId>> elements
        {
            get
            {
                return dynRevitSettings.ElementsContainers.Peek()[this];
            }
        }

        public List<ElementId> Elements
        {
            get
            {
                while (elements.Count <= runCount)
                    elements.Add(new List<ElementId>());
                return elements[runCount];
            }
            private set
            {
                elements[runCount] = value;
            }
        }

        //TODO: Move handling of increments to wrappers for eval. Should never have to touch this in subclasses.
        /// <summary>
        /// Implementation detail, records how many times this Element has been executed during this run.
        /// </summary>
        private int runCount;

        internal void ResetRuns()
        {
            if (runCount > 0)
            {
                PruneRuns(runCount);
                runCount = 0;
            }
        }

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            runCount++;
        }

        internal void PruneRuns(int runCount)
        {
            for (int i = elements.Count - 1; i >= runCount; i--)
            {
                var elems = elements[i];
                foreach (var e in elems)
                {
                    UIDocument.Document.Delete(e);
                }
                elems.Clear();
            }

            if (elements.Count > runCount)
            {
                elements.RemoveRange(
                   runCount,
                   elements.Count - runCount
                );
            }
        }

        protected override void __eval_internal(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var controller = dynRevitSettings.Controller;

            bool debug = controller.RunInDebug;

            if (!debug)
            {
                #region no debug

                if (controller.TransMode == DynamoController_Revit.TransactionMode.Manual && !controller.IsTransactionActive())
                {
                    throw new Exception("A Revit transaction is required in order evaluate this element.");
                }

                controller.InitTransaction();

                Evaluate(args, outPuts);

                foreach (ElementId eid in deletedIds)
                {
                    controller.RegisterSuccessfulDeleteHook(
                       eid,
                       onSuccessfulDelete
                    );
                }
                deletedIds.Clear();

                #endregion
            }
            else
            {
                #region debug

                Bench.Dispatcher.Invoke(new Action(
                   () =>
                      Bench.Log("Starting a debug transaction for element: " + NodeUI.NickName)
                ));

                IdlePromise.ExecuteOnIdle(
                   delegate
                   {
                       controller.InitTransaction();

                       try
                       {
                           Evaluate(args, outPuts);

                           foreach (ElementId eid in deletedIds)
                           {
                               controller.RegisterSuccessfulDeleteHook(
                                  eid,
                                  onSuccessfulDelete
                               );
                           }
                           deletedIds.Clear();

                           controller.EndTransaction();

                           //NodeUI.Dispatcher.BeginInvoke(new Action(
                           //    delegate
                           //    {
                           //        NodeUI.UpdateLayout();
                           //        NodeUI.ValidateConnections();
                           //    }
                           //));
                       }
                       catch (Exception ex)
                       {
                           controller.CancelTransaction();
                           throw ex;
                       }
                   }
                );

                #endregion
            }

            #region Register Elements w/ DMU

            var del = new DynElementUpdateDelegate(onDeleted);

            foreach (ElementId id in Elements)
                controller.RegisterDeleteHook(id, del);

            #endregion
        }

        private List<ElementId> deletedIds = new List<ElementId>();
        protected void DeleteElement(ElementId id, bool hookOnly = false)
        {
            if (!hookOnly)
                UIDocument.Document.Delete(id);
            deletedIds.Add(id);
        }

        /// <summary>
        /// Destroy all elements belonging to this dynElement
        /// </summary>
        public override void Destroy()
        {
            var controller = dynRevitSettings.Controller;

            IdlePromise.ExecuteOnIdle(
               delegate
               {
                   controller.InitTransaction();
                   try
                   {
                       runCount = 0;

                       var query = controller.HomeSpace.Nodes
                           .Where(x => x is dynFunctionWithRevit)
                           .Select(x => (x as dynFunctionWithRevit).ElementsContainer)
                           .Where(c => c.HasElements(this))
                           .SelectMany(c => c[this]);

                       foreach (var els in query)
                       {
                           foreach (ElementId e in els)
                           {
                               try
                               {
                                   dynRevitSettings.Doc.Document.Delete(e);
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
                       Bench.Log(
                          "Error deleting elements: "
                          + ex.GetType().Name
                          + " -- " + ex.Message
                       );
                   }
                   controller.EndTransaction();
                   WorkSpace.Modified();
               },
               true
            );
        }

        void onDeleted(List<ElementId> deleted)
        {
            int count = 0;
            foreach (var els in elements)
            {
                count += els.RemoveAll(deleted.Contains);
            }

            if (!isDirty)
                isDirty = count > 0;
        }

        void onSuccessfulDelete(List<ElementId> deleted)
        {
            foreach (var els in elements)
                els.RemoveAll(x => deleted.Contains(x));
        }
    }

    public class dynRevitTransactionNodeWithOneOutput : dynRevitTransactionNode
    {
        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            outPuts[OutPortData[0]] = Evaluate(args);
        }

        public virtual Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }
    }

    namespace SyncedNodeExtensions
    {
        public static class ElementSync
        {
            /// <summary>
            /// Registers the given element id with the DMU such that any change in the element will
            /// trigger a workspace modification event (dynamic running and saving).
            /// </summary>
            /// <param name="id">ElementId of the element to watch.</param>
            public static void RegisterEvalOnModified(this dynNode node, ElementId id, Action modAction = null, Action delAction = null)
            {
                var u = dynRevitSettings.Controller.Updater;
                u.RegisterChangeHook(
                   id,
                   ChangeTypeEnum.Modify,
                   ReEvalOnModified(node, modAction)
                );
                u.RegisterChangeHook(
                   id,
                   ChangeTypeEnum.Delete,
                   UnRegOnDelete(delAction)
                );
            }

            /// <summary>
            /// Unregisters the given element id with the DMU. Should not be called unless it has already
            /// been registered with RegisterEvalOnModified
            /// </summary>
            /// <param name="id">ElementId of the element to stop watching.</param>
            public static void UnregisterEvalOnModified(this dynNode node, ElementId id)
            {
                var u = dynRevitSettings.Controller.Updater;
                u.UnRegisterChangeHook(
                   id, ChangeTypeEnum.Modify
                );
                u.UnRegisterChangeHook(
                   id, ChangeTypeEnum.Delete
                );
            }

            static DynElementUpdateDelegate UnRegOnDelete(Action deleteAction)
            {
                return delegate(List<ElementId> deleted)
                {
                    foreach (var d in deleted)
                    {
                        var u = dynRevitSettings.Controller.Updater;
                        u.UnRegisterChangeHook(d, ChangeTypeEnum.Delete);
                        u.UnRegisterChangeHook(d, ChangeTypeEnum.Modify);
                    }
                    if (deleteAction != null)
                        deleteAction();
                };
            }

            static DynElementUpdateDelegate ReEvalOnModified(dynNode node, Action modifiedAction)
            {
                return delegate(List<ElementId> modified)
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
