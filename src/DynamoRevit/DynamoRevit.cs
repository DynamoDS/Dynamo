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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using Dynamo.NUnit.Tests;
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

#if DEBUG
using NUnit.Core;
using NUnit.Core.Filters;
#endif

namespace Dynamo.Applications
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitApp : IExternalApplication
    {
        private static readonly string m_AssemblyName = Assembly.GetExecutingAssembly().Location;
        public static DynamoUpdater Updater;
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

                Updater = new DynamoUpdater(application.ActiveAddInId, application.ControlledApplication);
                if (!UpdaterRegistry.IsUpdaterRegistered(Updater.GetUpdaterId()))
                    UpdaterRegistry.RegisterUpdater(Updater);

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

                UpdaterRegistry.AddTrigger(Updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
                UpdaterRegistry.AddTrigger(Updater.GetUpdaterId(), filter, Element.GetChangeTypeElementDeletion());
                UpdaterRegistry.AddTrigger(Updater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());

                env = new ExecutionEnvironment();
                //EnsureApplicationResources();

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
            UpdaterRegistry.UnregisterUpdater(Updater.GetUpdaterId());

            //if(Application.Current != null)
            //    Application.Current.Shutdown();

            return Result.Succeeded;
        }
    }

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevit : IExternalCommand
    {
        private static DynamoView dynamoView;
        private UIDocument m_doc;
        private UIApplication m_revit;
        private DynamoController dynamoController;
        private static bool isRunning = false;
        public static double? dynamoViewX = null;
        public static double? dynamoViewY = null;
        public static double? dynamoViewWidth = null;
        public static double? dynamoViewHeight = null;
        private bool handledCrash = false;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            //When a user double-clicks the Dynamo icon, we need to make
            //sure that we don't create another instance of Dynamo.
            if (isRunning)
            {
                Debug.WriteLine("Dynamo is already running.");
                if (dynamoView != null)
                {
                    dynamoView.Focus();
                }
                return Result.Succeeded;
            }

            isRunning = true;

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

                        Regex r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
                        string context = r.Replace(m_revit.Application.VersionName, "");

                        //they changed the application version name conventions for vasari
                        //it no longer has a version year so we can't compare it to other versions
                        //TODO:come up with a more stable way to test for Vasari beta 3
                        if (context == "Vasari")
                            context = "Vasari 2014";

                        dynamoController = new DynamoController_Revit(DynamoRevitApp.env, DynamoRevitApp.Updater, typeof(DynamoRevitViewModel), context);
                        

                        dynamoView = new DynamoView { DataContext = dynamoController.DynamoViewModel };
                        dynamoController.UIDispatcher = dynamoView.Dispatcher;

                        //set window handle and show dynamo
                        new WindowInteropHelper(dynamoView).Owner = mwHandle;

                        handledCrash = false;

                        dynamoView.WindowStartupLocation = WindowStartupLocation.Manual;

                        Rectangle bounds = Screen.PrimaryScreen.Bounds;
                        dynamoView.Left = dynamoViewX ?? bounds.X;
                        dynamoView.Top = dynamoViewY ?? bounds.Y;
                        dynamoView.Width = dynamoViewWidth ?? 1000.0;
                        dynamoView.Height = dynamoViewHeight ?? 800.0;

                        dynamoView.Show();

                        dynamoView.Dispatcher.UnhandledException -= DispatcherOnUnhandledException; 
                        dynamoView.Dispatcher.UnhandledException += DispatcherOnUnhandledException; 
                        dynamoView.Closing += dynamoView_Closing;
                        dynamoView.Closed += dynamoView_Closed;

                    });
            }
            catch (Exception ex)
            {
                isRunning = false;
                MessageBox.Show(ex.ToString());

                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
                DynamoLogger.Instance.Log("Dynamo log ended " + DateTime.Now.ToString());

                return Result.Failed;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// A method to deal with unhandle exceptions.  Executes right before Revit crashes.
        /// Dynamo is still valid at this time, but further work may cause corruption.  Here, 
        /// we run the ExitCommand, allowing the user to save all of their work.  Then, we send them
        /// to the issues page on Github. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Info about the exception</param>
        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;

            // only handle a single crash per Dynamo sesh, this should be reset in the initial command
            if (handledCrash)
            {
                return;
            }

            handledCrash = true;

            var exceptionMessage = args.Exception.Message;

            try
            {
                DynamoLogger.Instance.Log("Dynamo Unhandled Exception");
                DynamoLogger.Instance.Log(exceptionMessage);
            }
            catch
            {

            }

            try
            {
                dynSettings.Controller.OnRequestsCrashPrompt(this, args);
                dynSettings.Controller.DynamoViewModel.Exit(false); // don't allow cancellation
            }
            catch
            {

            }
            finally
            {
                args.Handled = true;
            }
            
        }

        /// <summary>
        /// Executes right before Dynamo closes, gives you the chance to cache whatever you might want.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closing(object sender, EventArgs e)
        {
            // cache the size of the window for later reloading
            dynamoViewX = dynamoView.Left;
            dynamoViewY = dynamoView.Top;
            dynamoViewWidth = dynamoView.ActualWidth;
            dynamoViewHeight = dynamoView.ActualHeight;
            IdlePromise.ClearPromises();
        }

        /// <summary>
        /// Executes after Dynamo closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closed(object sender, EventArgs e)
        {
            dynamoView = null;
            isRunning = false;
        }
    }

