﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Revit;
using Dynamo.Selection;
using Dynamo.Utilities;
using Value = Dynamo.FScheme.Value;

namespace Dynamo
{
    public class DynamoController_Revit : DynamoController
    {
        public DynamoUpdater Updater { get; private set; }

        dynamic oldPyEval;

        public DynamoController_Revit(FSchemeInterop.ExecutionEnvironment env, DynamoUpdater updater, Type viewModelType, string context)
            : base(env, viewModelType, context)
        {
            Updater = updater;
            
            dynRevitSettings.Controller = this;

            //AppDomain currentDomain = AppDomain.CurrentDomain;
            //currentDomain.AssemblyResolve += ResolveSSONETHandler;

            dynSettings.PackageManagerClient.AuthenticationRequested += RegisterSingleSignOn;

            AddPythonBindings();
            AddWatchNodeHandler();

            dynRevitSettings.Revit.Application.DocumentClosed += Application_DocumentClosed;
            dynRevitSettings.Revit.Application.DocumentOpened += Application_DocumentOpened;

            //allow the showing of elements in context
            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.CanFindNodesFromElements = true;
            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.FindNodesFromElements =
                new Action(FindNodesFromSelection);
        }

        void RegisterSingleSignOn(PackageManagerClient client)
        {
            var ads = Autodesk.Revit.AdWebServicesBase.GetInstance();
            client.Client.Provider = new Greg.RevitOxygenProvider(ads);
        }

        /// <summary>
        /// This module is dependent on the IntfSPD assembly, which will not be in the 
        /// executing directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Contains the name of the assembly</param>
        /// <returns></returns>
        private Assembly ResolveSSONETHandler(object sender, ResolveEventArgs args)
        {
            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            Assembly MyAssembly, objExecutingAssemblies;
            string strTempAssmbPath = "";

            if (args.Name.Substring(0, args.Name.IndexOf(",")).ToLower() == "SSONET.dll".ToLower())
            {
                if (this.Context == "Revit 2013")
                {
                    strTempAssmbPath = @"C:\Program Files\Autodesk\Revit Architecture 2013\Program\SSONET.dll";
                }
                else if (this.Context == "Revit 2014")
                {
                    strTempAssmbPath = @"C:\Program Files\Autodesk\Revit Architecture 2014\Program\SSONET.dll";
                }
                else if (this.Context == "Vasari 2014") {
                    strTempAssmbPath = @"C:\Program Files\Autodesk\Vasari Beta 3\Program\SSONET.dll";
                }
            } else
            {
                
            }

            //Load the assembly from the specified path. 					
            MyAssembly = Assembly.LoadFrom(strTempAssmbPath);

            //Return the loaded assembly.
            return MyAssembly;	
        }

        void FindNodesFromSelection()
        {
            var selectedIds = dynRevitSettings.Doc.Selection.Elements.Cast<Element>().Select(x=>x.Id);
            var transNodes = dynSettings.Controller.DynamoModel.CurrentSpace.Nodes.Where(x => x is dynRevitTransactionNode).Cast<dynRevitTransactionNode>();
            var foundNodes = transNodes.Where(x => x.AllElements.Intersect(selectedIds).Any());

            if (foundNodes.Any())
            {
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestCenterViewOnElement(this, new ModelEventArgs(foundNodes.First(), null));
                DynamoSelection.Instance.ClearSelection();
                foundNodes.ToList().ForEach(x=>DynamoSelection.Instance.Selection.Add(x));
            }
        }

        void Application_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            //when a document is opened 
            if (dynRevitSettings.Doc == null)
            {
                dynRevitSettings.Doc = dynRevitSettings.Revit.ActiveUIDocument;
                this.DynamoViewModel.RunEnabled = true;
            }
        }

