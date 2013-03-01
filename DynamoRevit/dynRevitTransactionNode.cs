using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Nodes;
using Dynamo.Utilities;
using Autodesk.Revit.DB;

using Value = Dynamo.FScheme.Value;
using Microsoft.FSharp.Collections;

namespace Dynamo.Revit
{
    public abstract class dynRevitTransactionNode : dynNode
    {
        //TODO: Move from dynElementSettings to another static area in DynamoRevit
        protected Autodesk.Revit.UI.UIDocument UIDocument
        {
            get { return dynRevitSettings.Doc; }
        }

        private List<List<ElementId>> elements;
        public List<ElementId> Elements
        {
            get
            {
                while (this.elements.Count <= this.runCount)
                    this.elements.Add(new List<ElementId>());
                return this.elements[this.runCount];
            }
            private set
            {
                this.elements[this.runCount] = value;
            }
        }

        //TODO: Move handling of increments to wrappers for eval. Should never have to touch this in subclasses.
        /// <summary>
        /// Implementation detail, records how many times this Element has been executed during this run.
        /// </summary>
        private int runCount;

        public dynRevitTransactionNode()
            : base()
        {
            elements = new List<List<ElementId>>() { new List<ElementId>() };
        }

        internal void ResetRuns()
        {
            if (this.runCount > 0)
            {
                PruneRuns(this.runCount);
                this.runCount = 0;
            }
        }

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            this.runCount++;
        }

        internal void PruneRuns(int runCount)
        {
            for (int i = this.elements.Count - 1; i >= runCount; i--)
            {
                var elems = this.elements[i];
                foreach (var e in elems)
                {
                    this.UIDocument.Document.Delete(e);
                }
                elems.Clear();
            }

            if (this.elements.Count > runCount)
            {
                this.elements.RemoveRange(
                   runCount,
                   this.elements.Count - runCount
                );
            }
        }

        protected override Value __eval_internal(FSharpList<Value> args)
        {
            Value result = null;

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

                result = this.Evaluate(args);

                foreach (ElementId eid in this.deletedIds)
                {
                    controller.RegisterSuccessfulDeleteHook(
                       eid,
                       onSuccessfulDelete
                    );
                }
                this.deletedIds.Clear();

                #endregion
            }
            else
            {
                #region debug

                Bench.Dispatcher.Invoke(new Action(
                   () =>
                      Bench.Log("Starting a debug transaction for element: " + this.NodeUI.NickName)
                ));

                result = IdlePromise<Value>.ExecuteOnIdle(
                   delegate
                   {
                       controller.InitTransaction();

                       try
                       {
                           var exp = this.Evaluate(args);

                           foreach (ElementId eid in this.deletedIds)
                           {
                               controller.RegisterSuccessfulDeleteHook(
                                  eid,
                                  onSuccessfulDelete
                               );
                           }
                           this.deletedIds.Clear();

                           controller.EndTransaction();

                           this.NodeUI.Dispatcher.BeginInvoke(new Action(
                               delegate
                               {
                                   this.NodeUI.UpdateLayout();
                                   this.NodeUI.ValidateConnections();
                               }
                           ));

                           return exp;
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

            var del = new DynElementUpdateDelegate(this.onDeleted);

            foreach (ElementId id in this.Elements)
                controller.RegisterDeleteHook(id, del);

            #endregion

            return result;
        }

        private List<ElementId> deletedIds = new List<ElementId>();
        protected void DeleteElement(ElementId id, bool hookOnly = false)
        {
            if (!hookOnly)
                this.UIDocument.Document.Delete(id);
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
                       this.runCount = 0;
                       foreach (var els in this.elements)
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
                           //els.Clear();
                       }

                       //clear out the array to avoid object initialization errors
                       elements.Clear();

                       //clear the data tree
                       //dataTree.Clear();
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
                   this.WorkSpace.Modified();
               },
               true
            );
        }

        void onDeleted(List<ElementId> deleted)
        {
            int count = 0;
            foreach (var els in this.elements)
            {
                count += els.RemoveAll(deleted.Contains);
            }

            if (!this.isDirty)
                this.isDirty = count > 0;
        }

        void onSuccessfulDelete(List<ElementId> deleted)
        {
            foreach (var els in this.elements)
                els.RemoveAll(x => deleted.Contains(x));
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