#if DEBUG

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    internal class DynamoRevitTester : IExternalCommand
    {
        private UIDocument m_doc;
        private UIApplication m_revit;
        private resultType testResult;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            DynamoLogger.Instance.StartLogging();

            // Get the StringStringMap class which can write support into.
            IDictionary<string, string> dataMap = revit.JournalData;

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

                //create dynamo
                Regex r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
                string context = r.Replace(m_revit.Application.VersionName, "");

                var dynamoController = new DynamoController_Revit(DynamoRevitApp.env, DynamoRevitApp.Updater, typeof(DynamoRevitViewModel), context);

                //flag to run evalauation synchronously, helps to 
                //avoid threading issues when testing.
                dynamoController.Testing = true;

                //http://stackoverflow.com/questions/2798561/how-to-run-nunit-from-my-code
                string assLocation = Assembly.GetExecutingAssembly().Location;
                var fi = new FileInfo(assLocation);
                string testLoc = Path.Combine(fi.DirectoryName, @"DynamoRevitTester.dll");

                //Tests must be executed on the main thread in order to access the Revit API.
                //NUnit's SimpleTestRunner runs the tests on the main thread
                //http://stackoverflow.com/questions/16216011/nunit-c-run-specific-tests-through-coding?rq=1
                CoreExtensions.Host.InitializeService();
                var runner = new SimpleTestRunner();
                var builder = new TestSuiteBuilder();
                var package = new TestPackage("DynamoRevitTests", new List<string>() { testLoc });
                runner.Load(package);
                TestSuite suite = builder.Build(package);
                TestFixture fixture = null;
                FindFixtureByName(out fixture, suite, "DynamoRevitTests");
                if (fixture == null)
                    throw new Exception("Could not find DynamoRevitTests fixture.");

                //foreach (var t in fixture.Tests)
                //{
                //    if (t is ParameterizedMethodSuite)
                //    {
                //        var paramSuite = t as ParameterizedMethodSuite;
                //        foreach (var tInner in paramSuite.Tests)
                //        {
                //            if (tInner is TestMethod)
                //                Results.Results.Add(new DynamoRevitTest(tInner as TestMethod));
                //        }
                //    }
                //    else if (t is TestMethod)
                //        Results.Results.Add(new DynamoRevitTest(t as TestMethod));
                //}

                InitializeResults();

                var cases = testResult.testsuite.results.Items.ToList();

                //for testing
                //if the journal file contains data
                bool canReadData = (0 < dataMap.Count) ? true : false;
                if (canReadData)
                {
                    TestMethod t = FindTestByName(fixture, dataMap["dynamoTestName"]);
                    if (t != null)
                    {
                        TestFilter filter = new NameFilter(t.TestName);
                        var result = (t as TestMethod).Run(new TestListener(), filter);

                        //result types
                        //Ignored, Failure, NotRunnable, Error, Success

                        var testCase = new testcaseType();
                        testCase.name = t.TestName.Name;
                        testCase.executed = result.Executed.ToString();
                        testCase.success = result.IsSuccess.ToString();
                        testCase.asserts = result.AssertCount.ToString(CultureInfo.InvariantCulture);
                        testCase.time = result.Time.ToString(CultureInfo.InvariantCulture);
                        testResult.testsuite.success = true.ToString();

                        var currAsserts = Convert.ToInt16(testResult.testsuite.asserts);
                        testResult.testsuite.asserts = (currAsserts + result.AssertCount).ToString();

                        var currCount = Convert.ToInt16(testResult.total);
                        testResult.total = (currCount + 1);

                        if (result.IsSuccess)
                        {
                            testCase.result = "Success";
                        }
                        else if (result.IsFailure)
                        {
                            var fail = new failureType();
                            fail.message = result.Message;
                            fail.stacktrace = result.StackTrace;
                            testCase.Item = fail;
                            testCase.result = "Failure";
                            testResult.testsuite.success = false.ToString();
                            testResult.testsuite.result = "Failure";
                        }
                        else if (result.IsError)
                        {
                            var errCount = Convert.ToInt16(testResult.errors);
                            testResult.errors = (errCount + 1);
                            testCase.result = "Error";
                            testResult.testsuite.result = "Failure";
                        }

                        
                        cases.Add(testCase);
                    }
                }

                testResult.testsuite.results.Items = cases.ToArray();

                SaveResults();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Sets up an NUNit ResultsType object or deserializing and existing one.
        /// </summary>
        private void InitializeResults()
        {
            //read the existing results and add to them
            string resultsDir = Path.GetDirectoryName(dynRevitSettings.Doc.Document.PathName);
            string resultPath = Path.Combine(resultsDir,
                                              "DynamoRevitTestResults.xml");

            if (File.Exists(resultPath))
            {
                //read from the file
                var x = new XmlSerializer(typeof (resultType));
                using (var sr = new StreamReader(resultPath))
                {
                    testResult = (resultType)x.Deserialize(sr);
                }
            }
            else
            {
                //create one result to dump everything into
                testResult = new resultType();
                testResult.name = Assembly.GetExecutingAssembly().Location;

                var suite = new testsuiteType();
                suite.name = "DynamoRevitTests";
                suite.description = "Dynamo tests on Revit.";
                suite.time = "0.0";
                suite.type = "TestFixture";
                suite.result = "Success";
                suite.executed = "True";

                testResult.testsuite = suite;
                testResult.testsuite.results = new resultsType();
                testResult.testsuite.results.Items = new object[]{};

                testResult.date = DateTime.Now.ToString("yyyy-MM-dd");
                testResult.time = DateTime.Now.ToString("HH:mm:ss");
                testResult.failures = 0;
                testResult.ignored = 0;
                testResult.notrun = 0;
                testResult.errors = 0;
                testResult.skipped = 0;
                testResult.inconclusive = 0;
                testResult.invalid = 0;
            }
        }

        /// <summary>
        /// Serializes the results to an NUnit compatible xml file.
        /// </summary>
        private void SaveResults()
        {
            string resultsDir = Path.GetDirectoryName(dynRevitSettings.Doc.Document.PathName);
            string resultPath = Path.Combine(resultsDir,
                                              "DynamoRevitTestResults.xml");

            //write to the file
            var x = new XmlSerializer(typeof(resultType));
            using (var tw = XmlWriter.Create(resultPath, new XmlWriterSettings(){Indent = true}))
            {
                tw.WriteComment("This file represents the results of running a test suite");
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                x.Serialize(tw, testResult, ns);
            }
        }
        
        /// <summary>
        /// Find an NUnit test fixture by name.
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="suite"></param>
        /// <param name="name"></param>
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
        
        /// <summary>
        /// Find an NUnit test method within a given fixture by name.
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private TestMethod FindTestByName(TestFixture fixture, string name)
        {
            foreach (var t in fixture.Tests)
            {
                if (t is TestMethod)
                {
                    if ((t as TestMethod).TestName.Name == name)
                    {
                        return t as TestMethod;
                    }
                }      
            }
            return null;
        }
    }

    public class TestListener : EventListener
    {
        public TestListener() { }
        public void RunStarted(string name, int testCount) { }
        public void RunFinished(TestResult result) { }
        public void RunFinished(Exception exception) { }
        public void TestStarted(TestName testName){}
        public void TestFinished(TestResult result){}
        public void SuiteStarted(TestName testName) { }
        public void SuiteFinished(TestResult result) { }
        public void UnhandledException(Exception exception){}
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