        void Application_DocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs e)
        {
            //Disable running against revit without a document
            if (dynRevitSettings.Revit.ActiveUIDocument == null)
            {
                dynRevitSettings.Doc = null;
                DynamoViewModel.RunEnabled = false;
            }
            else
            {
                dynRevitSettings.Doc = dynRevitSettings.Revit.ActiveUIDocument;
                DynamoViewModel.RunEnabled = true;
            }
        }

        #region Python Nodes Revit Hooks
        private delegate void LogDelegate(string msg);
        private delegate void SaveElementDelegate(Element e);

        void AddPythonBindings()
        {
            try
            {
                var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                Assembly ironPythonAssembly = null;

                string path;

                if (File.Exists(path = Path.Combine(assemblyPath, "DynamoPython.dll")))
                {
                    ironPythonAssembly = Assembly.LoadFrom(path);
                }
                else if (File.Exists(path = Path.Combine(assemblyPath, "Packages", "IronPython", "DynamoPython.dll")))
                {
                    ironPythonAssembly = Assembly.LoadFrom(path);
                }

                if (ironPythonAssembly == null)
                    throw new Exception();

                var pythonBindings = ironPythonAssembly.GetType("DynamoPython.PythonBindings");

                var pyBindingsProperty = pythonBindings.GetProperty("Bindings");
                var pyBindings = pyBindingsProperty.GetValue(null, null);

                var binding = ironPythonAssembly.GetType("DynamoPython.Binding");

                Func<string, object, object> createBinding =
                    (name, boundObject) =>
                        Activator.CreateInstance(binding, new[] { name, boundObject });

                Action<string, object> addToBindings =
                    (name, boundObject) =>
                        pyBindings.GetType()
                                  .InvokeMember(
                                      "Add", BindingFlags.InvokeMethod, null, pyBindings,
                                      new[] { createBinding(name, boundObject) });

                addToBindings("DynLog", new LogDelegate(dynSettings.Controller.DynamoViewModel.Log)); //Logging

                addToBindings(
                   "DynTransaction",
                   new Func<SubTransaction>(
                      delegate
                      {
                          if (!dynRevitSettings.Controller.IsTransactionActive())
                          {
                              dynRevitSettings.Controller.InitTransaction();
                          }
                          return new SubTransaction(dynRevitSettings.Doc.Document);
                      }));

                addToBindings("__revit__", dynRevitSettings.Doc.Application);
                addToBindings("__doc__", dynRevitSettings.Doc.Application.ActiveUIDocument.Document);

                var pythonEngine = ironPythonAssembly.GetType("DynamoPython.PythonEngine");
                var evaluatorField = pythonEngine.GetField("Evaluator");

                oldPyEval = evaluatorField.GetValue(null);

                //var x = PythonEngine.GetMembers();
                //foreach (var y in x)
                //    Console.WriteLine(y);

                var evalDelegateType = ironPythonAssembly.GetType("DynamoPython.PythonEngine+EvaluationDelegate");

                Delegate d = Delegate.CreateDelegate(
                    evalDelegateType,
                    this,
                    typeof(DynamoController_Revit)
                        .GetMethod("newEval", BindingFlags.NonPublic | BindingFlags.Instance));

                evaluatorField.SetValue(
                    null,
                    d);

                var drawingField = pythonEngine.GetField("Drawing");
                var drawDelegateType = ironPythonAssembly.GetType("DynamoPython.PythonEngine+DrawDelegate");
                Delegate draw = Delegate.CreateDelegate(
                    drawDelegateType,
                    this,
                    typeof(DynamoController_Revit)
                        .GetMethod("DrawPython", BindingFlags.NonPublic | BindingFlags.Instance));

                drawingField.SetValue(null, draw);

                // use this to pass into the python script a list of previously created elements from dynamo
                //TODO: ADD BACK IN
                //bindings.Add(new Binding("DynStoredElements", this.Elements));

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        void DrawPython(Value val, RenderDescription rd)
        {
            DrawContainers(val, rd);
        }

        private void DrawContainers(Value val, RenderDescription rd)
        {
            if (val.IsList)
            {
                foreach (Value v in ((Value.List)val).Item)
                {
                    DrawContainers(v, rd);
                }
            }
            if (val.IsContainer)
            {
                var drawable = ((Value.Container)val).Item;

                if(drawable is XYZ)
                {
                    dynRevitTransactionNode.DrawXYZ(rd, drawable);
                }
                else if (drawable is GeometryObject)
                {
                    dynRevitTransactionNode.DrawGeometryObject(rd, drawable);
                }
            }
        }

        Value newEval(bool dirty, string script, dynamic bindings)
        {
            bool transactionRunning = Transaction != null && Transaction.GetStatus() == TransactionStatus.Started;

            Value result = null;

            if (dynRevitSettings.Controller.InIdleThread)
                result = oldPyEval(dirty, script, bindings);
            else
            {
                result = IdlePromise<Value>.ExecuteOnIdle(
                   () => oldPyEval(dirty, script, bindings));
            }

            if (transactionRunning)
            {
                if (!IsTransactionActive())
                {
                    InitTransaction();
                }
                else
                {
                    var ts = Transaction.GetStatus();
                    if (ts != TransactionStatus.Started)
                    {
                        if (ts != TransactionStatus.RolledBack)
                            CancelTransaction();
                        InitTransaction();
                    }
                }
            }
            else if (DynamoViewModel.RunInDebug)
            {
                if (IsTransactionActive())
                    EndTransaction();
            }

            return result;
        }
        #endregion

        #region Watch Node Revit Hooks
        void AddWatchNodeHandler()
        {
            dynWatch.AddWatchHandler(new RevitElementWatchHandler());
        }

        private class RevitElementWatchHandler : WatchHandler
        {
            #region WatchHandler Members

            public bool AcceptsValue(object o)
            {
                return o is Element;
            }

            public void ProcessNode(object value, WatchNode node)
            {
                var element = value as Element;
                var id = element.Id;

                node.Clicked += delegate
                {
                    dynRevitSettings.Doc.ShowElements(element);
                };

                node.Link = id.ToString();
            }

            #endregion
        }
        #endregion

        public bool InIdleThread;

        private List<Autodesk.Revit.DB.ElementId> _transElements = new List<Autodesk.Revit.DB.ElementId>();

        private Dictionary<Autodesk.Revit.DB.ElementId, DynElementUpdateDelegate> _transDelElements
           = new Dictionary<Autodesk.Revit.DB.ElementId, DynElementUpdateDelegate>();

        internal void RegisterSuccessfulDeleteHook(Autodesk.Revit.DB.ElementId id, DynElementUpdateDelegate d)
        {
            this._transDelElements[id] = d;
        }

        private void CommitDeletions()
        {
            var delDict = new Dictionary<DynElementUpdateDelegate, List<Autodesk.Revit.DB.ElementId>>();
            foreach (var kvp in this._transDelElements)
            {
                if (!delDict.ContainsKey(kvp.Value))
                {
                    delDict[kvp.Value] = new List<Autodesk.Revit.DB.ElementId>();
                }
                delDict[kvp.Value].Add(kvp.Key);
            }

            foreach (var kvp in delDict)
                kvp.Key(kvp.Value);
        }

        internal void RegisterDeleteHook(Autodesk.Revit.DB.ElementId id, DynElementUpdateDelegate d)
        {
            DynElementUpdateDelegate del = delegate(List<Autodesk.Revit.DB.ElementId> deleted)
            {
                var valid = new List<Autodesk.Revit.DB.ElementId>();
                var invalid = new List<Autodesk.Revit.DB.ElementId>();
                foreach (var delId in deleted)
                {
                    try
                    {
                        Autodesk.Revit.DB.Element e = dynRevitSettings.Doc.Document.GetElement(delId);
                        if (e != null)
                        {
                            valid.Add(e.Id);
                        }
                        else
                            invalid.Add(delId);
                    }
                    catch
                    {
                        invalid.Add(delId);
                    }
                }
                valid.Clear();
                d(invalid);
                foreach (var invId in invalid)
                {
                    this.Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Modify);
                    this.Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Add);
                    this.Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Delete);
                }
            };

            DynElementUpdateDelegate mod = delegate(List<Autodesk.Revit.DB.ElementId> modded)
            {
                _transElements.RemoveAll(modded.Contains);

                foreach (var mid in modded)
                {
                    this.Updater.UnRegisterChangeHook(mid, ChangeTypeEnum.Modify);
                    this.Updater.UnRegisterChangeHook(mid, ChangeTypeEnum.Add);
                }
            };

            this.Updater.RegisterChangeHook(
               id, ChangeTypeEnum.Delete, del
            );
            this.Updater.RegisterChangeHook(
               id, ChangeTypeEnum.Modify, mod
            );
            this.Updater.RegisterChangeHook(
               id, ChangeTypeEnum.Add, mod
            );
            this._transElements.Add(id);
        }

        private Transaction _trans;
        public void InitTransaction()
        {
            if (_trans == null || _trans.GetStatus() != TransactionStatus.Started)
            {
                _trans = new Transaction(
                   dynRevitSettings.Doc.Document,
                   "Dynamo Script"
                );
                _trans.Start();

                FailureHandlingOptions failOpt = _trans.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(new DynamoWarningPrinter());
                _trans.SetFailureHandlingOptions(failOpt);
            }
        }

        public Transaction Transaction { get { return this._trans; } }

        public void EndTransaction()
        {
            if (_trans != null)
            {
                if (_trans.GetStatus() == TransactionStatus.Started)
                {
                    _trans.Commit();
                    _transElements.Clear();
                    CommitDeletions();
                    _transDelElements.Clear();
                }
                _trans = null;
            }
        }

        public void CancelTransaction()
        {
            if (_trans != null)
            {
                _trans.RollBack();
                _trans = null;
                this.Updater.RollBack(this._transElements);
                this._transElements.Clear();
                this._transDelElements.Clear();
            }
        }

        public bool IsTransactionActive()
        {
            return _trans != null;
        }

        protected override void OnRunCancelled(bool error)
        {
            base.OnRunCancelled(error);

            this.CancelTransaction();
        }

        protected override void OnEvaluationCompleted()
        {
            base.OnEvaluationCompleted();

            //Cleanup Delegate
            Action cleanup = delegate
            {
                this.InitTransaction(); //Initialize a transaction (if one hasn't been aleady)

                //Reset all elements
                foreach (var element in dynSettings.Controller.DynamoViewModel.AllNodes)
                {
                    if (element is dynRevitTransactionNode)
                        (element as dynRevitTransactionNode).ResetRuns();
                }

                //////
                /* FOR NON-DEBUG RUNS, THIS IS THE ACTUAL END POINT FOR DYNAMO TRANSACTION */
                //////

                this.EndTransaction(); //Close global transaction.
            };

            //If we're in a debug run or not already in the idle thread, then run the Cleanup Delegate
            //from the idle thread. Otherwise, just run it in this thread.
            if (dynSettings.Controller.DynamoViewModel.RunInDebug || !InIdleThread && !this.Testing)
            {
                IdlePromise.ExecuteOnIdle(cleanup, false);
            }
            else
                cleanup();
        }

        protected override void Run(IEnumerable<dynNodeModel> topElements, FScheme.Expression runningExpression)
        {

            //If we are not running in debug...
            if (!this.DynamoViewModel.RunInDebug)
            {
                //Do we need manual transaction control?
                bool manualTrans = topElements.Any((DynamoViewModel as DynamoRevitViewModel).CheckManualTransaction.TraverseUntilAny);

                //Can we avoid running everything in the Revit Idle thread?
                bool noIdleThread = manualTrans || 
                    !topElements.Any((DynamoViewModel as DynamoRevitViewModel).CheckRequiresTransaction.TraverseUntilAny);

                //If we don't need to be in the idle thread...
                if (noIdleThread || this.Testing)
                {
                    DynamoLogger.Instance.Log("Running expression in evaluation thread...");
                    (DynamoViewModel as DynamoRevitViewModel).TransMode = DynamoRevitViewModel.TransactionMode.Manual; //Manual transaction control

                    if(this.Testing)
                        (DynamoViewModel as DynamoRevitViewModel).TransMode = DynamoRevitViewModel.TransactionMode.Automatic;

                    this.InIdleThread = false; //Not in idle thread at the moment
                    base.Run(topElements, runningExpression); //Just run the Run Delegate
                }
                else //otherwise...
                {
                    DynamoLogger.Instance.Log("Running expression in Revit's Idle thread...");
                    (DynamoViewModel as DynamoRevitViewModel).TransMode = DynamoRevitViewModel.TransactionMode.Automatic; //Automatic transaction control

                    Debug.WriteLine("Adding a run to the idle stack.");
                    this.InIdleThread = true; //Now in the idle thread.
                    IdlePromise.ExecuteOnIdle(new Action(
                            () => base.Run(topElements, runningExpression)),
                            false); //Execute the Run Delegate in the Idle thread.
                    
                }
            }
            else //If we are in debug mode...
            {
                (DynamoViewModel as DynamoRevitViewModel).TransMode = DynamoRevitViewModel.TransactionMode.Debug; //Debug transaction control
                this.InIdleThread = true; //Everything will be evaluated in the idle thread.

                dynSettings.Controller.DynamoViewModel.Log("Running expression in debug.");

                //Execute the Run Delegate.
                base.Run(topElements, runningExpression);
            }
        }
    }

    public class DynamoWarningPrinter : Autodesk.Revit.DB.IFailuresPreprocessor
    {

        public DynamoWarningPrinter()
        {

        }

        public Autodesk.Revit.DB.FailureProcessingResult PreprocessFailures(Autodesk.Revit.DB.FailuresAccessor failuresAccessor)
        {
            var failList = failuresAccessor.GetFailureMessages();
            foreach (var fail in failList)
            {
                var severity = fail.GetSeverity();
                if (severity == Autodesk.Revit.DB.FailureSeverity.Warning)
                {
                    dynSettings.Controller.DynamoViewModel.Log(
                       "!! Warning: " + fail.GetDescriptionText()
                    );
                    failuresAccessor.DeleteWarning(fail);
                }
            }

            return Autodesk.Revit.DB.FailureProcessingResult.Continue;
        }
    }
}
