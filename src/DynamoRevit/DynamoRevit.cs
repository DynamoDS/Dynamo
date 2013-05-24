//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Data;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Dynamo.Applications.Properties;
using Dynamo.Controls;
using Dynamo.Utilities;
using IWin32Window = System.Windows.Interop.IWin32Window;
using MessageBox = System.Windows.Forms.MessageBox;
using Rectangle = System.Drawing.Rectangle;
using Dynamo.FSchemeInterop;
using Dynamo.Commands;
#if DEBUG
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Framework;
using NUnit.Util;
#endif

//MDJ needed for spatialfeildmanager
//TAF added to get strings from resource files

namespace Dynamo.Applications
{
    //MDJ - Added by Matt Jezyk - 10.27.2011
    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitApp : IExternalApplication
    {
        private static readonly string m_AssemblyName = Assembly.GetExecutingAssembly().Location;
        private static string m_AssemblyDirectory = Path.GetDirectoryName(m_AssemblyName);
        public static DynamoUpdater updater;
        private static ResourceManager res;
        public static ExecutionEnvironment env;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                //TAF load english_us TODO add a way to localize
                res = Resource_en_us.ResourceManager;
                // Create new ribbon panel
                RibbonPanel ribbonPanel = application.CreateRibbonPanel(res.GetString("App_Description"));

                //Create a push button in the ribbon panel 
                var pushButton = ribbonPanel.AddItem(new PushButtonData("Dynamo",
                                                                        res.GetString("App_Name"), m_AssemblyName,
                                                                        "Dynamo.Applications.DynamoRevit")) as
                                 PushButton;

                Bitmap dynamoIcon = Resources.Nodes_32_32;


                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    dynamoIcon.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                pushButton.LargeImage = bitmapSource;
                pushButton.Image = bitmapSource;

#if DEBUG
                var pushButton1 = ribbonPanel.AddItem(new PushButtonData("Test",
                                                                        res.GetString("App_Name"), m_AssemblyName,
                                                                        "Dynamo.Applications.DynamoRevitTester")) as PushButton;
                pushButton1.LargeImage = bitmapSource;
                pushButton1.Image = bitmapSource;
#endif

                // MDJ = element level events and dyanmic model update
                // MDJ 6-8-12  trying to get new dynamo to watch for user created ref points and re-run definition when they are moved

                IdlePromise.RegisterIdle(application);

                updater = new DynamoUpdater(application.ActiveAddInId, application.ControlledApplication);
                if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
                    UpdaterRegistry.RegisterUpdater(updater);

                var SpatialFieldFilter = new ElementClassFilter(typeof (SpatialFieldManager));
                var familyFilter = new ElementClassFilter(typeof (FamilyInstance));
                var refPointFilter = new ElementCategoryFilter(BuiltInCategory.OST_ReferencePoints);
                var modelCurveFilter = new ElementClassFilter(typeof (CurveElement));
                var sunFilter = new ElementClassFilter(typeof (SunAndShadowSettings));
                IList<ElementFilter> filterList = new List<ElementFilter>();

                filterList.Add(SpatialFieldFilter);
                filterList.Add(familyFilter);
                filterList.Add(modelCurveFilter);
                filterList.Add(refPointFilter);
                filterList.Add(sunFilter);

                ElementFilter filter = new LogicalOrFilter(filterList);

                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementDeletion());
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());

                env = new ExecutionEnvironment();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevit : IExternalCommand
    {
        private static DynamoView dynamoBench;
        private UIDocument m_doc;
        private UIApplication m_revit;


        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            if (dynamoBench != null)
            {
                dynamoBench.Focus();
                return Result.Succeeded;
            }

            DynamoLogger.Instance.StartLogging();

            try
            {
                m_revit = revit.Application;
                m_doc = m_revit.ActiveUIDocument;

                #region default level

                Level defaultLevel = null;
                var fecLevel = new FilteredElementCollector(m_doc.Document);
                fecLevel.OfClass(typeof (Level));
                defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                dynRevitSettings.Revit = m_revit;
                dynRevitSettings.Doc = m_doc;
                dynRevitSettings.DefaultLevel = defaultLevel;

                IdlePromise.ExecuteOnIdle(delegate
                    {
                        //get window handle
                        IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                        //show the window

                        string context = string.Format("{0} {1}", m_revit.Application.VersionName, m_revit.Application.VersionNumber);
                        var dynamoController = new DynamoController_Revit(DynamoRevitApp.env, DynamoRevitApp.updater, true, typeof(DynamoRevitViewModel), context);
                        dynamoBench = dynSettings.Bench;

                        //set window handle and show dynamo
                        new WindowInteropHelper(dynamoBench).Owner = mwHandle;

                        dynamoBench.WindowStartupLocation = WindowStartupLocation.Manual;

                        Rectangle bounds = Screen.PrimaryScreen.Bounds;
                        dynamoBench.Left = bounds.X;
                        dynamoBench.Top = bounds.Y;
                        dynamoBench.Loaded += dynamoForm_Loaded;

                        dynamoBench.Show();

                        dynamoBench.Closed += dynamoForm_Closed;
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
                DynamoLogger.Instance.Log("Dynamo log ended " + DateTime.Now.ToString());

                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void dynamoForm_Closed(object sender, EventArgs e)
        {
            IdlePromise.ClearPromises();
            dynamoBench = null;
        }

        private void dynamoForm_Loaded(object sender, RoutedEventArgs e)
        {
            ((DynamoView) sender).WindowState = WindowState.Maximized;
        }
    }

#if DEBUG

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevitTester : IExternalCommand
    {
        private UIDocument m_doc;
        private UIApplication m_revit;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            DynamoLogger.Instance.StartLogging();

            try
            {
                m_revit = revit.Application;
                m_doc = m_revit.ActiveUIDocument;

                #region default level

                Level defaultLevel = null;
                var fecLevel = new FilteredElementCollector(m_doc.Document);
                fecLevel.OfClass(typeof(Level));
                defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                dynRevitSettings.Revit = m_revit;
                dynRevitSettings.Doc = m_doc;
                dynRevitSettings.DefaultLevel = defaultLevel;

                IdlePromise.ExecuteOnIdle(delegate
                {
                    //get window handle
                    IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                    //create dynamo
                    string context = string.Format("{0} {1}", m_revit.Application.VersionName, m_revit.Application.VersionNumber);
                    var dynamoController = new DynamoController_Revit(DynamoRevitApp.env, DynamoRevitApp.updater, false, typeof(DynamoRevitViewModel), context);
                
                    //flag to run evalauation synchronously, helps to 
                    //avoid threading issues when testing.
                    dynamoController.RunEvaluationSynchronously = true;

                    //execute the tests
                    //http://stackoverflow.com/questions/2798561/how-to-run-nunit-from-my-code
                    string assLocation = Assembly.GetExecutingAssembly().Location;
                    FileInfo fi = new FileInfo(assLocation);
                    string testLoc = Path.Combine(fi.DirectoryName, @"DynamoRevitTests.dll");

                    //Tests must be executed on the main thread in order to access the Revit API.
                    //NUnit's SimpleTestRunner runs the tests on the main thread
                    //http://stackoverflow.com/questions/16216011/nunit-c-run-specific-tests-through-coding?rq=1
                    CoreExtensions.Host.InitializeService();
                    SimpleTestRunner runner = new SimpleTestRunner();
                    TestSuiteBuilder builder = new TestSuiteBuilder();
                    TestPackage package = new TestPackage("DynamoRevitTests", new List<string>() { testLoc });
                    runner.Load(package);
                    TestSuite suite = builder.Build(package);
                    TestFixture fixture = null;
                    FindFixtureByName(out fixture, suite, "DynamoRevitTests");
                    if (fixture == null)
                        throw new Exception("Could not find DynamoRevitTests fixture.");

                    foreach (TestMethod t in fixture.Tests)
                    {
                        TestName testName = t.TestName;
                        TestFilter filter = new NameFilter(testName);
                        TestResult result = t.Run(new RevitTestEventListener(), filter);
                        ResultSummarizer summ = new ResultSummarizer(result);
                        Assert.AreEqual(1, summ.ResultCount);
                    }
                });

                IdlePromise.ExecuteOnIdle(delegate
                {
                    DynamoLogger.Instance.FinishLogging();
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void FindFixtureByName(out TestFixture fixture, TestSuite suite, string name)
        {
            foreach (TestSuite innerSuite in suite.Tests)
            {
                if (innerSuite is TestFixture)
                {
                    if (((TestFixture)innerSuite).TestName.Name == name)
                    {
                        fixture = innerSuite as TestFixture;
                        return;
                    }
                }
                else
                {
                    FindFixtureByName(out fixture, innerSuite, name);
                    if (fixture != null)
                        return;
                }
            }

            fixture = null;
        }
    }


    //http://sqa.stackexchange.com/questions/2880/nunit-global-error-method-event-for-handling-exceptions
    /// <summary>
    /// Listens for test events and logs information
    /// </summary>
    class RevitTestEventListener : EventListener
    {
        public void RunStarted(string name, int testCount) {}
        public void RunFinished(TestResult result) { }
        public void RunFinished(Exception exception) { }
        public void TestStarted(TestName testName) 
        {
            DynamoLogger.Instance.Log(string.Format("Starting test {0}", testName.Name));
        }
        public void TestFinished(TestResult result) 
        {
            if (result.Executed && result.IsFailure)
            {
                DynamoLogger.Instance.Log(string.Format("Test FAILED : {0}", result.Message));
            }
            else if (result.Executed && result.IsSuccess)
                DynamoLogger.Instance.Log("Test PASS");
            else if (result.Executed && result.IsError)
                DynamoLogger.Instance.Log("Test ERROR");
        }
        public void SuiteStarted(TestName testName) { }
        public void SuiteFinished(TestResult result) { }
        public void UnhandledException(Exception exception) 
        {
            DynamoLogger.Instance.Log(exception.Message);
        }
        public void TestOutput(TestOutput testOutput) { }
    }
#endif

    internal class WindowHandle : IWin32Window
    {
        private readonly IntPtr _hwnd;

        public WindowHandle(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h,
                         "expected non-null window handle");

            _hwnd = h;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }
    }
}